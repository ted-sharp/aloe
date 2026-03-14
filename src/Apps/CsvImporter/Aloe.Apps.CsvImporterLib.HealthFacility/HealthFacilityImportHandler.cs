// <copyright file="HealthFacilityImportHandler.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.IO.Compression;
using System.Text;
using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Models;
using Aloe.Apps.CsvImporterLib.Services;
using Npgsql;

namespace Aloe.Apps.CsvImporterLib.HealthFacility;

/// <summary>
/// 特定健診実施機関の CSV インポートハンドラー。
/// ローカルの ZIP ファイルまたは CSV ファイルを受け付ける（SJIS・ヘッダーあり）。
/// </summary>
public sealed class HealthFacilityImportHandler : IImportHandler
{
    private const string StagingTable = "ext.raw_special_health_facility_codes";

    private const string SqlFull = """
        TRUNCATE ext.facility_codes;

        INSERT INTO ext.facility_codes
        SELECT * FROM ext.raw_special_health_facility_codes;

        TRUNCATE ext.raw_special_health_facility_codes;
        """;

    private readonly ICsvBulkLoader _loader;
    private readonly ImportRunRepository _repository;
    private readonly string _connectionString;

    /// <summary>
    /// コンストラクター。
    /// </summary>
    public HealthFacilityImportHandler(
        ICsvBulkLoader loader,
        ImportRunRepository repository,
        string connectionString)
    {
        _loader = loader;
        _repository = repository;
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public string HandlerKey => "health-facility";

    /// <inheritdoc />
    public async Task<ImportResult> RunAsync(
        ImportOptions options,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;

        if (options.Mode == ImportMode.Delta)
        {
            throw new NotSupportedException("特定健診実施機関 インポートは Full モードのみサポートしています。");
        }

        if (string.IsNullOrWhiteSpace(options.SourcePath))
        {
            throw new ArgumentException("特定健診実施機関 インポートには --source でローカル ZIP または CSV ファイルパスを指定してください。");
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

        var sjis = Encoding.GetEncoding(932);
        long rowsLoaded;

        if (sourcePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            using var csvStream = OpenCsvSkippingHeaderFromZip(sourcePath);
            rowsLoaded = await _loader.LoadAsync(_connectionString, StagingTable, csvStream, sjis, cancellationToken);
        }
        else
        {
            await using var fileStream = File.OpenRead(sourcePath);
            SkipFirstLine(fileStream);
            rowsLoaded = await _loader.LoadAsync(_connectionString, StagingTable, fileStream, sjis, cancellationToken);
        }

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
