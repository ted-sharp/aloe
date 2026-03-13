// <copyright file="ExcelPrintService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportLib.Abstractions;
using Aloe.Apps.ExcelReportLib.Models;

namespace Aloe.Apps.ExcelReportLib.Services;

/// <summary>
/// Excel読み取り・テンプレート置換・PDF保存・印刷をまとめるオーケストレーター。
/// </summary>
public class ExcelPrintService
{
    private readonly IExcelReader _reader;
    private readonly IPdfRenderer _pdfRenderer;
    private readonly ITemplateEngine _templateEngine;
    private readonly ISheetPrinter _printer;

    /// <summary>
    /// <see cref="ExcelPrintService"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public ExcelPrintService(
        IExcelReader reader,
        IPdfRenderer pdfRenderer,
        ITemplateEngine templateEngine,
        ISheetPrinter printer)
    {
        _reader = reader;
        _pdfRenderer = pdfRenderer;
        _templateEngine = templateEngine;
        _printer = printer;
    }

    /// <summary>
    /// Excel ストリームを読み取り、テンプレート置換・PDF保存・印刷を実行する。
    /// </summary>
    /// <param name="excelInput">入力Excelファイルのストリーム。</param>
    /// <param name="variables">テンプレート置換用変数辞書。null の場合は置換なし。</param>
    /// <param name="options">レポート生成オプション。</param>
    /// <param name="outputPdfPath">保存先PDFファイルパス。</param>
    /// <param name="printOptions">印刷オプション。</param>
    public void Print(
        Stream excelInput,
        IReadOnlyDictionary<string, string>? variables,
        ReportOptions? options,
        string outputPdfPath,
        PrintOptions printOptions)
    {
        var excelOptions = options?.ExcelOptions;
        if (excelOptions == null && options?.SheetIndex != null)
        {
            excelOptions = new ExcelReadOptions { SheetIndex = options.SheetIndex };
        }

        var model = _reader.Read(excelInput, excelOptions);

        if (variables != null && variables.Count > 0)
        {
            model = _templateEngine.Process(model, variables);
        }

        model = _templateEngine.ClearKeywords(model);

        var dir = Path.GetDirectoryName(outputPdfPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using (var pdfStream = File.Create(outputPdfPath))
        {
            _pdfRenderer.Render(model, pdfStream, options?.PdfOptions);
        }

        _printer.Print(model, printOptions);
    }
}
