
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

public class ExcelImporter
{
    private readonly AppDbContext _dbContext;

    public ExcelImporter()
    {
        this._dbContext = App.Host.Services.GetRequiredService<AppDbContext>();
    }

    /// <summary>
    /// EXCELファイルをPostgreSQLに取り込む
    /// </summary>
    /// <param name="xlsxPath">取り込むCSVファイルのパス</param>
    public async Task ImportJisExelToDatabase(string xlsxPath)
    {
        var fullPath = Path.GetFullPath(xlsxPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"EXCELファイルが見つかりません: {fullPath}");
        }

        // 取り込む前に前のデータを消す
        this._dbContext.Database.ExecuteSqlRaw("TRUNCATE ext.raw_zip_codes;");

        // EFCore の接続を利用して BinaryImport を実行
        var connection = this._dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        // EFCore で取得した接続は NpgsqlConnection としてキャスト可能
        var npgsqlConnection = (NpgsqlConnection)connection;


        // 事前に your_table が存在し、name (text) と age (integer) のカラムがあることを確認してください
        await using var writer = npgsqlConnection.BeginBinaryImport(
            """
            COPY ext.raw_jis_compat_maps (
                source_menkuten_code
              , source_unicode
              , source_text
              , source_jis_kubun
              , mapped_menkuten_code
              , mapped_unicode
              , mapped_text
              , multi_menkuten_code_1
              , multi_menkuten_code_2
              , multi_menkuten_code_3
              , multi_menkuten_code_4
              , multi_unicode_1
              , multi_unicode_2
              , multi_unicode_3
              , multi_unicode_4
              , multi_text
              , remarks
            ) FROM STDIN (FORMAT BINARY)
            """);

        var rows = MiniExcel.Query(fullPath, startCell: "A3");

        foreach (var row in rows)
        {
            // DynamicExpandObject を IDictionary<string, object> としてキャストし、Values から配列に変換
            var dict = row as IDictionary<string, object>;
            if (dict == null || dict.Values.Count < 17)
            {
                continue;
            }
            var data = dict.Values.ToArray();

            await writer.StartRowAsync();

            // Excel の各セル（インデックス 0～15）を COPY 対象の各カラムへ書き込む
            await writer.WriteAsync(Convert.ToString(data[0]), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[1]), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[2]), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[3]), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[4]), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[5]), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[6]), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[7]), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[8]), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[9]), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[10]), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[11]), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[12]), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[13]), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[14]), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[15]), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[16]), NpgsqlDbType.Text);
        }
        await writer.CompleteAsync();

        // 開き直さないと COPY モードのままとなる
        await connection.CloseAsync();
        await connection.OpenAsync();


        var sqlCommands = new List<string>
        {
            // 取り込む前に前のデータを消す
            "TRUNCATE ext.jis_compat_maps;",

            // インデックスがあったら削除する
            "DROP INDEX IF EXISTS ext.jis_compat_maps_IX1;",

            // 必要な項目だけ移す
            """
            INSERT INTO ext.jis_compat_maps
            (
                source_text
              , mapped_text
            )
            SELECT
                source_text
              , mapped_text
            FROM ext.raw_jis_compat_maps;
            """,

            // インデックスを作成する
            "CREATE INDEX jis_compat_maps_IX1 ON ext.jis_compat_maps (source_text);",

            // 取り込んだあとは不要なので消す
            "TRUNCATE ext.raw_jis_compat_maps;",
        };

        foreach (var sql in sqlCommands)
        {
            this._dbContext.Database.ExecuteSqlRaw(sql);
        }
    }
}
