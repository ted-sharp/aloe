// <copyright file="Program.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.CommandLine;
using Aloe.Apps.CsvImporterLib.Extensions;
using Aloe.Apps.CsvImporterLib.FhirCodes.Extensions;
using Aloe.Apps.CsvImporterLib.HealthFacility.Extensions;
using Aloe.Apps.CsvImporterLib.HoujinNumber.Extensions;
using Aloe.Apps.CsvImporterLib.JisCompatMap.Extensions;
using Aloe.Apps.CsvImporterLib.MedisCodes.Extensions;
using Aloe.Apps.CsvImporterLib.MhlwItems.Extensions;
using Aloe.Apps.CsvImporterLib.Models;
using Aloe.Apps.CsvImporterLib.PostalCode.Extensions;
using Microsoft.Extensions.DependencyInjection;

// ─── 共通オプション ───────────────────────────────────────────────

Option<string> connectionOption = new("--connection")
{
    Description = "PostgreSQL 接続文字列",
    Required = true,
};

Option<string?> workDirOption = new("--work-dir")
{
    Description = "ダウンロードZIPを保存/キャッシュするディレクトリ",
};

// ─── postal-code サブコマンド ─────────────────────────────────────

Option<bool> fullOption = new("--full")
{
    Description = "全件洗い替え（Full モード）",
};

Option<string?> yymmOption = new("--yymm")
{
    Description = "差分取り込み対象の年月（例: 2501）。指定すると Delta モードになる",
};

Command postalCodeCommand = new("postal-code", "日本郵便の郵便番号データを取り込む")
{
    fullOption,
    yymmOption,
};

postalCodeCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var connectionString = parseResult.GetValue(connectionOption)!;
    var full = parseResult.GetValue(fullOption);
    var yymm = parseResult.GetValue(yymmOption);
    var workDir = parseResult.GetValue(workDirOption);

    ImportMode mode;
    if (full)
    {
        mode = ImportMode.Full;
    }
    else if (yymm is not null)
    {
        mode = ImportMode.Delta;
    }
    else
    {
        mode = ImportMode.Auto;
    }

    var services = new ServiceCollection();
    services.AddCsvImporterCore(connectionString);
    services.AddPostalCodeImport(connectionString);

    await using var provider = services.BuildServiceProvider();
    var handler = provider.GetServices<Aloe.Apps.CsvImporterLib.Abstractions.IImportHandler>()
        .First(h => h.HandlerKey == "postal-code");

    var options = new ImportOptions(mode, yymm, connectionString, WorkDir: workDir);

    var progress = new Progress<ImportProgress>(p =>
    {
        if (p.Stage == ImportProgressStage.Downloading)
        {
            var pct = p.Percent.HasValue ? $"{p.Percent}%" : "...";
            Console.Write($"\rダウンロード中... {pct}   ");
        }
        else
        {
            Console.Write("\rインポート中...      ");
        }
    });

    Console.WriteLine($"郵便番号インポート開始 (モード: {mode})");
    var result = await handler.RunAsync(options, progress, cancellationToken);
    Console.WriteLine();

    if (result.Success)
    {
        Console.WriteLine($"完了: {result.RowsLoaded:N0} 件, 所要時間: {(result.FinishedAt - result.StartedAt).TotalSeconds:F1}s");
        return 0;
    }

    Console.Error.WriteLine($"エラー: {result.ErrorMessage}");
    return 1;
});

// ─── houjin-number サブコマンド ──────────────────────────────────

Option<string> sourceOption = new("--source")
{
    Description = "ローカルファイルパス",
    Required = true,
};

Command houjinNumberCommand = new("houjin-number", "国税庁の法人番号データを取り込む")
{
    sourceOption,
};

houjinNumberCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var connectionString = parseResult.GetValue(connectionOption)!;
    var source = parseResult.GetValue(sourceOption)!;

    var services = new ServiceCollection();
    services.AddCsvImporterCore(connectionString);
    services.AddHoujinNumberImport(connectionString);

    await using var provider = services.BuildServiceProvider();
    var handler = provider.GetServices<Aloe.Apps.CsvImporterLib.Abstractions.IImportHandler>()
        .First(h => h.HandlerKey == "houjin-number");

    var options = new ImportOptions(ImportMode.Full, null, connectionString, source);

    var progress = new Progress<ImportProgress>(p =>
    {
        Console.Write("\rインポート中...      ");
    });

    Console.WriteLine("法人番号インポート開始 (モード: Full)");
    var result = await handler.RunAsync(options, progress, cancellationToken);
    Console.WriteLine();

    if (result.Success)
    {
        Console.WriteLine($"完了: {result.RowsLoaded:N0} 件, 所要時間: {(result.FinishedAt - result.StartedAt).TotalSeconds:F1}s");
        return 0;
    }

    Console.Error.WriteLine($"エラー: {result.ErrorMessage}");
    return 1;
});

// ─── medis-hot サブコマンド ───────────────────────────────────────

Command medisHotCommand = new("medis-hot", "MEDIS HOT13薬品コードを取り込む（ローカルZIP）")
{
    sourceOption,
};

medisHotCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var connectionString = parseResult.GetValue(connectionOption)!;
    var source = parseResult.GetValue(sourceOption)!;

    var services = new ServiceCollection();
    services.AddCsvImporterCore(connectionString);
    services.AddMedisCodesImport(connectionString);

    await using var provider = services.BuildServiceProvider();
    var handler = provider.GetServices<Aloe.Apps.CsvImporterLib.Abstractions.IImportHandler>()
        .First(h => h.HandlerKey == "medis-hot");

    var options = new ImportOptions(ImportMode.Full, null, connectionString, source);

    var progress = new Progress<ImportProgress>(_ => Console.Write("\rインポート中...      "));

    Console.WriteLine("MEDIS HOT13 インポート開始 (モード: Full)");
    var result = await handler.RunAsync(options, progress, cancellationToken);
    Console.WriteLine();

    if (result.Success)
    {
        Console.WriteLine($"完了: {result.RowsLoaded:N0} 件, 所要時間: {(result.FinishedAt - result.StartedAt).TotalSeconds:F1}s");
        return 0;
    }

    Console.Error.WriteLine($"エラー: {result.ErrorMessage}");
    return 1;
});

// ─── medis-disease サブコマンド ───────────────────────────────────

Command medisDiseaseCommand = new("medis-disease", "MEDIS 病名マスター（ICD-10）を取り込む（ローカルZIP）")
{
    sourceOption,
};

medisDiseaseCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var connectionString = parseResult.GetValue(connectionOption)!;
    var source = parseResult.GetValue(sourceOption)!;

    var services = new ServiceCollection();
    services.AddCsvImporterCore(connectionString);
    services.AddMedisCodesImport(connectionString);

    await using var provider = services.BuildServiceProvider();
    var handler = provider.GetServices<Aloe.Apps.CsvImporterLib.Abstractions.IImportHandler>()
        .First(h => h.HandlerKey == "medis-disease");

    var options = new ImportOptions(ImportMode.Full, null, connectionString, source);

    var progress = new Progress<ImportProgress>(_ => Console.Write("\rインポート中...      "));

    Console.WriteLine("MEDIS 病名マスター インポート開始 (モード: Full)");
    var result = await handler.RunAsync(options, progress, cancellationToken);
    Console.WriteLine();

    if (result.Success)
    {
        Console.WriteLine($"完了: {result.RowsLoaded:N0} 件, 所要時間: {(result.FinishedAt - result.StartedAt).TotalSeconds:F1}s");
        return 0;
    }

    Console.Error.WriteLine($"エラー: {result.ErrorMessage}");
    return 1;
});

// ─── medis-jlac10 サブコマンド ────────────────────────────────────

Command medisJlac10Command = new("medis-jlac10", "MEDIS JLAC10検査コードを取り込む（ローカルXLSX）")
{
    sourceOption,
};

medisJlac10Command.SetAction(async (parseResult, cancellationToken) =>
{
    var connectionString = parseResult.GetValue(connectionOption)!;
    var source = parseResult.GetValue(sourceOption)!;

    var services = new ServiceCollection();
    services.AddCsvImporterCore(connectionString);
    services.AddMedisCodesImport(connectionString);

    await using var provider = services.BuildServiceProvider();
    var handler = provider.GetServices<Aloe.Apps.CsvImporterLib.Abstractions.IImportHandler>()
        .First(h => h.HandlerKey == "medis-jlac10");

    var options = new ImportOptions(ImportMode.Full, null, connectionString, source);

    var progress = new Progress<ImportProgress>(_ => Console.Write("\rインポート中...      "));

    Console.WriteLine("MEDIS JLAC10 インポート開始 (モード: Full)");
    var result = await handler.RunAsync(options, progress, cancellationToken);
    Console.WriteLine();

    if (result.Success)
    {
        Console.WriteLine($"完了: {result.RowsLoaded:N0} 件, 所要時間: {(result.FinishedAt - result.StartedAt).TotalSeconds:F1}s");
        return 0;
    }

    Console.Error.WriteLine($"エラー: {result.ErrorMessage}");
    return 1;
});

// ─── jis-compat-map サブコマンド ─────────────────────────────────

Command jisCompatMapCommand = new("jis-compat-map", "JIS互換文字マップを取り込む（ローカルXLSX）")
{
    sourceOption,
};

jisCompatMapCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var connectionString = parseResult.GetValue(connectionOption)!;
    var source = parseResult.GetValue(sourceOption)!;

    var services = new ServiceCollection();
    services.AddCsvImporterCore(connectionString);
    services.AddJisCompatMapImport(connectionString);

    await using var provider = services.BuildServiceProvider();
    var handler = provider.GetServices<Aloe.Apps.CsvImporterLib.Abstractions.IImportHandler>()
        .First(h => h.HandlerKey == "jis-compat-map");

    var options = new ImportOptions(ImportMode.Full, null, connectionString, source);

    var progress = new Progress<ImportProgress>(_ => Console.Write("\rインポート中...      "));

    Console.WriteLine("JIS互換マップ インポート開始 (モード: Full)");
    var result = await handler.RunAsync(options, progress, cancellationToken);
    Console.WriteLine();

    if (result.Success)
    {
        Console.WriteLine($"完了: {result.RowsLoaded:N0} 件, 所要時間: {(result.FinishedAt - result.StartedAt).TotalSeconds:F1}s");
        return 0;
    }

    Console.Error.WriteLine($"エラー: {result.ErrorMessage}");
    return 1;
});

// ─── mhlw-items サブコマンド ─────────────────────────────────────

Command mhlwItemsCommand = new("mhlw-items", "厚労省XML特定健診項目を取り込む（ローカルXLSX）")
{
    sourceOption,
};

mhlwItemsCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var connectionString = parseResult.GetValue(connectionOption)!;
    var source = parseResult.GetValue(sourceOption)!;

    var services = new ServiceCollection();
    services.AddCsvImporterCore(connectionString);
    services.AddMhlwItemsImport(connectionString);

    await using var provider = services.BuildServiceProvider();
    var handler = provider.GetServices<Aloe.Apps.CsvImporterLib.Abstractions.IImportHandler>()
        .First(h => h.HandlerKey == "mhlw-items");

    var options = new ImportOptions(ImportMode.Full, null, connectionString, source);

    var progress = new Progress<ImportProgress>(_ => Console.Write("\rインポート中...      "));

    Console.WriteLine("厚労省XML特定健診項目 インポート開始 (モード: Full)");
    var result = await handler.RunAsync(options, progress, cancellationToken);
    Console.WriteLine();

    if (result.Success)
    {
        Console.WriteLine($"完了: {result.RowsLoaded:N0} 件, 所要時間: {(result.FinishedAt - result.StartedAt).TotalSeconds:F1}s");
        return 0;
    }

    Console.Error.WriteLine($"エラー: {result.ErrorMessage}");
    return 1;
});

// ─── health-facility サブコマンド ────────────────────────────────

Command healthFacilityCommand = new("health-facility", "特定健診実施機関を取り込む（ローカルZIPまたはCSV）")
{
    sourceOption,
};

healthFacilityCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var connectionString = parseResult.GetValue(connectionOption)!;
    var source = parseResult.GetValue(sourceOption)!;

    var services = new ServiceCollection();
    services.AddCsvImporterCore(connectionString);
    services.AddHealthFacilityImport(connectionString);

    await using var provider = services.BuildServiceProvider();
    var handler = provider.GetServices<Aloe.Apps.CsvImporterLib.Abstractions.IImportHandler>()
        .First(h => h.HandlerKey == "health-facility");

    var options = new ImportOptions(ImportMode.Full, null, connectionString, source);

    var progress = new Progress<ImportProgress>(_ => Console.Write("\rインポート中...      "));

    Console.WriteLine("特定健診実施機関 インポート開始 (モード: Full)");
    var result = await handler.RunAsync(options, progress, cancellationToken);
    Console.WriteLine();

    if (result.Success)
    {
        Console.WriteLine($"完了: {result.RowsLoaded:N0} 件, 所要時間: {(result.FinishedAt - result.StartedAt).TotalSeconds:F1}s");
        return 0;
    }

    Console.Error.WriteLine($"エラー: {result.ErrorMessage}");
    return 1;
});

// ─── fhir-codes サブコマンド ─────────────────────────────────────

Option<string[]> sourcePathsOption = new("--source")
{
    Description = "FHIR CodeSystem JSON ファイルパス（複数指定可）",
    Required = true,
    AllowMultipleArgumentsPerToken = false,
};
sourcePathsOption.Arity = ArgumentArity.OneOrMore;

Command fhirCodesCommand = new("fhir-codes", "FHIR観察コードを取り込む（ローカルJSONファイル）")
{
    sourcePathsOption,
};

fhirCodesCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var connectionString = parseResult.GetValue(connectionOption)!;
    var sources = parseResult.GetValue(sourcePathsOption)!;

    var services = new ServiceCollection();
    services.AddCsvImporterCore(connectionString);
    services.AddFhirCodesImport(connectionString);

    await using var provider = services.BuildServiceProvider();
    var handler = provider.GetServices<Aloe.Apps.CsvImporterLib.Abstractions.IImportHandler>()
        .First(h => h.HandlerKey == "fhir-codes");

    var options = new ImportOptions(ImportMode.Full, null, connectionString, SourcePaths: sources);

    var progress = new Progress<ImportProgress>(_ => Console.Write("\rインポート中...      "));

    Console.WriteLine($"FHIR観察コード インポート開始 (ファイル数: {sources.Length})");
    var result = await handler.RunAsync(options, progress, cancellationToken);
    Console.WriteLine();

    if (result.Success)
    {
        Console.WriteLine($"完了: {result.RowsLoaded:N0} 件, 所要時間: {(result.FinishedAt - result.StartedAt).TotalSeconds:F1}s");
        return 0;
    }

    Console.Error.WriteLine($"エラー: {result.ErrorMessage}");
    return 1;
});

// ─── ルートコマンド ───────────────────────────────────────────────

RootCommand rootCommand = new("各種CSVデータをPostgreSQLへインポートする")
{
    connectionOption,
    workDirOption,
    postalCodeCommand,
    houjinNumberCommand,
    medisHotCommand,
    medisDiseaseCommand,
    medisJlac10Command,
    jisCompatMapCommand,
    mhlwItemsCommand,
    healthFacilityCommand,
    fhirCodesCommand,
};

return await rootCommand.Parse(args).InvokeAsync();
