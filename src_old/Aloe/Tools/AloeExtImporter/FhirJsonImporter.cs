
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Microsoft.Extensions.DependencyInjection;
using MiniExcelLibs;
using Npgsql;
using NpgsqlTypes;
using System.Linq;
using System.Text.Json;

namespace AloeExtImporter;

public class FhirJsonImporter
{
    private readonly AppDbContext _dbContext;

    public FhirJsonImporter()
    {
        this._dbContext = App.Host.Services.GetRequiredService<AppDbContext>();
    }

    /// <summary>
    /// TRUNCATE します。
    /// </summary>
    public async Task TruncateDatabase()
    {
        // 取り込む前に前のデータを消す
        await this._dbContext.Database.ExecuteSqlRawAsync("TRUNCATE ext.raw_fhir_observation_codes;");
    }

    /// <summary>
    /// JSONファイルをPostgreSQLに取り込む
    /// </summary>
    /// <param name="jsonPath">取り込むEXCELファイルのパス</param>
    public async Task ImportJsonToDatabase(string jsonPath)
    {
        var fullPath = Path.GetFullPath(jsonPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"JSONファイルが見つかりません: {fullPath}");
        }

        var jsonText = await File.ReadAllTextAsync(fullPath);
        using var doc = JsonDocument.Parse(jsonText);

        // JSONのルート要素から"url"フィールドの値を取得
        if (!doc.RootElement.TryGetProperty("url", out JsonElement urlElement))
        {
            throw new InvalidOperationException("JSON内に 'url' フィールドが見つかりません。");
        }
        var codingSystem = urlElement.GetString();

        if (!doc.RootElement.TryGetProperty("concept", out JsonElement concepts))
        {
            throw new InvalidOperationException("concept 配列が見つかりません。");
        }

        // EFCore の接続を利用して BinaryImport を実行
        var connection = this._dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        // EFCore で取得した接続は NpgsqlConnection としてキャスト可能
        var npgsqlConnection = (NpgsqlConnection)connection;

        // 事前に your_table が存在し、name (text) と age (integer) のカラムがあることを確認してください
        await using var writer = await npgsqlConnection.BeginBinaryImportAsync(
            """
            COPY ext.raw_fhir_observation_codes (
              concept_code
              , jurisdiction_coding_system
              , concept_display
            ) FROM STDIN (FORMAT BINARY)
            """);

        foreach (var item in concepts.EnumerateArray())
        {
            var code = item.GetProperty("code").GetString()?.Trim();
            var display = item.GetProperty("display").GetString()?.Trim();

            await writer.StartRowAsync();
            await writer.WriteAsync(code, NpgsqlDbType.Text);
            await writer.WriteAsync(codingSystem, NpgsqlDbType.Text);
            await writer.WriteAsync(display, NpgsqlDbType.Text);
        }
        await writer.CompleteAsync();
    }
}
