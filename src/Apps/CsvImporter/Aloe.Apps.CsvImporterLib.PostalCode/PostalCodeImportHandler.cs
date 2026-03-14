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
    public async Task<ImportResult> RunAsync(ImportOptions options, CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;

        await _repository.EnsureTablesAsync(cancellationToken);
        await EnsurePostalCodeTablesAsync(cancellationToken);

        var mode = options.Mode == ImportMode.Auto ? ImportMode.Full : options.Mode;
        var runId = await _repository.BeginRunAsync(HandlerKey, mode.ToString().ToLowerInvariant(), cancellationToken);

        try
        {
            long rowsLoaded = mode == ImportMode.Full
                ? await RunFullAsync(cancellationToken)
                : await RunDeltaAsync(options.Yymm ?? throw new ArgumentException("Delta モードでは --yymm が必要です。"), cancellationToken);

            await _repository.CompleteRunAsync(runId, rowsLoaded, cancellationToken);
            return new ImportResult(true, rowsLoaded, startedAt, DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            await _repository.FailRunAsync(runId, ex.Message, cancellationToken);
            return new ImportResult(false, 0, startedAt, DateTimeOffset.UtcNow, ex.Message);
        }
    }

    private async Task<long> RunFullAsync(CancellationToken cancellationToken)
    {
        await TruncateStagingAsync(cancellationToken);

        using var csvStream = await _fetcher.FetchAsync(FullUrl, "*.csv", cancellationToken);
        var rowsLoaded = await _loader.LoadAsync(_connectionString, StagingTable, csvStream, Encoding.UTF8, cancellationToken);

        await ExecuteSqlSectionAsync("MergeToProduction.sql", "-- [FULL]", "-- [DELTA_ADD]", cancellationToken);
        await TruncateStagingAsync(cancellationToken);

        return rowsLoaded;
    }

    private async Task<long> RunDeltaAsync(string yymm, CancellationToken cancellationToken)
    {
        // 追加・更新
        await TruncateStagingAsync(cancellationToken);
        var addUrl = string.Format(CultureInfo.InvariantCulture, DeltaAddUrlTemplate, yymm);
        using var addStream = await _fetcher.FetchAsync(addUrl, "*.csv", cancellationToken);
        var rowsAdded = await _loader.LoadAsync(_connectionString, StagingTable, addStream, Encoding.UTF8, cancellationToken);
        await ExecuteSqlSectionAsync("MergeToProduction.sql", "-- [DELTA_ADD]", "-- [DELTA_DEL]", cancellationToken);
        await TruncateStagingAsync(cancellationToken);

        // 削除
        var delUrl = string.Format(CultureInfo.InvariantCulture, DeltaDelUrlTemplate, yymm);
        using var delStream = await _fetcher.FetchAsync(delUrl, "*.csv", cancellationToken);
        await _loader.LoadAsync(_connectionString, StagingTable, delStream, Encoding.UTF8, cancellationToken);
        await ExecuteSqlSectionAsync("MergeToProduction.sql", "-- [DELTA_DEL]", null, cancellationToken);
        await TruncateStagingAsync(cancellationToken);

        return rowsAdded;
    }

    private async Task EnsurePostalCodeTablesAsync(CancellationToken cancellationToken)
    {
        var sql = ReadEmbeddedSql("CreateTables.sql");
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task TruncateStagingAsync(CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand($"TRUNCATE {StagingTable}", conn);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task ExecuteSqlSectionAsync(string fileName, string startMarker, string? endMarker, CancellationToken cancellationToken)
    {
        var fullSql = ReadEmbeddedSql(fileName);
        var sql = ExtractSection(fullSql, startMarker, endMarker);
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string ExtractSection(string sql, string startMarker, string? endMarker)
    {
        var start = sql.IndexOf(startMarker, StringComparison.Ordinal);
        if (start < 0)
        {
            return sql;
        }

        start += startMarker.Length;

        if (endMarker is not null)
        {
            var end = sql.IndexOf(endMarker, start, StringComparison.Ordinal);
            if (end >= 0)
            {
                return sql[start..end].Trim();
            }
        }

        return sql[start..].Trim();
    }

    private static string ReadEmbeddedSql(string fileName)
    {
        var assembly = typeof(PostalCodeImportHandler).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"埋め込みリソース '{fileName}' が見つかりません。");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
