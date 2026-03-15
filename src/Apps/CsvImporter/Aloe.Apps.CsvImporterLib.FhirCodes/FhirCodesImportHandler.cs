// <copyright file="FhirCodesImportHandler.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Text.Json;
using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Models;
using Aloe.Apps.CsvImporterLib.Services;
using Npgsql;
using NpgsqlTypes;

namespace Aloe.Apps.CsvImporterLib.FhirCodes;

/// <summary>
/// FHIR観察コード JSON インポートハンドラー。
/// CodeSystem リソース形式の JSON ファイルを複数受け付ける。
/// </summary>
public sealed class FhirCodesImportHandler : IImportHandler
{
    private const string StagingTable = "ext.raw_fhir_observation_codes";
    private const string SqlTruncate = $"TRUNCATE {StagingTable}";

    private readonly ImportRunRepository _repository;
    private readonly string _connectionString;

    /// <summary>
    /// コンストラクター。
    /// </summary>
    public FhirCodesImportHandler(ImportRunRepository repository, string connectionString)
    {
        _repository = repository;
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public string HandlerKey => "fhir-codes";

    /// <inheritdoc />
    public async Task<ImportResult> RunAsync(
        ImportOptions options,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;

        if (options.Mode == ImportMode.Delta)
        {
            throw new NotSupportedException("FHIR観察コード インポートは Full モードのみサポートしています。");
        }

        if (options.SourcePaths is null || options.SourcePaths.Length == 0)
        {
            throw new ArgumentException("FHIR観察コード インポートには --source で JSON ファイルパスを1つ以上指定してください。");
        }

        var runId = await _repository.BeginRunAsync(HandlerKey, "full", cancellationToken);

        try
        {
            progress?.Report(new ImportProgress(ImportProgressStage.Importing));
            var rowsLoaded = await RunFullAsync(options.SourcePaths, cancellationToken);

            await _repository.CompleteRunAsync(runId, rowsLoaded, cancellationToken);
            return new ImportResult(true, rowsLoaded, startedAt, DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            await _repository.FailRunAsync(runId, ex.Message, cancellationToken);
            return new ImportResult(false, 0, startedAt, DateTimeOffset.UtcNow, ex.Message);
        }
    }

    private async Task<long> RunFullAsync(string[] sourcePaths, CancellationToken cancellationToken)
    {
        await ExecuteSqlAsync(SqlTruncate, cancellationToken);

        long totalRows = 0;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var writer = await conn.BeginBinaryImportAsync(
            $"COPY {StagingTable} (coding_system, code, display) FROM STDIN (FORMAT BINARY)",
            cancellationToken);

        foreach (var path in sourcePaths)
        {
            await using var fileStream = File.OpenRead(path);
            using var doc = await JsonDocument.ParseAsync(fileStream, cancellationToken: cancellationToken);
            var root = doc.RootElement;

            var codingSystem = root.TryGetProperty("url", out var urlProp) ? urlProp.GetString() : null;

            if (!root.TryGetProperty("concept", out var conceptArray) ||
                conceptArray.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var concept in conceptArray.EnumerateArray())
            {
                var code = concept.TryGetProperty("code", out var codeProp) ? codeProp.GetString() : null;
                var display = concept.TryGetProperty("display", out var displayProp) ? displayProp.GetString() : null;

                if (string.IsNullOrEmpty(code))
                {
                    continue;
                }

                await writer.StartRowAsync(cancellationToken);
                await WriteStrAsync(writer, codingSystem, cancellationToken);
                await WriteStrAsync(writer, code, cancellationToken);
                await WriteStrAsync(writer, display, cancellationToken);

                totalRows++;
            }
        }

        await writer.CompleteAsync(cancellationToken);

        return totalRows;
    }

    private static async Task WriteStrAsync(NpgsqlBinaryImporter writer, string? value, CancellationToken cancellationToken)
    {
        if (value is null)
        {
            await writer.WriteNullAsync(cancellationToken);
        }
        else
        {
            await writer.WriteAsync(value, NpgsqlDbType.Text, cancellationToken);
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
