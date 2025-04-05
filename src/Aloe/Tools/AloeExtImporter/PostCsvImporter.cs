
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Microsoft.Extensions.DependencyInjection;

namespace AloeExtImporter;

public class PostCsvImporter
{
    private readonly AppDbContext _dbContext;

    public PostCsvImporter()
    {
        this._dbContext = App.Host.Services.GetRequiredService<AppDbContext>();
    }

    /// <summary>
    /// CSVファイルをPostgreSQLに取り込む
    /// </summary>
    /// <param name="csvPath">取り込むCSVファイルのパス</param>
    public async Task ImportPostCsvToDatabase(string csvPath)
    {
        var fullPath = Path.GetFullPath(csvPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"CSVファイルが見つかりません: {fullPath}");
        }

        var sqlCommands = new List<string>
        {
            // 取り込む前に前のデータを消す
            "TRUNCATE ext.raw_zip_codes;",

            // CSV を取り込む
            $"COPY ext.raw_zip_codes FROM '{fullPath}' CSV;",

            // 取り込む前に前のデータを消す
            "TRUNCATE ext.zip_codes;",

            // インデックスがあったら削除する
            "DROP INDEX IF EXISTS ext.zip_codes_IX1;",

            // 必要な項目だけ移す
            """
            INSERT INTO ext.zip_codes
            (
                zip_code
              , prefecture_katakana
              , city_katakana
              , town_katakana
              , prefecture
              , city
              , town
            )
            SELECT
                zip_code7
              , prefecture_katakana
              , city_katakana
              , town_katakana
              , prefecture
              , city
              , town
            FROM ext.raw_zip_codes;
            """,

            // インデックスを作成する
            "CREATE INDEX zip_codes_IX1 ON ext.zip_codes (zip_code);",

            // 取り込んだあとは不要なので消す
            "TRUNCATE ext.raw_zip_codes;",
        };

        foreach (var sql in sqlCommands)
        {
            await this._dbContext.Database.ExecuteSqlRawAsync(sql);
        }
    }

    /// <summary>
    /// CSVファイルをPostgreSQLに取り込む
    /// </summary>
    /// <param name="csvPath">取り込むCSVファイルのパス</param>
    public async Task ImportHoujinCsvToDatabase(string csvPath)
    {
        var fullPath = Path.GetFullPath(csvPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"CSVファイルが見つかりません: {fullPath}");
        }

        var sqlCommands = new List<string>
        {
            // 取り込む前に前のデータを消す
            "TRUNCATE ext.raw_houjin_numbers;",

            // CSV を取り込む
            $"COPY ext.raw_houjin_numbers FROM '{fullPath}' CSV;",

            // 取り込む前に前のデータを消す
            "TRUNCATE ext.houjin_numbers;",

            // インデックスがあったら削除する
            "DROP INDEX IF EXISTS ext.houjin_numbers_IX1;",

            // 必要な項目だけ移す
            """
            INSERT INTO ext.houjin_numbers
            (
                corporate_number
              , name
              , zip_code
              , prefecture_name
              , city_name
              , street_number
            )
            SELECT
                corporate_number
              , name
              , post_code
              , prefecture_name
              , city_name
              , street_number
            FROM ext.raw_houjin_numbers
            WHERE prefecture_name IS NOT NULL
              AND post_code IS NOT NULL
              AND close_date IS NULL
              AND hihyoji = '0'
            ;
            """,

            // インデックスを作成する
            "CREATE INDEX houjin_numbers_IX1 ON ext.houjin_numbers (corporate_number);",

            // 取り込んだあとは不要なので消す
            "TRUNCATE ext.raw_houjin_numbers;",
        };

        foreach (var sql in sqlCommands)
        {
            await this._dbContext.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
