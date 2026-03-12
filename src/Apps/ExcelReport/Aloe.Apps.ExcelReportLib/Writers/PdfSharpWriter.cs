// <copyright file="PdfSharpWriter.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportLib.Abstractions;
using Aloe.Apps.ExcelReportLib.Models;
using Aloe.Apps.ExcelReportLib.Services;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Aloe.Apps.ExcelReportLib.Writers;

/// <summary>
/// PDFsharpを使用した <see cref="IPdfRenderer"/> の実装。
/// <see cref="XGraphics"/> による絶対座標ベースの描画で、
/// Excel方眼紙のレイアウトを忠実にPDFへ再現する。
/// </summary>
public class PdfSharpWriter : IPdfRenderer
{
    private readonly PositionCalculator _calc = new();

    /// <inheritdoc />
    public void Render(SheetModel model, Stream output, PdfRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(output);

        using var document = new PdfDocument();
        var page = document.AddPage();

        page.Width = XUnit.FromPoint(model.PageSettings.PaperWidthPt);
        page.Height = XUnit.FromPoint(model.PageSettings.PaperHeightPt);

        using var gfx = XGraphics.FromPdfPage(page);

        double marginLeft = model.PageSettings.MarginLeftPt;
        double marginTop = model.PageSettings.MarginTopPt;

        var state = gfx.Save();
        gfx.TranslateTransform(marginLeft, marginTop);

        DrawBackgrounds(gfx, model);
        DrawBorders(gfx, model);
        DrawCellTexts(gfx, model);
        DrawShapes(gfx, model);
        DrawImages(gfx, model);

        gfx.Restore(state);

        document.Save(output);
    }

    private void DrawBackgrounds(XGraphics gfx, SheetModel model)
    {
        foreach (var cell in model.Cells)
        {
            if (string.IsNullOrEmpty(cell.BackgroundColor))
            {
                continue;
            }

            var merged = FindMergedRange(model, cell.Row, cell.Column);
            var rect = _calc.GetCellRect(model, cell.Row, cell.Column, merged);
            var brush = new XSolidBrush(ParseColor(cell.BackgroundColor));
            gfx.DrawRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }
    }

    private void DrawBorders(XGraphics gfx, SheetModel model)
    {
        foreach (var cell in model.Cells)
        {
            var rect = _calc.GetCellRect(model, cell.Row, cell.Column);

            DrawBorderEdge(gfx, cell.Borders.Top, rect.X, rect.Y, rect.X + rect.Width, rect.Y);
            DrawBorderEdge(gfx, cell.Borders.Bottom, rect.X, rect.Y + rect.Height, rect.X + rect.Width, rect.Y + rect.Height);
            DrawBorderEdge(gfx, cell.Borders.Left, rect.X, rect.Y, rect.X, rect.Y + rect.Height);
            DrawBorderEdge(gfx, cell.Borders.Right, rect.X + rect.Width, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
        }
    }

    private static void DrawBorderEdge(XGraphics gfx, BorderEdge edge, double x1, double y1, double x2, double y2)
    {
        if (edge.Style == BorderLineStyle.None)
        {
            return;
        }

        double width = GetBorderWidth(edge.Style);
        var pen = new XPen(ParseColor(edge.Color), width);
        SetPenDashStyle(pen, edge.Style);

        gfx.DrawLine(pen, x1, y1, x2, y2);
    }

    private void DrawCellTexts(XGraphics gfx, SheetModel model)
    {
        foreach (var cell in model.Cells)
        {
            if (string.IsNullOrEmpty(cell.Value))
            {
                continue;
            }

            var merged = FindMergedRange(model, cell.Row, cell.Column);
            var rect = _calc.GetCellRect(model, cell.Row, cell.Column, merged);

            var fontStyle = XFontStyleEx.Regular;
            if (cell.Font.Bold && cell.Font.Italic)
            {
                fontStyle = XFontStyleEx.BoldItalic;
            }
            else if (cell.Font.Bold)
            {
                fontStyle = XFontStyleEx.Bold;
            }
            else if (cell.Font.Italic)
            {
                fontStyle = XFontStyleEx.Italic;
            }

            var font = new XFont(cell.Font.Name, cell.Font.SizeInPoints, fontStyle);
            var brush = new XSolidBrush(ParseColor(cell.Font.Color));

            const double padding = 2.0;
            double maxWidth = rect.Width - (padding * 2);
            double lineHeight = font.GetHeight();

            var lines = WrapLines(cell.Value, maxWidth, s => gfx.MeasureString(s, font).Width, cell.Alignment.WrapText);
            double totalHeight = lines.Count * lineHeight;

            double topY = cell.Alignment.Vertical switch
            {
                VerticalAlign.Top => rect.Y + padding,
                VerticalAlign.Center => rect.Y + ((rect.Height - totalHeight) / 2),
                _ => rect.Y + rect.Height - padding - totalHeight,
            };

            var hFormat = new XStringFormat
            {
                Alignment = cell.Alignment.Horizontal switch
                {
                    HorizontalAlign.Left => XStringAlignment.Near,
                    HorizontalAlign.Center => XStringAlignment.Center,
                    HorizontalAlign.Right => XStringAlignment.Far,
                    _ => XStringAlignment.Near,
                },
                LineAlignment = XLineAlignment.Near,
            };

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                double lineTopY = topY + (i * lineHeight);
                var lineRect = new XRect(rect.X + padding, lineTopY, Math.Max(maxWidth, 0), lineHeight);
                gfx.DrawString(line, font, brush, lineRect, hFormat);

                double textWidth = gfx.MeasureString(line, font).Width;
                double baseline = lineTopY + (lineHeight * 0.8);
                double decoStartX = cell.Alignment.Horizontal switch
                {
                    HorizontalAlign.Center => rect.X + padding + ((maxWidth - textWidth) / 2),
                    HorizontalAlign.Right => rect.X + padding + maxWidth - textWidth,
                    _ => rect.X + padding,
                };

                DrawTextDecorations(gfx, cell.Font, decoStartX, baseline, textWidth);
            }
        }
    }

    private static List<string> WrapLines(string text, double maxWidth, Func<string, double> measureFn, bool wrapText)
    {
        var result = new List<string>();
        foreach (var hardLine in text.Split('\n'))
        {
            if (!wrapText || maxWidth <= 0 || measureFn(hardLine) <= maxWidth)
            {
                result.Add(hardLine);
                continue;
            }

            var current = new System.Text.StringBuilder();
            foreach (char c in hardLine)
            {
                current.Append(c);
                if (current.Length > 1 && measureFn(current.ToString()) > maxWidth)
                {
                    result.Add(current.ToString()[..^1]);
                    current.Clear();
                    current.Append(c);
                }
            }

            if (current.Length > 0)
            {
                result.Add(current.ToString());
            }
        }

        return result;
    }

    private static void DrawTextDecorations(XGraphics gfx, FontData fontData, double startX, double baseline, double textWidth)
    {
        if (!fontData.Underline && !fontData.DoubleUnderline && !fontData.Strikethrough)
        {
            return;
        }

        double fontSize = fontData.SizeInPoints;
        var pen = new XPen(ParseColor(fontData.Color), 0.5);

        if (fontData.Underline)
        {
            double y = baseline + (fontSize * 0.12);
            gfx.DrawLine(pen, startX, y, startX + textWidth, y);
        }

        if (fontData.DoubleUnderline)
        {
            double y1 = baseline + (fontSize * 0.12);
            double y2 = baseline + (fontSize * 0.28);
            gfx.DrawLine(pen, startX, y1, startX + textWidth, y1);
            gfx.DrawLine(pen, startX, y2, startX + textWidth, y2);
        }

        if (fontData.Strikethrough)
        {
            double y = baseline - (fontSize * 0.3);
            gfx.DrawLine(pen, startX, y, startX + textWidth, y);
        }
    }

    private void DrawShapes(XGraphics gfx, SheetModel model)
    {
        foreach (var shape in model.Shapes)
        {
            var rect = _calc.GetAnchorRect(model, shape.From, shape.To);

            XPen? pen = null;
            if (shape.LineWidth > 0)
            {
                pen = new XPen(ParseColor(shape.LineColor), shape.LineWidth);
            }

            XBrush? brush = null;
            if (!string.IsNullOrEmpty(shape.FillColor))
            {
                brush = new XSolidBrush(ParseColor(shape.FillColor));
            }

            switch (shape.ShapeType)
            {
                case ShapeType.Rectangle:
                case ShapeType.RoundedRectangle:
                    DrawRectShape(gfx, rect, pen, brush);
                    break;
                case ShapeType.Ellipse:
                    DrawEllipseShape(gfx, rect, pen, brush);
                    break;
                case ShapeType.Line:
                    if (pen != null)
                    {
                        gfx.DrawLine(pen, rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
                    }

                    break;
                default:
                    DrawRectShape(gfx, rect, pen, brush);
                    break;
            }

            if (!string.IsNullOrEmpty(shape.Text))
            {
                DrawShapeText(gfx, shape, rect);
            }
        }
    }

    private static void DrawRectShape(XGraphics gfx, RectD rect, XPen? pen, XBrush? brush)
    {
        var xRect = new XRect(rect.X, rect.Y, rect.Width, rect.Height);
        if (brush != null && pen != null)
        {
            gfx.DrawRectangle(pen, brush, xRect);
        }
        else if (brush != null)
        {
            gfx.DrawRectangle(brush, xRect);
        }
        else if (pen != null)
        {
            gfx.DrawRectangle(pen, xRect);
        }
    }

    private static void DrawEllipseShape(XGraphics gfx, RectD rect, XPen? pen, XBrush? brush)
    {
        var xRect = new XRect(rect.X, rect.Y, rect.Width, rect.Height);
        if (brush != null && pen != null)
        {
            gfx.DrawEllipse(pen, brush, xRect);
        }
        else if (brush != null)
        {
            gfx.DrawEllipse(brush, xRect);
        }
        else if (pen != null)
        {
            gfx.DrawEllipse(pen, xRect);
        }
    }

    private static void DrawShapeText(XGraphics gfx, ShapeData shape, RectD rect)
    {
        var fontData = shape.TextFont ?? new FontData();
        var font = new XFont(fontData.Name, fontData.SizeInPoints, XFontStyleEx.Regular);
        var brush = new XSolidBrush(ParseColor(fontData.Color));
        var format = new XStringFormat
        {
            Alignment = XStringAlignment.Center,
            LineAlignment = XLineAlignment.Center,
        };

        gfx.DrawString(shape.Text!, font, brush, new XRect(rect.X, rect.Y, rect.Width, rect.Height), format);
    }

    private void DrawImages(XGraphics gfx, SheetModel model)
    {
        foreach (var image in model.Images)
        {
            if (image.ImageBytes.Length == 0)
            {
                continue;
            }

            var rect = _calc.GetAnchorRect(model, image.From, image.To);

            try
            {
                using var ms = new MemoryStream(image.ImageBytes);
                var xImage = XImage.FromStream(ms);
                gfx.DrawImage(xImage, rect.X, rect.Y, rect.Width, rect.Height);
            }
            catch
            {
                // 読み込み不可の画像はスキップ
            }
        }
    }

    private static double GetBorderWidth(BorderLineStyle style)
    {
        return style switch
        {
            BorderLineStyle.Thin => 0.5,
            BorderLineStyle.Medium => 1.0,
            BorderLineStyle.Thick => 2.0,
            BorderLineStyle.Double => 0.5,
            BorderLineStyle.Hair => 0.25,
            BorderLineStyle.Dashed => 0.5,
            BorderLineStyle.Dotted => 0.5,
            _ => 0.5,
        };
    }

    private static void SetPenDashStyle(XPen pen, BorderLineStyle style)
    {
        switch (style)
        {
            case BorderLineStyle.Dashed:
                pen.DashStyle = XDashStyle.Dash;
                break;
            case BorderLineStyle.Dotted:
                pen.DashStyle = XDashStyle.Dot;
                break;
            case BorderLineStyle.Hair:
                pen.DashStyle = XDashStyle.Dot;
                break;
            default:
                pen.DashStyle = XDashStyle.Solid;
                break;
        }
    }

    private static MergedRange? FindMergedRange(SheetModel model, int row, int col)
    {
        foreach (var range in model.MergedRanges)
        {
            if (range.FirstRow == row && range.FirstColumn == col)
            {
                return range;
            }
        }

        return null;
    }

    private static XColor ParseColor(string argb)
    {
        if (string.IsNullOrEmpty(argb) || argb.Length < 7)
        {
            return XColors.Black;
        }

        try
        {
            string hex = argb.TrimStart('#');
            if (hex.Length == 8)
            {
                byte a = Convert.ToByte(hex[..2], 16);
                byte r = Convert.ToByte(hex[2..4], 16);
                byte g = Convert.ToByte(hex[4..6], 16);
                byte b = Convert.ToByte(hex[6..8], 16);
                return XColor.FromArgb(a, r, g, b);
            }
            else if (hex.Length == 6)
            {
                byte r = Convert.ToByte(hex[..2], 16);
                byte g = Convert.ToByte(hex[2..4], 16);
                byte b = Convert.ToByte(hex[4..6], 16);
                return XColor.FromArgb(r, g, b);
            }
        }
        catch
        {
            // パース失敗時は黒
        }

        return XColors.Black;
    }
}
