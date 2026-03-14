// <copyright file="Program.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.CommandLine;
using Aloe.Apps.CsvImporterLib.Extensions;
using Aloe.Apps.CsvImporterLib.Models;
using Aloe.Apps.CsvImporterLib.PostalCode.Extensions;
using Microsoft.Extensions.DependencyInjection;

// ─── 共通オプション ───────────────────────────────────────────────

Option<string> connectionOption = new("--connection")
{
    Description = "PostgreSQL 接続文字列",
    Required = true,
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

    var options = new ImportOptions(mode, yymm, connectionString);

    Console.WriteLine($"郵便番号インポート開始 (モード: {mode})");
    var result = await handler.RunAsync(options, cancellationToken);

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
    postalCodeCommand,
};

return await rootCommand.Parse(args).InvokeAsync();
