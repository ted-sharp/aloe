
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

namespace AloeExtImporter;

public class MedisExcelImporter
{
    private readonly AppDbContext _dbContext;

    public MedisExcelImporter()
    {
        this._dbContext = App.Host.Services.GetRequiredService<AppDbContext>();
    }

    /// <summary>
    /// EXCELファイルをPostgreSQLに取り込む
    /// </summary>
    /// <param name="xlsxPath">取り込むEXCELファイルのパス</param>
    public async Task ImportExelToDatabase(string xlsxPath)
    {
        var fullPath = Path.GetFullPath(xlsxPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"EXCELファイルが見つかりません: {fullPath}");
        }

        // 取り込む前に前のデータを消す
        await this._dbContext.Database.ExecuteSqlRawAsync("TRUNCATE ext.raw_jlac10_codes;");

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
            COPY ext.raw_jlac10_codes (
              jlac10_code_17
              , analyte_flag
              , analyte_code
              , analyte_name
              , identification_flag
              , identification_code
              , identification_name
              , specimen_flag
              , specimen_code
              , specimen_name
              , methodology_flag
              , methodology_code
              , methodology_name
              , result_identifying_general_flag
              , result_identifying_general_code
              , result_identifying_general_name
              , result_identifying_specific_flag
              , result_identifying_specific_name
              , result_identifying_specific_code
            ) FROM STDIN (FORMAT BINARY)
            """);

        var rows = await MiniExcel.QueryAsync(fullPath, sheetName: "17桁コード表", startCell: "C5");

        foreach (var row in rows)
        {
            // DynamicExpandObject を IDictionary<string, object> としてキャストし、Values から配列に変換
            var dict = row as IDictionary<string, object>;
            if (dict == null || dict.Values.Count < 19)
            {
                continue;
            }
            var data = dict.Values.ToArray();

            await writer.StartRowAsync();

            // Excel の各セル（インデックス 0～15）を COPY 対象の各カラムへ書き込む
            await writer.WriteAsync(Convert.ToString(data[0])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[1])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[2])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[3])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[4])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[5])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[6])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[7])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[8])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[9])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[10])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[11])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[12])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[13])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[14])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[15])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[16])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[17])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[18])?.Trim(), NpgsqlDbType.Text);
        }
        await writer.CompleteAsync();

        // 開き直さないと COPY モードのままとなる
        await connection.CloseAsync();
        await connection.OpenAsync();

        var sqlCommands = new List<string>
        {
            // 取り込む前に前のデータを消す
            "TRUNCATE ext.jlac10_codes;",

            // インデックスがあったら削除する
            "DROP INDEX IF EXISTS ext.jlac10_codes_IX1;",
            "DROP INDEX IF EXISTS ext.jlac10_codes_IX2;",

            // 必要な項目だけ移す
            """
            INSERT INTO ext.jlac10_codes
            (
              jlac10_code
              , analyte_code
              , analyte_name
              , identification_code
              , identification_name
              , specimen_code
              , specimen_name
              , methodology_code
              , methodology_name
              , result_identifying_general_code
              , result_identifying_general_name
              , result_identifying_specific_name
              , result_identifying_specific_code
            )
            SELECT
              jlac10_code_17
              , analyte_code
              , analyte_name
              , identification_code
              , identification_name
              , specimen_code
              , specimen_name
              , methodology_code
              , methodology_name
              , result_identifying_general_code
              , result_identifying_general_name
              , result_identifying_specific_name
              , result_identifying_specific_code
            FROM ext.raw_jlac10_codes
            WHERE
              -- 途中のヘッダーを念の為除外
              jlac10_code_17 <> 'JLAC10コード（17桁）'
              AND jlac10_code_17 <> '新コード'
            ;
            """,

            // インデックスを作成する
            "CREATE INDEX jlac10_codes_IX1 ON ext.jlac10_codes (jlac10_code);",
            "CREATE INDEX jlac10_codes_IX2 ON ext.jlac10_codes (analyte_name);",

            // 取り込んだあとは不要なので消す
            "TRUNCATE ext.raw_jlac10_codes;",
        };

        foreach (var sql in sqlCommands)
        {
            await this._dbContext.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
