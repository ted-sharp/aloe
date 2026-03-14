// <copyright file="PostalCodeImportHandler.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Models;
using Aloe.Apps.CsvImporterLib.Services;
using Npgsql;

namespace Aloe.Apps.CsvImporterLib.PostalCode;

/// <summary>
/// 日本郵便 郵便番号データの CSV インポートハンドラー。
/// </summary>
public sealed class PostalCodeImportHandler : IImportHandler
{
    private const string FullUrl = "https://www.post.japanpost.jp/zipcode/dl/utf/zip/utf_ken_all.zip";
    private const string DeltaAddUrlTemplate = "https://www.post.japanpost.jp/zipcode/dl/utf/zip/utf_add_{0}.zip";
    private const string DeltaDelUrlTemplate = "https://www.post.japanpost.jp/zipcode/dl/utf/zip/utf_del_{0}.zip";
    private const string StagingTable = "ext.postal_codes_staged";

    private const string SqlFull = """
        TRUNCATE ext.postal_codes;
        DROP INDEX IF EXISTS ext.postal_codes_ix1;

        INSERT INTO ext.postal_codes (postal_code, prefecture_katakana, city_katakana, town_katakana, prefecture, city, town)
        SELECT postal_code7, prefecture_katakana, city_katakana, town_katakana, prefecture, city, town
        FROM ext.postal_codes_staged;

        CREATE INDEX postal_codes_ix1 ON ext.postal_codes (postal_code7);
        """;

    private const string SqlDeltaAdd = """
        INSERT INTO ext.postal_codes (postal_code, prefecture_katakana, city_katakana, town_katakana, prefecture, city, town)
        SELECT postal_code7, prefecture_katakana, city_katakana, town_katakana, prefecture, city, town
        FROM ext.postal_codes_staged;
        """;

    private const string SqlDeltaDel = """
        DELETE FROM ext.postal_codes
        WHERE postal_code IN (SELECT postal_code7 FROM ext.postal_codes_staged);
        """;

    private readonly ISourceFetcher _fetcher;
    private readonly ICsvBulkLoader _loader;
    private readonly ImportRunRepository _repository;
    private readonly string _connectionString;

    /// <summary>
    /// コンストラクター。
    /// </summary>
    public PostalCodeImportHandler(
        ISourceFetcher fetcher,
        ICsvBulkLoader loader,
        ImportRunRepository repository,
        string connectionString)
    {
        _fetcher = fetcher;
        _loader = loader;
        _repository = repository;
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public string HandlerKey => "postal-code";

    /// <inheritdoc />
    public async Task<ImportResult> RunAsync(
        ImportOptions options,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;

        var mode = options.Mode == ImportMode.Auto ? ImportMode.Full : options.Mode;
        var runId = await _repository.BeginRunAsync(HandlerKey, mode.ToString().ToLowerInvariant(), cancellationToken);

        try
        {
            long rowsLoaded = mode == ImportMode.Full
                ? await RunFullAsync(options.WorkDir, progress, cancellationToken)
                : await RunDeltaAsync(
                    options.Yymm ?? throw new ArgumentException("Delta モードでは --yymm が必要です。"),
                    options.WorkDir,
                    progress,
                    cancellationToken);

            await _repository.CompleteRunAsync(runId, rowsLoaded, cancellationToken);
            return new ImportResult(true, rowsLoaded, startedAt, DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            await _repository.FailRunAsync(runId, ex.Message, cancellationToken);
            return new ImportResult(false, 0, startedAt, DateTimeOffset.UtcNow, ex.Message);
        }
    }

    private async Task<long> RunFullAsync(string? workDir, IProgress<ImportProgress>? progress, CancellationToken cancellationToken)
    {
        await TruncateStagingAsync(cancellationToken);

        var downloadProgress = progress is null ? null : new Progress<int>(pct =>
            progress.Report(new ImportProgress(ImportProgressStage.Downloading, pct)));

        using var csvStream = await _fetcher.FetchAsync(FullUrl, "*.csv", downloadProgress, workDir, cancellationToken);

        progress?.Report(new ImportProgress(ImportProgressStage.Importing));
        var rowsLoaded = await _loader.LoadAsync(_connectionString, StagingTable, csvStream, Encoding.UTF8, cancellationToken);

        await ExecuteSqlAsync(SqlFull, cancellationToken);
        await TruncateStagingAsync(cancellationToken);

        return rowsLoaded;
    }

    private async Task<long> RunDeltaAsync(string yymm, string? workDir, IProgress<ImportProgress>? progress, CancellationToken cancellationToken)
    {
        // 追加・更新
        await TruncateStagingAsync(cancellationToken);
        var addUrl = string.Format(CultureInfo.InvariantCulture, DeltaAddUrlTemplate, yymm);

        var downloadProgress = progress is null ? null : new Progress<int>(pct =>
            progress.Report(new ImportProgress(ImportProgressStage.Downloading, pct)));

        using var addStream = await _fetcher.FetchAsync(addUrl, "*.csv", downloadProgress, workDir, cancellationToken);

        progress?.Report(new ImportProgress(ImportProgressStage.Importing));
        var rowsAdded = await _loader.LoadAsync(_connectionString, StagingTable, addStream, Encoding.UTF8, cancellationToken);
        await ExecuteSqlAsync(SqlDeltaAdd, cancellationToken);
        await TruncateStagingAsync(cancellationToken);

        // 削除
        var delUrl = string.Format(CultureInfo.InvariantCulture, DeltaDelUrlTemplate, yymm);
        using var delStream = await _fetcher.FetchAsync(delUrl, "*.csv", cancellationToken: cancellationToken);
        await _loader.LoadAsync(_connectionString, StagingTable, delStream, Encoding.UTF8, cancellationToken);
        await ExecuteSqlAsync(SqlDeltaDel, cancellationToken);
        await TruncateStagingAsync(cancellationToken);

        return rowsAdded;
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
