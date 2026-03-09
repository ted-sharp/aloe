// <copyright file="Program.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.CommandLine;
using Aloe.Apps.ExcelReportLib.Extensions;
using Aloe.Apps.ExcelReportLib.Services;
using Microsoft.Extensions.DependencyInjection;

Argument<FileInfo> inputArg = new("input")
{
    Description = "入力Excelファイル(.xlsx)のパス",
};

Argument<FileInfo> outputArg = new("output")
{
    Description = "出力PDFファイルのパス",
};

Option<int?> sheetOption = new("--sheet", "-s")
{
    Description = "シートインデックス(0-based)。省略時は先頭シート",
};

Option<string[]> varOption = new("--var", "-v")
{
    Description = "置換変数(Key=Value形式)。複数指定可",
    AllowMultipleArgumentsPerToken = true,
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

RootCommand rootCommand = new("Excel方眼紙テンプレートからPDFを生成するCLIツール")
{
    inputArg,
    outputArg,
    sheetOption,
    varOption,
    excelReaderOption,
    pdfRendererOption,
};

rootCommand.SetAction(parseResult =>
{
    var input = parseResult.GetValue(inputArg)!;
    var output = parseResult.GetValue(outputArg)!;
    var sheet = parseResult.GetValue(sheetOption);
    var vars = parseResult.GetValue(varOption);
    var excelReader = parseResult.GetValue(excelReaderOption) ?? "npoi";
    var pdfRenderer = parseResult.GetValue(pdfRendererOption) ?? "pdfsharp";

    if (!input.Exists)
    {
        Console.Error.WriteLine($"入力ファイルが見つかりません: {input.FullName}");
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

    Dictionary<string, string>? variables = null;
    if (vars is { Length: > 0 })
    {
        variables = [];
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

    var options = sheet.HasValue
        ? new Aloe.Apps.ExcelReportLib.Models.ReportOptions { SheetIndex = sheet.Value }
        : null;

    service.GeneratePdf(input.FullName, output.FullName, variables, options);

    Console.WriteLine($"PDF生成完了: {output.FullName}");
    return 0;
});

return rootCommand.Parse(args).Invoke();
