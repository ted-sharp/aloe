
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

public class MhlwExcelImporter
{
    private readonly AppDbContext _dbContext;

    public MhlwExcelImporter()
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
            COPY ext.raw_mhlw_xml_tokutei_kenshin_items (
              category_code
              , category_name
              , sort_no
              , jlac10_code
              , item_name
              , item_data_type
              , xml_data_type
              , xml_data_length
              , xml_data_format
              , item_data_unit
              , xml_analyte_code
              , xml_analyte_name
              , xml_methodology_code
              , xml_methodology_name
              , xml_data_unit
              , result_code_oid
              , item_code_oid
              , xml_remarks
              , remarks
            ) FROM STDIN (FORMAT BINARY)
            """);

        var rows = await MiniExcel.QueryAsync(fullPath, startCell: "C3");

        foreach (var row in rows)
        {
            // DynamicExpandObject を IDictionary<string, object> としてキャストし、Values から配列に変換
            var dict = row as IDictionary<string, object>;
            if (dict == null || dict.Values.Count < 21)
            {
                continue;
            }
            var data = dict.Values.ToArray();

            var jlac10_code = Convert.ToString(data[3])?.Trim();
            if (String.IsNullOrWhiteSpace(jlac10_code))
            {
                // 空行を省く
                continue;
            }

            await writer.StartRowAsync();

            // Excel の各セル（インデックス 0～15）を COPY 対象の各カラムへ書き込む
            await writer.WriteAsync(Convert.ToString(data[0])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[1])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[2])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(jlac10_code, NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[4])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[5])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[6])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[7])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[8])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[9])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[10])?.Trim(), NpgsqlDbType.Text);
            // 3列飛ばす
            await writer.WriteAsync(Convert.ToString(data[14])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[15])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[16])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[17])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[18])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[19])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[20])?.Trim(), NpgsqlDbType.Text);
            await writer.WriteAsync(Convert.ToString(data[21])?.Trim(), NpgsqlDbType.Text);
        }
        await writer.CompleteAsync();
    }
}
