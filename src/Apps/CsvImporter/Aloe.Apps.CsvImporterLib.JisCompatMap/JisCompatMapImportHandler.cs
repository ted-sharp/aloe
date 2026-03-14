// <copyright file="JisCompatMapImportHandler.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Models;
using Aloe.Apps.CsvImporterLib.Services;
using ClosedXML.Excel;
using Npgsql;
using NpgsqlTypes;

namespace Aloe.Apps.CsvImporterLib.JisCompatMap;

/// <summary>
/// JIS互換マップ Excel インポートハンドラー。
/// A3セルを起点として読み込み、IBM拡張漢字エントリも追加する。
/// </summary>
public sealed class JisCompatMapImportHandler : IImportHandler
{
    private const int StartRow = 3;
    private const int StartColumn = 1; // A
    private const string StagingTable = "ext.raw_jis_compat_maps";

    private const string StagingColumns =
        "source_menkuten_code, source_unicode, source_text, source_jis_kubun, " +
        "mapped_menkuten_code, mapped_unicode, mapped_text, " +
        "multi_menkuten_code_1, multi_menkuten_code_2, multi_menkuten_code_3, multi_menkuten_code_4, " +
        "multi_unicode_1, multi_unicode_2, multi_unicode_3, multi_unicode_4, " +
        "multi_text, remarks";

    private const string SqlFull = """
        TRUNCATE ext.jis_compat_maps;

        INSERT INTO ext.jis_compat_maps (source_text, mapped_text)
        SELECT source_text, mapped_text
        FROM ext.raw_jis_compat_maps
        WHERE mapped_text <> source_text
          AND mapped_text <> '';

        TRUNCATE ext.raw_jis_compat_maps;
        """;

    private const string SqlIbmExtended = """
        INSERT INTO ext.jis_compat_maps (source_text, mapped_text)
        VALUES
            ('髙', '高')
          , ('閒', '聞')
          , ('晴', '晴')
          , ('益', '益')
          , ('礼', '礼')
          , ('靖', '靖')
          , ('精', '精')
          , ('羽', '羽')
          , ('逸', '逸')
          , ('飯', '飯')
          , ('飼', '飼')
          , ('館', '館')
          , ('鶴', '鶴')
        ON CONFLICT DO NOTHING;
        """;

    private readonly ImportRunRepository _repository;
    private readonly string _connectionString;

    /// <summary>
    /// コンストラクター。
    /// </summary>
    public JisCompatMapImportHandler(ImportRunRepository repository, string connectionString)
    {
        _repository = repository;
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public string HandlerKey => "jis-compat-map";

    /// <inheritdoc />
    public async Task<ImportResult> RunAsync(
        ImportOptions options,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;

        if (options.Mode == ImportMode.Delta)
        {
            throw new NotSupportedException("JIS互換マップ インポートは Full モードのみサポートしています。");
        }

        if (string.IsNullOrWhiteSpace(options.SourcePath))
        {
            throw new ArgumentException("JIS互換マップ インポートには --source でローカル XLSX ファイルパスを指定してください。");
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

        long rowsLoaded = 0;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var writer = await conn.BeginBinaryImportAsync(
            $"COPY {StagingTable} ({StagingColumns}) FROM STDIN (FORMAT BINARY)",
            cancellationToken);

        await Task.Run(() =>
        {
            using var workbook = new XLWorkbook(sourcePath);
            var sheet = workbook.Worksheet(1);
            var lastRow = sheet.LastRowUsed()?.RowNumber() ?? StartRow;

            for (var row = StartRow; row <= lastRow; row++)
            {
                var col1 = sheet.Cell(row, StartColumn).GetString().Trim();
                if (string.IsNullOrEmpty(col1))
                {
                    continue;
                }

                writer.StartRow();
                for (var col = 0; col < 17; col++)
                {
                    var val = sheet.Cell(row, StartColumn + col).GetString().Trim();
                    WriteStr(writer, val);
                }

                rowsLoaded++;
            }
        }, cancellationToken);

        await writer.CompleteAsync(cancellationToken);

        await ExecuteSqlAsync(SqlFull, cancellationToken);
        await ExecuteSqlAsync(SqlIbmExtended, cancellationToken);

        return rowsLoaded;
    }

    private static void WriteStr(NpgsqlBinaryImporter writer, string? value)
    {
        writer.Write(value ?? "", NpgsqlDbType.Text);
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
