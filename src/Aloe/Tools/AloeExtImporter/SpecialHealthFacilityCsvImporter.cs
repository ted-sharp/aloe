
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Microsoft.Extensions.DependencyInjection;

namespace AloeExtImporter;

public class SpecialHealthFacilityCsvImporter
{
    private readonly AppDbContext _dbContext;

    public SpecialHealthFacilityCsvImporter()
    {
        this._dbContext = App.Host.Services.GetRequiredService<AppDbContext>();
    }

    /// <summary>
    /// CSVファイルをPostgreSQLに取り込む
    /// </summary>
    /// <param name="csvPath">取り込むCSVファイルのパス</param>
    public async Task ImportSpecialHealthFacilityCsvToDatabase(string csvPath)
    {
        var fullPath = Path.GetFullPath(csvPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"CSVファイルが見つかりません: {fullPath}");
        }

        var sqlCommands = new List<string>
        {
            // 取り込む前に前のデータを消す
            "TRUNCATE ext.raw_special_health_facility_codes;",

            // CSV を取り込む
            $"""
            COPY ext.raw_special_health_facility_codes
            FROM '{fullPath}'
            WITH (
                FORMAT   CSV
              , HEADER   true
              , ENCODING 'SJIS'
            )
            ;
            """,

            // 不要URLを削除
            """
            UPDATE ext.raw_special_health_facility_codes
            SET website = ''
            WHERE website = 'http://'
            ;
            """,

            // 取り込む前に前のデータを消す
            "TRUNCATE ext.facility_codes;",

            // インデックスがあったら削除する
            "DROP INDEX IF EXISTS ext.facility_codes_IX1;",
            "DROP INDEX IF EXISTS ext.facility_codes_IX2;",
            "DROP INDEX IF EXISTS ext.facility_codes_IX3;",
            "DROP INDEX IF EXISTS ext.facility_codes_IX4;",

            // 必要な項目だけ移す
            """
            INSERT INTO ext.facility_codes
            (
                facility_code
              , facility_name
              , zip_code
              , address
              , phone
              , website
            )
            SELECT
                facility_code
              , facility_name
              , zip_code
              , address
              , phone
              , website
            FROM ext.raw_special_health_facility_codes
            ;
            """,

            // インデックスを作成する
            "CREATE INDEX facility_codes_IX1 ON ext.facility_codes (facility_code);",
            "CREATE INDEX facility_codes_IX2 ON ext.facility_codes (facility_name);",
            "CREATE INDEX facility_codes_IX3 ON ext.facility_codes (zip_code);",
            "CREATE INDEX facility_codes_IX4 ON ext.facility_codes (phone);",

            // 取り込んだあとは不要なので消す
            "TRUNCATE ext.raw_special_health_facility_codes;",
        };

        foreach (var sql in sqlCommands)
        {
            await this._dbContext.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
