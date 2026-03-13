// <copyright file="WindowsSheetPrinter.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.Versioning;
using Aloe.Apps.ExcelReportLib.Abstractions;
using Aloe.Apps.ExcelReportLib.Models;
using Aloe.Apps.ExcelReportLib.Writers;
using SkiaSharp;

namespace Aloe.Apps.ExcelReportLib.Printers;

/// <summary>
/// SkiaSharp でシートをビットマップに描画し、Windows プリントスプーラーに送信する <see cref="ISheetPrinter"/> 実装。
/// </summary>
[SupportedOSPlatform("windows")]
public class WindowsSheetPrinter : ISheetPrinter
{
    private readonly SkiaSheetRenderer _renderer;

    /// <summary>
    /// <see cref="WindowsSheetPrinter"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    /// <param name="renderer">Skia 描画レンダラー。</param>
    public WindowsSheetPrinter(SkiaSheetRenderer renderer)
    {
        _renderer = renderer;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetInstalledPrinters()
    {
        var printers = new List<string>();
        foreach (string name in PrinterSettings.InstalledPrinters)
        {
            printers.Add(name);
        }

        return printers;
    }

    /// <inheritdoc />
    public void Print(SheetModel model, PrintOptions options)
    {
        var ps = model.PageSettings;
        double contentWidthMm = ps.PaperWidthMm - ps.MarginLeftMm - ps.MarginRightMm;
        double contentHeightMm = ps.PaperHeightMm - ps.MarginTopMm - ps.MarginBottomMm;

        int widthPx = (int)(contentWidthMm / 25.4 * options.Dpi);
        int heightPx = (int)(contentHeightMm / 25.4 * options.Dpi);

        using var surface = SKSurface.Create(new SKImageInfo(widthPx, heightPx));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        canvas.Scale(options.Dpi / 72f);
        _renderer.Render(canvas, model);

        using var skImage = surface.Snapshot();
        using var encoded = skImage.Encode(SKEncodedImageFormat.Png, 100);
        using var bitmapStream = encoded.AsStream();
        using var bitmap = new Bitmap(bitmapStream);

        using var doc = new PrintDocument();
        doc.PrinterSettings.PrinterName = options.PrinterName;
        doc.PrinterSettings.Copies = (short)Math.Max(1, options.Copies);

        int paperWidthHundredths = (int)(ps.PaperWidthMm / 25.4 * 100);
        int paperHeightHundredths = (int)(ps.PaperHeightMm / 25.4 * 100);
        doc.DefaultPageSettings.PaperSize = new PaperSize("Custom", paperWidthHundredths, paperHeightHundredths);
        doc.DefaultPageSettings.Margins = new Margins(
            (int)(ps.MarginLeftMm / 25.4 * 100),
            (int)(ps.MarginRightMm / 25.4 * 100),
            (int)(ps.MarginTopMm / 25.4 * 100),
            (int)(ps.MarginBottomMm / 25.4 * 100));

        doc.PrintPage += (_, e) =>
        {
            e.Graphics!.DrawImage(bitmap, e.MarginBounds);
        };

        doc.Print();
    }
}
