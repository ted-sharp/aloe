// <copyright file="QuestPdfWriter.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportLib.Abstractions;
using Aloe.Apps.ExcelReportLib.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace Aloe.Apps.ExcelReportLib.Writers;

/// <summary>
/// QuestPDFを使用した <see cref="IPdfRenderer"/> の実装。
/// Canvas APIによる絶対座標描画でExcel方眼紙レイアウトをPDFに再現する。
/// </summary>
public class QuestPdfWriter : IPdfRenderer
{
    private readonly SkiaSheetRenderer _renderer;
    private readonly ILogger<QuestPdfWriter> _logger;

    /// <summary>
    /// <see cref="QuestPdfWriter"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    /// <param name="renderer">Skia 描画レンダラー。</param>
    /// <param name="logger">ロガー。省略時は NullLogger を使用する。</param>
    public QuestPdfWriter(SkiaSheetRenderer renderer, ILogger<QuestPdfWriter>? logger = null)
    {
        _renderer = renderer;
        _logger = logger ?? NullLogger<QuestPdfWriter>.Instance;
    }

    /// <inheritdoc />
    public void Render(SheetModel model, Stream output, PdfRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(output);

        QuestPDF.Settings.License = LicenseType.Community;

        var pageSettings = model.PageSettings;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(
                    (float)pageSettings.PaperWidthMm,
                    (float)pageSettings.PaperHeightMm,
                    Unit.Millimetre);

                page.MarginTop((float)pageSettings.MarginTopMm, Unit.Millimetre);
                page.MarginBottom((float)pageSettings.MarginBottomMm, Unit.Millimetre);
                page.MarginLeft((float)pageSettings.MarginLeftMm, Unit.Millimetre);
                page.MarginRight((float)pageSettings.MarginRightMm, Unit.Millimetre);

                page.Content().SkiaSharpSvgCanvas((SKCanvas canvas, Size _) =>
                {
                    _renderer.Render(canvas, model);
                });
            });
        }).GeneratePdf(output);
    }
}
