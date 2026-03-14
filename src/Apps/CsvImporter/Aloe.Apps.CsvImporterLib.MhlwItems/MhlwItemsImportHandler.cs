// <copyright file="MhlwItemsImportHandler.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Models;
using Aloe.Apps.CsvImporterLib.Services;
using ClosedXML.Excel;
using Npgsql;
using NpgsqlTypes;

namespace Aloe.Apps.CsvImporterLib.MhlwItems;

/// <summary>
/// 厚労省XML特定健診項目 Excel インポートハンドラー。
/// C3セルを起点として読み込む（raw テーブルのみ）。
/// </summary>
public sealed class MhlwItemsImportHandler : IImportHandler
{
    private const int StartRow = 3;
    private const int StartColumn = 3; // C
    private const string StagingTable = "ext.raw_mhlw_xml_tokutei_kenshin_items";
    private const string SqlTruncate = $"TRUNCATE {StagingTable}";

    private readonly ImportRunRepository _repository;
    private readonly string _connectionString;

    /// <summary>
    /// コンストラクター。
    /// </summary>
    public MhlwItemsImportHandler(ImportRunRepository repository, string connectionString)
    {
        _repository = repository;
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public string HandlerKey => "mhlw-items";

    /// <inheritdoc />
    public async Task<ImportResult> RunAsync(
        ImportOptions options,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;

        if (options.Mode == ImportMode.Delta)
        {
            throw new NotSupportedException("厚労省XML特定健診項目 インポートは Full モードのみサポートしています。");
        }

        if (string.IsNullOrWhiteSpace(options.SourcePath))
        {
            throw new ArgumentException("厚労省XML特定健診項目 インポートには --source でローカル XLSX ファイルパスを指定してください。");
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
        await ExecuteSqlAsync(SqlTruncate, cancellationToken);

        long rowsLoaded = 0;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        // COPY カラム定義は ext.raw_mhlw_xml_tokutei_kenshin_items のスキーマに合わせてください
        await using var writer = await conn.BeginBinaryImportAsync(
            $"COPY {StagingTable} (code, name, label) FROM STDIN (FORMAT BINARY)",
            cancellationToken);

        await Task.Run(() =>
        {
            using var workbook = new XLWorkbook(sourcePath);
            var sheet = workbook.Worksheet(1);
            var lastRow = sheet.LastRowUsed()?.RowNumber() ?? StartRow;

            for (var row = StartRow; row <= lastRow; row++)
            {
                var code = sheet.Cell(row, StartColumn).GetString();
                if (string.IsNullOrEmpty(code))
                {
                    continue;
                }

                var name = sheet.Cell(row, StartColumn + 1).GetString();
                var label = sheet.Cell(row, StartColumn + 2).GetString();

                writer.StartRow();
                writer.Write(code, NpgsqlDbType.Text);
                WriteStr(writer, name);
                WriteStr(writer, label);

                rowsLoaded++;
            }
        }, cancellationToken);

        await writer.CompleteAsync(cancellationToken);

        return rowsLoaded;
    }

    private static void WriteStr(NpgsqlBinaryImporter writer, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            writer.WriteNull();
        }
        else
        {
            writer.Write(value, NpgsqlDbType.Text);
        }
    }

    private async Task ExecuteSqlAsync(string sql, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
