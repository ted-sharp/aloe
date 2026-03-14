// <copyright file="ImportRunRepository.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Text.Json;
using Npgsql;

namespace Aloe.Apps.CsvImporterLib.Services;

/// <summary>
/// ingestion_runs / ingestion_files テーブルの CRUD を担当するリポジトリ。
/// </summary>
public sealed class ImportRunRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// コンストラクター。
    /// </summary>
    public ImportRunRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// メタデータテーブルが存在しない場合に作成する。
    /// </summary>
    public async Task EnsureTablesAsync(CancellationToken cancellationToken = default)
    {
        var sql = ReadEmbeddedSql("CreateMetadataTables.sql");
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// 新しいインポート実行を開始し、run_id を返す。
    /// </summary>
    public async Task<long> BeginRunAsync(string sourceSystem, string mode, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO ext.ingestion_runs (source_system, mode, status, started_at) VALUES (@s, @m, 'running', NOW()) RETURNING run_id",
            conn);
        cmd.Parameters.AddWithValue("s", sourceSystem);
        cmd.Parameters.AddWithValue("m", mode);
        return (long)(await cmd.ExecuteScalarAsync(cancellationToken))!;
    }

    /// <summary>
    /// インポート実行を完了状態で更新する。
    /// </summary>
    public async Task CompleteRunAsync(long runId, long rowsLoaded, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            "UPDATE ext.ingestion_runs SET status = 'completed', rows_loaded = @r, finished_at = NOW() WHERE run_id = @id",
            conn);
        cmd.Parameters.AddWithValue("r", rowsLoaded);
        cmd.Parameters.AddWithValue("id", runId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// インポート実行を失敗状態で更新する。
    /// </summary>
    public async Task FailRunAsync(long runId, string errorMessage, CancellationToken cancellationToken = default)
    {
        var errJson = JsonSerializer.Serialize(new { message = errorMessage });
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            "UPDATE ext.ingestion_runs SET status = 'failed', finished_at = NOW(), errors = @e::jsonb WHERE run_id = @id",
            conn);
        cmd.Parameters.AddWithValue("e", errJson);
        cmd.Parameters.AddWithValue("id", runId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string ReadEmbeddedSql(string fileName)
    {
        var assembly = typeof(ImportRunRepository).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"埋め込みリソース '{fileName}' が見つかりません。");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
