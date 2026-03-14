// <copyright file="MedisHotImportHandler.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.IO.Compression;
using System.Text;
using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Models;
using Aloe.Apps.CsvImporterLib.Services;
using Npgsql;

namespace Aloe.Apps.CsvImporterLib.MedisCodes;

/// <summary>
/// MEDIS HOT13薬品コードの CSV インポートハンドラー。
/// </summary>
public sealed class MedisHotImportHandler : IImportHandler
{
    private const string StagingTable = "ext.raw_hot13_codes";

    private const string SqlFull = """
        TRUNCATE ext.hot13_codes;

        INSERT INTO ext.hot13_codes
        SELECT * FROM ext.raw_hot13_codes;

        TRUNCATE ext.raw_hot13_codes;
        """;

    private readonly ICsvBulkLoader _loader;
    private readonly ImportRunRepository _repository;
    private readonly string _connectionString;

    /// <summary>
    /// コンストラクター。
    /// </summary>
    public MedisHotImportHandler(
        ICsvBulkLoader loader,
        ImportRunRepository repository,
        string connectionString)
    {
        _loader = loader;
        _repository = repository;
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public string HandlerKey => "medis-hot";

    /// <inheritdoc />
    public async Task<ImportResult> RunAsync(
        ImportOptions options,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;

        if (options.Mode == ImportMode.Delta)
        {
            throw new NotSupportedException("MEDIS HOT13 インポートは Full モードのみサポートしています。");
        }

        if (string.IsNullOrWhiteSpace(options.SourcePath))
        {
            throw new ArgumentException("MEDIS HOT13 インポートには --source でローカルZIPファイルパスを指定してください。");
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

        using var csvStream = OpenCsvSkippingHeaderFromZip(sourcePath);
        var rowsLoaded = await _loader.LoadAsync(_connectionString, StagingTable, csvStream, Encoding.GetEncoding(932), cancellationToken);

        await ExecuteSqlAsync(SqlFull, cancellationToken);

        return rowsLoaded;
    }

    private static Stream OpenCsvSkippingHeaderFromZip(string zipPath)
    {
        var archive = ZipFile.OpenRead(zipPath);
        var csvEntry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"ZIP ファイル '{zipPath}' 内に CSV ファイルが見つかりません。");

        var entryStream = csvEntry.Open();
        SkipFirstLine(entryStream);
        return new ZipEntryStream(entryStream, archive);
    }

    private static void SkipFirstLine(Stream stream)
    {
        // SJIS では 0x0A (LF) はシングルバイト文字のため、バイト単位でスキップ可能
        int b;
        while ((b = stream.ReadByte()) != -1 && b != '\n')
        {
        }
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
}
