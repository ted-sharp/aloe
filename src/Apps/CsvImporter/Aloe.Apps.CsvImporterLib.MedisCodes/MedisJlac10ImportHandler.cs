// <copyright file="MedisJlac10ImportHandler.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Models;
using Aloe.Apps.CsvImporterLib.Services;
using ClosedXML.Excel;
using Npgsql;
using NpgsqlTypes;

namespace Aloe.Apps.CsvImporterLib.MedisCodes;

/// <summary>
/// MEDIS JLAC10検査コードの Excel インポートハンドラー。
/// シート「17桁コード表」のC5セルを起点として読み込む。
/// </summary>
public sealed class MedisJlac10ImportHandler : IImportHandler
{
    private const string SheetName = "17桁コード表";
    private const int StartRow = 5;
    private const int StartColumn = 3; // C
    private const string StagingTable = "ext.raw_jlac10_codes";

    private const string SqlFull = """
        TRUNCATE ext.jlac10_codes;

        INSERT INTO ext.jlac10_codes
        SELECT * FROM ext.raw_jlac10_codes;

        TRUNCATE ext.raw_jlac10_codes;
        """;

    private readonly ImportRunRepository _repository;
    private readonly string _connectionString;

    /// <summary>
    /// コンストラクター。
    /// </summary>
    public MedisJlac10ImportHandler(ImportRunRepository repository, string connectionString)
    {
        _repository = repository;
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public string HandlerKey => "medis-jlac10";

    /// <inheritdoc />
    public async Task<ImportResult> RunAsync(
        ImportOptions options,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;

        if (options.Mode == ImportMode.Delta)
        {
            throw new NotSupportedException("MEDIS JLAC10 インポートは Full モードのみサポートしています。");
        }

        if (string.IsNullOrWhiteSpace(options.SourcePath))
        {
            throw new ArgumentException("MEDIS JLAC10 インポートには --source でローカル XLSX ファイルパスを指定してください。");
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

        // COPY カラム定義は ext.raw_jlac10_codes のスキーマに合わせてください
        await using var writer = await conn.BeginBinaryImportAsync(
            $"COPY {StagingTable} (jlac10_code, name) FROM STDIN (FORMAT BINARY)",
            cancellationToken);

        // ClosedXML は同期 API のため Task.Run でラップ
        await Task.Run(() =>
        {
            using var workbook = new XLWorkbook(sourcePath);
            var sheet = workbook.Worksheet(SheetName);
            var lastRow = sheet.LastRowUsed()?.RowNumber() ?? StartRow;

            for (var row = StartRow; row <= lastRow; row++)
            {
                var code = sheet.Cell(row, StartColumn).GetString();
                if (string.IsNullOrEmpty(code))
                {
                    continue;
                }

                var name = sheet.Cell(row, StartColumn + 1).GetString();

                writer.StartRow();
                writer.Write(code, NpgsqlDbType.Text);

                if (string.IsNullOrEmpty(name))
                {
                    writer.WriteNull();
                }
                else
                {
                    writer.Write(name, NpgsqlDbType.Text);
                }

                rowsLoaded++;
            }
        }, cancellationToken);

        await writer.CompleteAsync(cancellationToken);
        await ExecuteSqlAsync(SqlFull, cancellationToken);

        return rowsLoaded;
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
