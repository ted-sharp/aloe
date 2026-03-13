// <copyright file="Program.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.CommandLine;
using System.Text.Json;
using Aloe.Apps.ExcelReportLib.Abstractions;
using Aloe.Apps.ExcelReportLib.Extensions;
using Aloe.Apps.ExcelReportLib.Models;
using Aloe.Apps.ExcelReportLib.Services;
using Microsoft.Extensions.DependencyInjection;

// ─── generate サブコマンド ────────────────────────────────────────

Argument<FileInfo> generateInputArg = new("input")
{
    Description = "入力Excelファイル(.xlsx)のパス",
};

Argument<FileInfo> generateOutputArg = new("output")
{
    Description = "出力PDFファイルのパス",
};

Option<int?> generateSheetOption = new("--sheet", "-s")
{
    Description = "シートインデックス(0-based)。省略時は先頭シート",
};

Option<string[]> generateVarOption = new("--var", "-v")
{
    Description = "置換変数(Key=Value形式)。複数指定可",
    AllowMultipleArgumentsPerToken = true,
};

Option<FileInfo?> generateVarFileOption = new("--var-file", "-f")
{
    Description = "置換変数を定義したJSONファイルのパス",
};

Option<string> excelReaderOption = new("--excel-reader")
{
    Description = "Excel読み取りライブラリ(npoi / closedxml)",
    DefaultValueFactory = _ => "npoi",
};

Option<string> pdfRendererOption = new("--pdf-renderer")
{
    Description = "PDF描画ライブラリ(pdfsharp / questpdf)",
    DefaultValueFactory = _ => "pdfsharp",
};

Command generateCommand = new("generate", "ExcelテンプレートからPDFを生成する")
{
    generateInputArg,
    generateOutputArg,
    generateSheetOption,
    generateVarOption,
    generateVarFileOption,
    excelReaderOption,
    pdfRendererOption,
};

generateCommand.SetAction(parseResult =>
{
    var input = parseResult.GetValue(generateInputArg)!;
    var output = parseResult.GetValue(generateOutputArg)!;
    var sheet = parseResult.GetValue(generateSheetOption);
    var excelReader = parseResult.GetValue(excelReaderOption) ?? "npoi";
    var pdfRenderer = parseResult.GetValue(pdfRendererOption) ?? "pdfsharp";

    if (!input.Exists)
    {
        Console.Error.WriteLine($"入力ファイルが見つかりません: {input.FullName}");
        return 1;
    }

    if (!LoadVariables(parseResult, generateVarFileOption, generateVarOption, out var variables))
    {
        return 1;
    }

    var services = new ServiceCollection();
    services.AddExcelReportCore();

    if (excelReader.Equals("closedxml", StringComparison.OrdinalIgnoreCase))
    {
        services.AddExcelReportWithClosedXml();
    }
    else
    {
        services.AddExcelReportWithNpoi();
    }

    if (pdfRenderer.Equals("questpdf", StringComparison.OrdinalIgnoreCase))
    {
        services.AddExcelReportWithQuestPdf();
    }
    else
    {
        services.AddExcelReportWithPdfSharp();
    }

    using var provider = services.BuildServiceProvider();
    var service = provider.GetRequiredService<ExcelReportService>();

    var options = sheet.HasValue
        ? new ReportOptions { SheetIndex = sheet.Value }
        : null;

    service.GeneratePdf(input.FullName, output.FullName, variables.Count > 0 ? variables : null, options);

    Console.WriteLine($"PDF生成完了: {output.FullName}");
    return 0;
});

// ─── print サブコマンド ───────────────────────────────────────────

Argument<FileInfo> printInputArg = new("input")
{
    Description = "入力Excelファイル(.xlsx)のパス",
};

Option<string> printerOption = new("--printer", "-p")
{
    Description = "送信先プリンター名",
    Required = true,
};

Option<FileInfo?> printOutputOption = new("--output", "-o")
{
    Description = "中間PDFの保存先パス。省略時は一時ファイルを使用して印刷後に削除",
};

Option<int?> printSheetOption = new("--sheet", "-s")
{
    Description = "シートインデックス(0-based)。省略時は先頭シート",
};

Option<string[]> printVarOption = new("--var", "-v")
{
    Description = "置換変数(Key=Value形式)。複数指定可",
    AllowMultipleArgumentsPerToken = true,
};

Option<FileInfo?> printVarFileOption = new("--var-file", "-f")
{
    Description = "置換変数を定義したJSONファイルのパス",
};

Option<int> copiesOption = new("--copies", "-c")
{
    Description = "印刷部数",
    DefaultValueFactory = _ => 1,
};

Option<int> dpiOption = new("--dpi")
{
    Description = "印刷解像度(DPI)",
    DefaultValueFactory = _ => 300,
};

Command printCommand = new("print", "ExcelテンプレートをプリンターへPDF印刷する")
{
    printInputArg,
    printerOption,
    printOutputOption,
    printSheetOption,
    printVarOption,
    printVarFileOption,
    copiesOption,
    dpiOption,
};

printCommand.SetAction(parseResult =>
{
    if (!OperatingSystem.IsWindows())
    {
        Console.Error.WriteLine("print コマンドは Windows でのみ使用できます。");
        return 1;
    }

    var input = parseResult.GetValue(printInputArg)!;
    var printer = parseResult.GetValue(printerOption)!;
    var printOutput = parseResult.GetValue(printOutputOption);
    var sheet = parseResult.GetValue(printSheetOption);
    var copies = parseResult.GetValue(copiesOption);
    var dpi = parseResult.GetValue(dpiOption);

    if (!input.Exists)
    {
        Console.Error.WriteLine($"入力ファイルが見つかりません: {input.FullName}");
        return 1;
    }

    if (!LoadVariables(parseResult, printVarFileOption, printVarOption, out var variables))
    {
        return 1;
    }

    var services = new ServiceCollection();
    services.AddExcelReportWithWindowsPrinter();
    using var provider = services.BuildServiceProvider();
    var service = provider.GetRequiredService<ExcelPrintService>();

    var useTempFile = printOutput is null;
    var pdfPath = printOutput?.FullName ?? Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

    try
    {
        using var excelStream = input.OpenRead();

        var reportOptions = sheet.HasValue
            ? new ReportOptions { SheetIndex = sheet.Value }
            : null;

        var printOptions = new PrintOptions
        {
            PrinterName = printer,
            Copies = copies,
            Dpi = dpi,
        };

        service.Print(excelStream, variables.Count > 0 ? variables : null, reportOptions, pdfPath, printOptions);

        Console.WriteLine($"印刷完了: {printer}");
        if (!useTempFile)
        {
            Console.WriteLine($"PDF保存: {pdfPath}");
        }
    }
    finally
    {
        if (useTempFile)
        {
            try { File.Delete(pdfPath); } catch (IOException) { }
        }
    }

    return 0;
});

// ─── printers サブコマンド ─────────────────────────────────────────

Command printersCommand = new("printers", "インストール済みプリンターの一覧を表示する");

printersCommand.SetAction(_ =>
{
    if (!OperatingSystem.IsWindows())
    {
        Console.Error.WriteLine("printers コマンドは Windows でのみ使用できます。");
        return 1;
    }

    var services = new ServiceCollection();
    services.AddExcelReportWithWindowsPrinter();
    using var provider = services.BuildServiceProvider();
    var printerService = provider.GetRequiredService<ISheetPrinter>();

    foreach (var name in printerService.GetInstalledPrinters())
    {
        Console.WriteLine(name);
    }

    return 0;
});

// ─── ルートコマンド ───────────────────────────────────────────────

RootCommand rootCommand = new("Excel方眼紙テンプレートからPDFを生成・印刷するCLIツール")
{
    generateCommand,
    printCommand,
    printersCommand,
};

return rootCommand.Parse(args).Invoke();

// ─── ヘルパー関数 ─────────────────────────────────────────────────

static bool LoadVariables(
    ParseResult parseResult,
    Option<FileInfo?> varFileOption,
    Option<string[]> varOption,
    out Dictionary<string, string> variables)
{
    variables = [];

    var varFile = parseResult.GetValue(varFileOption);
    if (varFile is not null)
    {
        if (!varFile.Exists)
        {
            Console.Error.WriteLine($"変数ファイルが見つかりません: {varFile.FullName}");
            return false;
        }

        var json = File.ReadAllText(varFile.FullName);
        var fileVars = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        if (fileVars is not null)
        {
            foreach (var kvp in fileVars)
            {
                variables[kvp.Key] = kvp.Value;
            }
        }
    }

    var vars = parseResult.GetValue(varOption);
    if (vars is { Length: > 0 })
    {
        foreach (var v in vars)
        {
            int eqIndex = v.IndexOf('=', StringComparison.Ordinal);
            if (eqIndex > 0)
            {
                string key = v[..eqIndex];
                string value = v[(eqIndex + 1)..];
                variables[key] = value;
            }
        }
    }

    return true;
}
