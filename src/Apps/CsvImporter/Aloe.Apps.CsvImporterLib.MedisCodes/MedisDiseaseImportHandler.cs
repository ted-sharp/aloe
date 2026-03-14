// <copyright file="MedisDiseaseImportHandler.cs" company="ted-sharp">
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
/// MEDIS 病名マスター（ICD-10準拠）の CSV インポートハンドラー。
/// ZIP 内の3ファイル（病名基本情報・修飾語基本情報・索引語情報）を一括インポートする。
/// </summary>
public sealed class MedisDiseaseImportHandler : IImportHandler
{
    private const string StagingDiagnosis = "ext.raw_icd10_diagnosis_codes";
    private const string StagingModifier = "ext.raw_icd10_modifier_codes";
    private const string StagingIndexTerm = "ext.raw_icd10_index_terms";

    private const string SqlDiagnosis = """
        TRUNCATE ext.icd10_diagnosis_codes;
        INSERT INTO ext.icd10_diagnosis_codes SELECT * FROM ext.raw_icd10_diagnosis_codes;
        TRUNCATE ext.raw_icd10_diagnosis_codes;
        """;

    private const string SqlModifier = """
        TRUNCATE ext.icd10_modifier_codes;
        INSERT INTO ext.icd10_modifier_codes SELECT * FROM ext.raw_icd10_modifier_codes;
        TRUNCATE ext.raw_icd10_modifier_codes;
        """;

    private const string SqlIndexTerm = """
        TRUNCATE ext.icd10_index_terms;
        INSERT INTO ext.icd10_index_terms SELECT * FROM ext.raw_icd10_index_terms;
        TRUNCATE ext.raw_icd10_index_terms;
        """;

    private readonly ICsvBulkLoader _loader;
    private readonly ImportRunRepository _repository;
    private readonly string _connectionString;

    /// <summary>
    /// コンストラクター。
    /// </summary>
    public MedisDiseaseImportHandler(
        ICsvBulkLoader loader,
        ImportRunRepository repository,
        string connectionString)
    {
        _loader = loader;
        _repository = repository;
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public string HandlerKey => "medis-disease";

    /// <inheritdoc />
    public async Task<ImportResult> RunAsync(
        ImportOptions options,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;

        if (options.Mode == ImportMode.Delta)
        {
            throw new NotSupportedException("MEDIS 病名マスター インポートは Full モードのみサポートしています。");
        }

        if (string.IsNullOrWhiteSpace(options.SourcePath))
        {
            throw new ArgumentException("MEDIS 病名マスター インポートには --source でローカルZIPファイルパスを指定してください。");
        }

        var runId = await _repository.BeginRunAsync(HandlerKey, "full", cancellationToken);

        try
        {
            progress?.Report(new ImportProgress(ImportProgressStage.Importing));
            var rowsLoaded = await RunFullAsync(options.SourcePath, progress, cancellationToken);

            await _repository.CompleteRunAsync(runId, rowsLoaded, cancellationToken);
            return new ImportResult(true, rowsLoaded, startedAt, DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            await _repository.FailRunAsync(runId, ex.Message, cancellationToken);
            return new ImportResult(false, 0, startedAt, DateTimeOffset.UtcNow, ex.Message);
        }
    }

    private async Task<long> RunFullAsync(string sourcePath, IProgress<ImportProgress>? progress, CancellationToken cancellationToken)
    {
        using var archive = ZipFile.OpenRead(sourcePath);

        var csvEntries = archive.Entries
            .Where(e => e.FullName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (csvEntries.Count < 3)
        {
            throw new InvalidOperationException(
                $"ZIP ファイル '{sourcePath}' 内に CSV ファイルが3つ以上必要です（病名基本情報・修飾語基本情報・索引語情報）。見つかった数: {csvEntries.Count}");
        }

        // アルファベット順で病名基本情報・修飾語基本情報・索引語情報を割り当て
        // 実際のファイル名パターンに応じてこの割り当てを調整してください
        var diagnosisEntry = FindEntry(csvEntries, "病名", "IYK", "diagnosis") ?? csvEntries[0];
        var modifierEntry = FindEntry(csvEntries, "修飾", "IYM", "modifier") ?? csvEntries[1];
        var indexTermEntry = FindEntry(csvEntries, "索引", "IYI", "index") ?? csvEntries[2];

        var sjis = Encoding.GetEncoding(932);

        // 病名基本情報
        await TruncateStagingAsync(StagingDiagnosis, cancellationToken);
        using (var stream = diagnosisEntry.Open())
        {
            var rowsLoaded = await _loader.LoadAsync(_connectionString, StagingDiagnosis, stream, sjis, cancellationToken);
            progress?.Report(new ImportProgress(ImportProgressStage.Importing));
            await ExecuteSqlAsync(SqlDiagnosis, cancellationToken);
        }

        // 修飾語基本情報
        await TruncateStagingAsync(StagingModifier, cancellationToken);
        using (var stream = modifierEntry.Open())
        {
            await _loader.LoadAsync(_connectionString, StagingModifier, stream, sjis, cancellationToken);
            progress?.Report(new ImportProgress(ImportProgressStage.Importing));
            await ExecuteSqlAsync(SqlModifier, cancellationToken);
        }

        // 索引語情報
        await TruncateStagingAsync(StagingIndexTerm, cancellationToken);
        using (var stream = indexTermEntry.Open())
        {
            var rowsLoaded = await _loader.LoadAsync(_connectionString, StagingIndexTerm, stream, sjis, cancellationToken);
            progress?.Report(new ImportProgress(ImportProgressStage.Importing));
            await ExecuteSqlAsync(SqlIndexTerm, cancellationToken);
            return rowsLoaded;
        }
    }

    private static ZipArchiveEntry? FindEntry(List<ZipArchiveEntry> entries, params string[] keywords)
    {
        return entries.FirstOrDefault(e =>
            keywords.Any(kw => e.FullName.Contains(kw, StringComparison.OrdinalIgnoreCase)));
    }

    private async Task TruncateStagingAsync(string table, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand($"TRUNCATE {table}", conn);
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
