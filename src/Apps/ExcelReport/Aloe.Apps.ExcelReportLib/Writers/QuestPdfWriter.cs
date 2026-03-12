// <copyright file="QuestPdfWriter.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportLib.Abstractions;
using Aloe.Apps.ExcelReportLib.Models;
using Aloe.Apps.ExcelReportLib.Services;
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
    private readonly PositionCalculator _calc = new();
    private readonly SystemFontResolver _fontResolver = new();
    private readonly ILogger<QuestPdfWriter> _logger;

    /// <summary>
    /// <see cref="QuestPdfWriter"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    /// <param name="logger">ロガー。省略時は NullLogger を使用する。</param>
    public QuestPdfWriter(ILogger<QuestPdfWriter>? logger = null)
    {
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

                page.Content().SkiaSharpSvgCanvas((SKCanvas canvas, Size size) =>
                {
                    DrawBackgrounds(canvas, model);
                    DrawBorders(canvas, model);
                    DrawCellTexts(canvas, model);
                    DrawShapes(canvas, model);
                    DrawImages(canvas, model);
                });
            });
        }).GeneratePdf(output);
    }

    private void DrawBackgrounds(SKCanvas canvas, SheetModel model)
    {
        foreach (var cell in model.Cells)
        {
            if (string.IsNullOrEmpty(cell.BackgroundColor))
            {
                continue;
            }

            var merged = FindMergedRange(model, cell.Row, cell.Column);
            var rect = _calc.GetCellRect(model, cell.Row, cell.Column, merged);

            using var paint = new SKPaint
            {
                Color = ParseSkColor(cell.BackgroundColor),
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
            };

            canvas.DrawRect((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, paint);
        }
    }

    private void DrawBorders(SKCanvas canvas, SheetModel model)
    {
        foreach (var cell in model.Cells)
        {
            var rect = _calc.GetCellRect(model, cell.Row, cell.Column);

            DrawBorderEdge(canvas, cell.Borders.Top,
                (float)rect.X, (float)rect.Y, (float)(rect.X + rect.Width), (float)rect.Y);
            DrawBorderEdge(canvas, cell.Borders.Bottom,
                (float)rect.X, (float)(rect.Y + rect.Height), (float)(rect.X + rect.Width), (float)(rect.Y + rect.Height));
            DrawBorderEdge(canvas, cell.Borders.Left,
                (float)rect.X, (float)rect.Y, (float)rect.X, (float)(rect.Y + rect.Height));
            DrawBorderEdge(canvas, cell.Borders.Right,
                (float)(rect.X + rect.Width), (float)rect.Y, (float)(rect.X + rect.Width), (float)(rect.Y + rect.Height));
        }
    }

    private static void DrawBorderEdge(SKCanvas canvas, BorderEdge edge, float x1, float y1, float x2, float y2)
    {
        if (edge.Style == BorderLineStyle.None)
        {
            return;
        }

        using var paint = new SKPaint
        {
            Color = ParseSkColor(edge.Color),
            StrokeWidth = GetBorderWidth(edge.Style),
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
        };

        SetPathEffect(paint, edge.Style);

        canvas.DrawLine(x1, y1, x2, y2, paint);
    }

    private void DrawCellTexts(SKCanvas canvas, SheetModel model)
    {
        foreach (var cell in model.Cells)
        {
            if (string.IsNullOrEmpty(cell.Value))
            {
                continue;
            }

            var merged = FindMergedRange(model, cell.Row, cell.Column);
            var rect = _calc.GetCellRect(model, cell.Row, cell.Column, merged);

            using var typeface = ResolveTypeface(cell.Font.Name, cell.Font.Bold, cell.Font.Italic);

            using var font = new SKFont(typeface, (float)cell.Font.SizeInPoints);
            using var paint = new SKPaint
            {
                Color = ParseSkColor(cell.Font.Color),
                IsAntialias = true,
            };

            const float padding = 2.0f;
            float maxWidth = (float)rect.Width - (padding * 2);
            var metrics = font.Metrics;
            float lineHeight = metrics.Descent - metrics.Ascent;

            var lines = WrapLines(cell.Value, maxWidth, s => font.MeasureText(s), cell.Alignment.WrapText);
            float totalTextHeight = lines.Count * lineHeight;

            float firstBaselineY = cell.Alignment.Vertical switch
            {
                VerticalAlign.Top => (float)rect.Y + padding - metrics.Ascent,
                VerticalAlign.Center => (float)(rect.Y + (rect.Height / 2) - (totalTextHeight / 2)) - metrics.Ascent,
                _ => (float)(rect.Y + rect.Height - padding - metrics.Descent) - ((lines.Count - 1) * lineHeight),
            };

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                float lineY = firstBaselineY + (i * lineHeight);
                float lineTextWidth = font.MeasureText(line);

                float x = cell.Alignment.Horizontal switch
                {
                    HorizontalAlign.Center => (float)(rect.X + (rect.Width / 2)),
                    HorizontalAlign.Right => (float)(rect.X + rect.Width - padding),
                    _ => (float)rect.X + padding,
                };

                var textAlign = cell.Alignment.Horizontal switch
                {
                    HorizontalAlign.Center => SKTextAlign.Center,
                    HorizontalAlign.Right => SKTextAlign.Right,
                    _ => SKTextAlign.Left,
                };

                canvas.DrawText(line, x, lineY, textAlign, font, paint);

                float decoStartX = cell.Alignment.Horizontal switch
                {
                    HorizontalAlign.Center => x - (lineTextWidth / 2),
                    HorizontalAlign.Right => x - lineTextWidth,
                    _ => x,
                };

                DrawTextDecorations(canvas, cell.Font, decoStartX, lineY, lineTextWidth, metrics, paint);
            }
        }
    }

    private static List<string> WrapLines(string text, float maxWidth, Func<string, float> measureFn, bool wrapText)
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

    private static void DrawTextDecorations(SKCanvas canvas, FontData fontData, float startX, float baselineY, float textWidth, SKFontMetrics metrics, SKPaint basePaint)
    {
        if (!fontData.Underline && !fontData.DoubleUnderline && !fontData.Strikethrough)
        {
            return;
        }

        float fontSize = (float)fontData.SizeInPoints;
        using var pen = new SKPaint
        {
            Color = basePaint.Color,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
        };

        float underlinePos = metrics.UnderlinePosition.GetValueOrDefault(fontSize * 0.12f);
        float underlineThick = metrics.UnderlineThickness.GetValueOrDefault(1f);
        float strikePos = metrics.StrikeoutPosition.GetValueOrDefault(-(fontSize * 0.3f));
        float strikeThick = metrics.StrikeoutThickness.GetValueOrDefault(1f);

        if (fontData.Underline)
        {
            pen.StrokeWidth = underlineThick;
            canvas.DrawLine(startX, baselineY + underlinePos, startX + textWidth, baselineY + underlinePos, pen);
        }

        if (fontData.DoubleUnderline)
        {
            pen.StrokeWidth = underlineThick;
            canvas.DrawLine(startX, baselineY + underlinePos, startX + textWidth, baselineY + underlinePos, pen);
            canvas.DrawLine(startX, baselineY + underlinePos + (underlineThick * 2.5f), startX + textWidth, baselineY + underlinePos + (underlineThick * 2.5f), pen);
        }

        if (fontData.Strikethrough)
        {
            pen.StrokeWidth = strikeThick;
            canvas.DrawLine(startX, baselineY + strikePos, startX + textWidth, baselineY + strikePos, pen);
        }
    }

    private void DrawShapes(SKCanvas canvas, SheetModel model)
    {
        foreach (var shape in model.Shapes)
        {
            var rect = _calc.GetAnchorRect(model, shape.From, shape.To);
            var skRect = new SKRect((float)rect.X, (float)rect.Y, (float)(rect.X + rect.Width), (float)(rect.Y + rect.Height));

            if (!string.IsNullOrEmpty(shape.FillColor))
            {
                using var fillPaint = new SKPaint
                {
                    Color = ParseSkColor(shape.FillColor),
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true,
                };

                DrawShapeGeometry(canvas, shape.ShapeType, skRect, fillPaint);
            }

            if (shape.LineWidth > 0)
            {
                using var strokePaint = new SKPaint
                {
                    Color = ParseSkColor(shape.LineColor),
                    StrokeWidth = (float)shape.LineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true,
                };

                DrawShapeGeometry(canvas, shape.ShapeType, skRect, strokePaint);
            }

            if (!string.IsNullOrEmpty(shape.Text))
            {
                DrawShapeText(canvas, shape, rect);
            }
        }
    }

    private static void DrawShapeGeometry(SKCanvas canvas, ShapeType type, SKRect rect, SKPaint paint)
    {
        switch (type)
        {
            case ShapeType.Rectangle:
            case ShapeType.RoundedRectangle:
                canvas.DrawRect(rect, paint);
                break;
            case ShapeType.Ellipse:
                canvas.DrawOval(rect, paint);
                break;
            case ShapeType.Line:
                canvas.DrawLine(rect.Left, rect.Top, rect.Right, rect.Bottom, paint);
                break;
            default:
                canvas.DrawRect(rect, paint);
                break;
        }
    }

    private SKTypeface ResolveTypeface(string name, bool bold, bool italic)
    {
        string? path = _fontResolver.GetFontFilePath(name, bold);
        if (path != null)
        {
            var tf = SKTypeface.FromFile(path);
            if (tf != null)
            {
                return tf;
            }

            _logger.LogWarning("フォントファイルのロードに失敗しました: {Path} (font={Name}, bold={Bold})", path, name, bold);
        }
        else
        {
            _logger.LogWarning("フォントが解決できませんでした: {Name} (bold={Bold}) — SKTypeface.FromFamilyName にフォールバックします", name, bold);
        }

        var typeface = SKTypeface.FromFamilyName(
            name,
            bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal,
            italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);

        if (typeface == null)
        {
            _logger.LogWarning("SKTypeface.FromFamilyName も null を返しました: {Name} — デフォルトフォントを使用します", name);
            return SKTypeface.Default;
        }

        return typeface;
    }

    private void DrawShapeText(SKCanvas canvas, ShapeData shape, RectD rect)
    {
        var fontData = shape.TextFont ?? new FontData();

        using var typeface = ResolveTypeface(fontData.Name, fontData.Bold, fontData.Italic);
        using var font = new SKFont(typeface, (float)fontData.SizeInPoints);
        using var paint = new SKPaint
        {
            Color = ParseSkColor(fontData.Color),
            IsAntialias = true,
        };

        var metrics = font.Metrics;
        float y = (float)(rect.Y + (rect.Height / 2)) - ((metrics.Ascent + metrics.Descent) / 2);
        float x = (float)(rect.X + (rect.Width / 2));

        canvas.DrawText(shape.Text!, x, y, SKTextAlign.Center, font, paint);
    }

    private void DrawImages(SKCanvas canvas, SheetModel model)
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
                using var skImage = SKImage.FromEncodedData(image.ImageBytes);
                if (skImage != null)
                {
                    var destRect = new SKRect(
                        (float)rect.X, (float)rect.Y,
                        (float)(rect.X + rect.Width), (float)(rect.Y + rect.Height));
                    canvas.DrawImage(skImage, destRect);
                }
            }
            catch
            {
                // 読み込み不可の画像はスキップ
            }
        }
    }

    private static float GetBorderWidth(BorderLineStyle style)
    {
        return style switch
        {
            BorderLineStyle.Thin => 0.5f,
            BorderLineStyle.Medium => 1.0f,
            BorderLineStyle.Thick => 2.0f,
            BorderLineStyle.Double => 0.5f,
            BorderLineStyle.Hair => 0.25f,
            BorderLineStyle.Dashed => 0.5f,
            BorderLineStyle.Dotted => 0.5f,
            _ => 0.5f,
        };
    }

    private static void SetPathEffect(SKPaint paint, BorderLineStyle style)
    {
        switch (style)
        {
            case BorderLineStyle.Dashed:
                paint.PathEffect = SKPathEffect.CreateDash([4f, 2f], 0);
                break;
            case BorderLineStyle.Dotted:
                paint.PathEffect = SKPathEffect.CreateDash([1f, 1f], 0);
                break;
            case BorderLineStyle.Hair:
                paint.PathEffect = SKPathEffect.CreateDash([1f, 1f], 0);
                break;
            default:
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

    private static SKColor ParseSkColor(string argb)
    {
        if (string.IsNullOrEmpty(argb) || argb.Length < 7)
        {
            return SKColors.Black;
        }

        if (SKColor.TryParse(argb, out var color))
        {
            return color;
        }

        return SKColors.Black;
    }
}
