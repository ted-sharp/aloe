// <copyright file="JfagyCodesImportHandler.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Text.Json;
using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Models;
using Aloe.Apps.CsvImporterLib.Services;
using Npgsql;
using NpgsqlTypes;

namespace Aloe.Apps.CsvImporterLib.JfagyCodes;

/// <summary>
/// J-FAGY アレルゲンコード JSON インポートハンドラー。
/// FHIR CodeSystem リソース形式の JSON ファイルを複数受け付け、
/// 親子階層構造（parent_code）と別名（designation）を抽出する。
/// </summary>
public sealed class JfagyCodesImportHandler : IImportHandler
{
    private const string StagingTable = "ext.raw_jfagy_allergen_codes";
    private const string SqlTruncate = $"TRUNCATE {StagingTable}";

    private readonly ImportRunRepository _repository;
    private readonly string _connectionString;

    /// <summary>
    /// コンストラクター。
    /// </summary>
    public JfagyCodesImportHandler(ImportRunRepository repository, string connectionString)
    {
        _repository = repository;
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public string HandlerKey => "jfagy-codes";

    /// <inheritdoc />
    public async Task<ImportResult> RunAsync(
        ImportOptions options,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;

        if (options.Mode == ImportMode.Delta)
        {
            throw new NotSupportedException("J-FAGY アレルゲンコード インポートは Full モードのみサポートしています。");
        }

        if (options.SourcePaths is null || options.SourcePaths.Length == 0)
        {
            throw new ArgumentException("J-FAGY アレルゲンコード インポートには --source で JSON ファイルパスを1つ以上指定してください。");
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
            $"COPY {StagingTable} (coding_system, code, display, parent_code, designation) FROM STDIN (FORMAT BINARY)",
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

            totalRows += await WriteConceptsAsync(writer, codingSystem, conceptArray, parentCode: null, cancellationToken);
        }

        await writer.CompleteAsync(cancellationToken);

        return totalRows;
    }

    private static async Task<long> WriteConceptsAsync(
        NpgsqlBinaryImporter writer,
        string? codingSystem,
        JsonElement conceptArray,
        string? parentCode,
        CancellationToken cancellationToken)
    {
        long count = 0;

        foreach (var concept in conceptArray.EnumerateArray())
        {
            var code = concept.TryGetProperty("code", out var codeProp) ? codeProp.GetString() : null;
            var display = concept.TryGetProperty("display", out var displayProp) ? displayProp.GetString() : null;

            if (string.IsNullOrEmpty(code))
            {
                continue;
            }

            // property 配列から parent の valueCode を抽出
            var extractedParent = parentCode;
            if (concept.TryGetProperty("property", out var propertyArray) &&
                propertyArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var prop in propertyArray.EnumerateArray())
                {
                    if (prop.TryGetProperty("code", out var propCode) &&
                        propCode.GetString() == "parent" &&
                        prop.TryGetProperty("valueCode", out var valueCode))
                    {
                        extractedParent = valueCode.GetString();
                        break;
                    }
                }
            }

            // designation 配列から最初の value を抽出
            string? designation = null;
            if (concept.TryGetProperty("designation", out var designationArray) &&
                designationArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var desig in designationArray.EnumerateArray())
                {
                    if (desig.TryGetProperty("value", out var valueProp))
                    {
                        designation = valueProp.GetString();
                        break;
                    }
                }
            }

            await writer.StartRowAsync(cancellationToken);
            await WriteStrAsync(writer, codingSystem, cancellationToken);
            await WriteStrAsync(writer, code, cancellationToken);
            await WriteStrAsync(writer, display, cancellationToken);
            await WriteStrAsync(writer, extractedParent, cancellationToken);
            await WriteStrAsync(writer, designation, cancellationToken);

            count++;

            // ネストされた concept を再帰的に走査
            if (concept.TryGetProperty("concept", out var childConcepts) &&
                childConcepts.ValueKind == JsonValueKind.Array)
            {
                count += await WriteConceptsAsync(writer, codingSystem, childConcepts, code, cancellationToken);
            }
        }

        return count;
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
