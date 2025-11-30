
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

public class MedisImporter
{
    private readonly AppDbContext _dbContext;

    public MedisImporter()
    {
        this._dbContext = App.Host.Services.GetRequiredService<AppDbContext>();
    }

    /// <summary>
    /// CSVファイルをPostgreSQLに取り込む
    /// </summary>
    public async Task ImportHotCsvToDatabase(string diagnosisPath)
    {
        var fullPath = Path.GetFullPath(diagnosisPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"CSVファイルが見つかりません: {fullPath}");
        }

        var sqlCommands = new List<string>
        {
            // 取り込む前に前のデータを消す
            "TRUNCATE ext.raw_hot13_codes;",

            // CSV を取り込む
            $"""
             COPY ext.raw_hot13_codes
             FROM '{fullPath}'
             WITH (
                 FORMAT   CSV
               , HEADER   true
               , ENCODING 'SJIS'
             )
             ;
             """,

            // 取り込む前に前のデータを消す
            "TRUNCATE ext.hot13_codes;",

            // インデックスがあったら削除する
            "DROP INDEX IF EXISTS ext.hot13_codes_IX1;",
            "DROP INDEX IF EXISTS ext.hot13_codes_IX2;",
            "DROP INDEX IF EXISTS ext.hot13_codes_IX3;",
            "DROP INDEX IF EXISTS ext.hot13_codes_IX4;",
            "DROP INDEX IF EXISTS ext.hot13_codes_IX5;",
            "DROP INDEX IF EXISTS ext.hot13_codes_IX6;",

            // 必要な項目だけ移す
            """
            INSERT INTO ext.hot13_codes
            (
                hot13_code
              , hot9_code
              , hot7_code
              , yakka_code
              , yj_code
              , receipt_code
              , official_name
              , product_name
              , receipt_drug_name
              , medication_type
              , pharmaceutical_company
              , pharmaceutical_distributor
            )
            SELECT
                hot13_code
              , hot7_code || distributor_code
              , hot7_code
              , yakka_code
              , yj_code
              , receipt_code
              , official_name
              , product_name
              , receipt_drug_name
              , medication_type
              , pharmaceutical_company
              , pharmaceutical_distributor
            FROM ext.raw_hot13_codes
            WHERE record_type IN ('1', '4')
              AND hot13_code IS NOT NULL
              AND yakka_code IS NOT NULL
              AND yj_code IS NOT NULL
              AND receipt_code IS NOT NULL
            ;
            """,

            // インデックスを作成する
            "CREATE INDEX hot13_codes_IX1 ON ext.hot13_codes (hot13_code);",
            "CREATE INDEX hot13_codes_IX2 ON ext.hot13_codes (hot9_code);",
            "CREATE INDEX hot13_codes_IX3 ON ext.hot13_codes (hot7_code);",
            "CREATE INDEX hot13_codes_IX4 ON ext.hot13_codes (yakka_code);",
            "CREATE INDEX hot13_codes_IX5 ON ext.hot13_codes (yj_code);",
            "CREATE INDEX hot13_codes_IX6 ON ext.hot13_codes (receipt_code);",

            // 取り込んだあとは不要なので消す
            "TRUNCATE ext.raw_hot13_codes;",
        };

        foreach (var sql in sqlCommands)
        {
            await this._dbContext.Database.ExecuteSqlRawAsync(sql);
        }
    }

    /// <summary>
    /// CSVファイルをPostgreSQLに取り込む
    /// </summary>
    public async Task ImportDiagnosisCsvToDatabase(string diagnosisPath)
    {
        var fullPath = Path.GetFullPath(diagnosisPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"CSVファイルが見つかりません: {fullPath}");
        }

        var sqlCommands = new List<string>
        {
            // 取り込む前に前のデータを消す
            "TRUNCATE ext.raw_icd10_diagnosis_codes;",

            // CSV を取り込む
            $"""
             COPY ext.raw_icd10_diagnosis_codes
             FROM '{fullPath}'
             WITH (
                 FORMAT   CSV
               , ENCODING 'SJIS'
             )
             ;
             """,

            // 取り込む前に前のデータを消す
            "TRUNCATE ext.icd10_diagnosis_codes;",

            // インデックスがあったら削除する
            "DROP INDEX IF EXISTS ext.icd10_diagnosis_codes_IX1;",
            "DROP INDEX IF EXISTS ext.icd10_diagnosis_codes_IX2;",
            "DROP INDEX IF EXISTS ext.icd10_diagnosis_codes_IX3;",

            // 必要な項目だけ移す
            """
            INSERT INTO ext.icd10_diagnosis_codes
            (
                diagnosis_code
              , diagnosis_name
              , diagnosis_name_kana
              , diagnosis_frequency_level
              , diagnosis_domain_code
              , modifier_recommendation_code
            )
            SELECT
                diagnosis_code
              , diagnosis_name
              , diagnosis_name_kana
              , diagnosis_frequency_level
              , diagnosis_domain_code
              , modifier_recommendation_code
            FROM ext.raw_icd10_diagnosis_codes
            ;
            """,

            // インデックスを作成する
            "CREATE INDEX icd10_diagnosis_codes_IX1 ON ext.icd10_diagnosis_codes (diagnosis_code);",
            "CREATE INDEX icd10_diagnosis_codes_IX2 ON ext.icd10_diagnosis_codes (diagnosis_name);",
            "CREATE INDEX icd10_diagnosis_codes_IX3 ON ext.icd10_diagnosis_codes (diagnosis_name_kana);",

            // 取り込んだあとは不要なので消す
            "TRUNCATE ext.raw_icd10_diagnosis_codes;",
        };

        foreach (var sql in sqlCommands)
        {
            await this._dbContext.Database.ExecuteSqlRawAsync(sql);
        }
    }

    /// <summary>
    /// CSVファイルをPostgreSQLに取り込む
    /// </summary>
    public async Task ImportModifierCsvToDatabase(string modifierPath)
    {
        var fullPath = Path.GetFullPath(modifierPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"CSVファイルが見つかりません: {fullPath}");
        }

        var sqlCommands = new List<string>
        {
            // 取り込む前に前のデータを消す
            "TRUNCATE ext.raw_icd10_modifier_codes;",

            // CSV を取り込む
            $"""
             COPY ext.raw_icd10_modifier_codes
             FROM '{fullPath}'
             WITH (
                 FORMAT   CSV
               , ENCODING 'SJIS'
             )
             ;
             """,

            // 取り込む前に前のデータを消す
            "TRUNCATE ext.icd10_modifier_codes;",

            // インデックスがあったら削除する
            "DROP INDEX IF EXISTS ext.icd10_modifier_codes_IX1;",
            "DROP INDEX IF EXISTS ext.icd10_modifier_codes_IX2;",
            "DROP INDEX IF EXISTS ext.icd10_modifier_codes_IX3;",

            // 必要な項目だけ移す
            """
            INSERT INTO ext.icd10_modifier_codes
            (
                modifier_code
              , modifier_name
              , modifier_name_kana
              , modifier_position_code
              , modifier_classification_code
              , modifier_mutex_group_code
              , modifier_description
            )
            SELECT
                modifier_code
              , modifier_name
              , modifier_name_kana
              , modifier_position_code
              , modifier_classification_code
              , modifier_mutex_group_code
              , modifier_description
            FROM ext.raw_icd10_modifier_codes
            ;
            """,

            // インデックスを作成する
            "CREATE INDEX icd10_modifier_codes_IX1 ON ext.icd10_modifier_codes (modifier_code);",
            "CREATE INDEX icd10_modifier_codes_IX2 ON ext.icd10_modifier_codes (modifier_name);",
            "CREATE INDEX icd10_modifier_codes_IX3 ON ext.icd10_modifier_codes (modifier_name_kana);",

            // 取り込んだあとは不要なので消す
            "TRUNCATE ext.raw_icd10_modifier_codes;",
        };

        foreach (var sql in sqlCommands)
        {
            await this._dbContext.Database.ExecuteSqlRawAsync(sql);
        }
    }

    /// <summary>
    /// CSVファイルをPostgreSQLに取り込む
    /// </summary>
    public async Task ImportIndexTermCsvToDatabase(string indexTermPath)
    {
        var fullPath = Path.GetFullPath(indexTermPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"CSVファイルが見つかりません: {fullPath}");
        }

        var sqlCommands = new List<string>
        {
            // 取り込む前に前のデータを消す
            "TRUNCATE ext.raw_icd10_index_terms;",

            // CSV を取り込む
            $"""
             COPY ext.raw_icd10_index_terms
             FROM '{fullPath}'
             WITH (
                 FORMAT   CSV
               , ENCODING 'SJIS'
             )
             ;
             """,

            // 取り込む前に前のデータを消す
            "TRUNCATE ext.icd10_index_terms;",

            // インデックスがあったら削除する
            "DROP INDEX IF EXISTS ext.icd10_index_terms_IX1;",

            // 必要な項目だけ移す
            """
            INSERT INTO ext.icd10_index_terms
            (
                index_term
              , linked_code
              , linked_table_code
              , linked_name_variant_code
              , synonym_type_code
              , character_variant_type_code
            )
            SELECT
                index_term
              , linked_code
              , linked_table_code
              , linked_name_variant_code
              , synonym_type_code
              , character_variant_type_code
            FROM ext.raw_icd10_index_terms
            ;
            """,

            // インデックスを作成する
            "CREATE INDEX icd10_index_terms_IX1 ON ext.icd10_index_terms (index_term);",

            // 取り込んだあとは不要なので消す
            "TRUNCATE ext.raw_icd10_index_terms;",
        };

        foreach (var sql in sqlCommands)
        {
            await this._dbContext.Database.ExecuteSqlRawAsync(sql);
        }
    }

    /// <summary>
    /// EXCELファイルをPostgreSQLに取り込む
    /// </summary>
    /// <param name="xlsxPath">取り込むEXCELファイルのパス</param>
    public async Task ImportExamExelToDatabase(string xlsxPath)
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
