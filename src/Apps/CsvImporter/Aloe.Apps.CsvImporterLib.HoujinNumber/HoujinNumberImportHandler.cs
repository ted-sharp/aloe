// <copyright file="HoujinNumberImportHandler.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.IO.Compression;
using System.Text;
using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Models;
using Aloe.Apps.CsvImporterLib.Services;
using Npgsql;

namespace Aloe.Apps.CsvImporterLib.HoujinNumber;

/// <summary>
/// 国税庁 法人番号データの CSV インポートハンドラー。
/// </summary>
public sealed class HoujinNumberImportHandler : IImportHandler
{
    private const string StagingTable = "ext.raw_houjin_numbers";

    private const string SqlFull = """
        TRUNCATE ext.houjin_numbers;
        DROP INDEX IF EXISTS ext.houjin_numbers_ix1;

        INSERT INTO ext.houjin_numbers (corporate_number, name, postal_code, prefecture_name, city_name, street_number)
        SELECT corporate_number, name, post_code, prefecture_name, city_name, street_number
        FROM ext.raw_houjin_numbers
        WHERE prefecture_name IS NOT NULL
          AND post_code IS NOT NULL
          AND close_date IS NULL
          AND hihyoji = '0';

        CREATE INDEX houjin_numbers_ix1 ON ext.houjin_numbers (corporate_number);
        """;

    private readonly ICsvBulkLoader _loader;
    private readonly ImportRunRepository _repository;
    private readonly string _connectionString;

    /// <summary>
    /// コンストラクター。
    /// </summary>
    public HoujinNumberImportHandler(
        ICsvBulkLoader loader,
        ImportRunRepository repository,
        string connectionString)
    {
        _loader = loader;
        _repository = repository;
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public string HandlerKey => "houjin-number";

    /// <inheritdoc />
    public async Task<ImportResult> RunAsync(
        ImportOptions options,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;

        if (options.Mode == ImportMode.Delta)
        {
            throw new NotSupportedException("法人番号インポートは Full モードのみサポートしています。");
        }

        if (string.IsNullOrWhiteSpace(options.SourcePath))
        {
            throw new ArgumentException("法人番号インポートには --source でローカルZIPファイルパスを指定してください。");
        }

        var runId = await _repository.BeginRunAsync(HandlerKey, "full", cancellationToken);

        try
        {
            progress?.Report(new ImportProgress(ImportProgressStage.Importing));
            var rowsLoaded = await RunFullAsync(options.SourcePath, cancellationToken);

            await _repository.CompleteRunAsync(runId, rowsLoaded, cancellationToken);
            return new ImportResult(true, rowsLoaded, startedAt, DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            await _repository.FailRunAsync(runId, ex.Message, cancellationToken);
            return new ImportResult(false, 0, startedAt, DateTimeOffset.UtcNow, ex.Message);
        }
    }

    private async Task<long> RunFullAsync(string sourcePath, CancellationToken cancellationToken)
    {
        await TruncateStagingAsync(cancellationToken);

        using var csvStream = OpenCsvFromZip(sourcePath);
        var rowsLoaded = await _loader.LoadAsync(_connectionString, StagingTable, csvStream, Encoding.UTF8, cancellationToken);

        await ExecuteSqlAsync(SqlFull, cancellationToken);
        await TruncateStagingAsync(cancellationToken);

        return rowsLoaded;
    }

    private static Stream OpenCsvFromZip(string zipPath)
    {
        var archive = ZipFile.OpenRead(zipPath);
        var csvEntry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"ZIP ファイル '{zipPath}' 内に CSV ファイルが見つかりません。");

        // ZipArchive はストリームが閉じられるまで生存させる必要があるため、
        // ラッパーストリームで archive の Dispose を管理する。
        var entryStream = csvEntry.Open();
        return new ZipEntryStream(entryStream, archive);
    }

    private async Task TruncateStagingAsync(CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand($"TRUNCATE {StagingTable}", conn);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task ExecuteSqlAsync(string sql, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// ZipArchive のエントリストリームをラップし、Dispose 時に ZipArchive も解放するストリーム。
    /// </summary>
    private sealed class ZipEntryStream : Stream
    {
        private readonly Stream _inner;
        private readonly ZipArchive _archive;

        public ZipEntryStream(Stream inner, ZipArchive archive)
        {
            _inner = inner;
            _archive = archive;
        }

        public override bool CanRead => _inner.CanRead;

        public override bool CanSeek => _inner.CanSeek;

        public override bool CanWrite => _inner.CanWrite;

        public override long Length => _inner.Length;

        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override void Flush() => _inner.Flush();

        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

        public override void SetLength(long value) => _inner.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => _inner.ReadAsync(buffer, offset, count, cancellationToken);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => _inner.ReadAsync(buffer, cancellationToken);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
                _archive.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
