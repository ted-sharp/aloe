// <copyright file="ExcelReportService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportLib.Abstractions;
using Aloe.Apps.ExcelReportLib.Models;

namespace Aloe.Apps.ExcelReportLib.Services;

/// <summary>
/// Excel方眼紙テンプレートからPDFを生成するパイプラインを統括するオーケストレーター。
/// IExcelReader → ITemplateEngine → IPdfRenderer の順に処理を実行する。
/// </summary>
public class ExcelReportService
{
    private readonly IExcelReader _reader;
    private readonly IPdfRenderer _renderer;
    private readonly ITemplateEngine _templateEngine;

    /// <summary>
    /// <see cref="ExcelReportService"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    /// <param name="reader">Excel読み取り実装。</param>
    /// <param name="renderer">PDF描画実装。</param>
    /// <param name="templateEngine">テンプレート置換エンジン。</param>
    public ExcelReportService(IExcelReader reader, IPdfRenderer renderer, ITemplateEngine templateEngine)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _templateEngine = templateEngine ?? throw new ArgumentNullException(nameof(templateEngine));
    }

    /// <summary>
    /// Excelファイルからシートを読み取り、テンプレート置換を行い、PDFを出力する。
    /// </summary>
    /// <param name="excelInput">入力Excelファイル(.xlsx)のストリーム。</param>
    /// <param name="pdfOutput">PDF出力先のストリーム。</param>
    /// <param name="variables">テンプレート置換用の変数辞書。null の場合は置換なし。</param>
    /// <param name="options">レポート生成オプション。null の場合はデフォルト。</param>
    /// <param name="clearKeywords">true の場合、残存する <c>${...}</c> プレースホルダを空文字列に消去する。</param>
    public void GeneratePdf(
        Stream excelInput,
        Stream pdfOutput,
        IReadOnlyDictionary<string, string>? variables = null,
        ReportOptions? options = null,
        bool clearKeywords = false)
    {
        ArgumentNullException.ThrowIfNull(excelInput);
        ArgumentNullException.ThrowIfNull(pdfOutput);

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

        if (clearKeywords)
        {
            model = _templateEngine.ClearKeywords(model);
        }

        _renderer.Render(model, pdfOutput, options?.PdfOptions);
    }

    /// <summary>
    /// Excelファイルのパスを指定してPDFファイルを生成する。
    /// </summary>
    /// <param name="excelPath">入力Excelファイルのパス。</param>
    /// <param name="pdfPath">出力PDFファイルのパス。</param>
    /// <param name="variables">テンプレート置換用の変数辞書。null の場合は置換なし。</param>
    /// <param name="options">レポート生成オプション。null の場合はデフォルト。</param>
    public void GeneratePdf(
        string excelPath,
        string pdfPath,
        IReadOnlyDictionary<string, string>? variables = null,
        ReportOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(excelPath);
        ArgumentException.ThrowIfNullOrEmpty(pdfPath);

        using var excelStream = File.OpenRead(excelPath);
        using var pdfStream = File.Create(pdfPath);
        GeneratePdf(excelStream, pdfStream, variables, options);
    }
}
