// <copyright file="NpoiExcelReader.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Reflection;
using Aloe.Apps.ExcelReportLib.Abstractions;
using Aloe.Apps.ExcelReportLib.Models;
using NPOI.OpenXmlFormats.Dml;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace Aloe.Apps.ExcelReportLib.Readers;

/// <summary>
/// NPOIを使用した <see cref="IExcelReader"/> の実装。
/// .xlsx 形式のExcelファイルからセル値・書式・罫線・結合セル・図形・画像を読み取る。
/// コメント・マクロ・数式は除外し、数式セルは評価済みの値を文字列として格納する。
/// </summary>
public class NpoiExcelReader : IExcelReader
{
    private const double DefaultColumnWidthPt = 48.0;
    private const double DefaultRowHeightPt = 15.0;

    /// <summary>NPOI列幅(1/256文字単位)をポイントに変換する概算係数。</summary>
    private const double ColumnWidthToPointsFactor = 7.0017 / 256.0 * 72.0 / 96.0;

    /// <inheritdoc />
    public SheetModel Read(Stream stream, ExcelReadOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var workbook = new XSSFWorkbook(stream);
        int sheetIndex = options?.SheetIndex ?? 0;
        var sheet = workbook.GetSheetAt(sheetIndex);

        var model = new SheetModel();

        ReadPageSettings(sheet, model);
        ReadColumnWidths(sheet, model);
        ReadRowHeights(sheet, model);
        ReadMergedRanges(sheet, model);
        ReadCells(sheet, workbook, model);
        ReadDrawings(sheet, model, workbook);

        return model;
    }

    private static void ReadPageSettings(ISheet sheet, SheetModel model)
    {
        var ps = sheet.PrintSetup;
        var settings = model.PageSettings;

        if (ps.Landscape)
        {
            settings.Orientation = PageOrientation.Landscape;
        }

        short paperSize = ps.PaperSize;
        SetPaperDimensions(settings, paperSize);

        var margins = sheet;
        if (margins.GetMargin(MarginType.TopMargin) > 0)
        {
            settings.MarginTopMm = margins.GetMargin(MarginType.TopMargin) * 25.4;
        }

        if (margins.GetMargin(MarginType.BottomMargin) > 0)
        {
            settings.MarginBottomMm = margins.GetMargin(MarginType.BottomMargin) * 25.4;
        }

        if (margins.GetMargin(MarginType.LeftMargin) > 0)
        {
            settings.MarginLeftMm = margins.GetMargin(MarginType.LeftMargin) * 25.4;
        }

        if (margins.GetMargin(MarginType.RightMargin) > 0)
        {
            settings.MarginRightMm = margins.GetMargin(MarginType.RightMargin) * 25.4;
        }

        if (settings.Orientation == PageOrientation.Landscape)
        {
            (settings.PaperWidthMm, settings.PaperHeightMm) = (settings.PaperHeightMm, settings.PaperWidthMm);
        }
    }

    private static void SetPaperDimensions(PageSettings settings, short paperSize)
    {
        // Excel paper size enumeration (common sizes)
        switch (paperSize)
        {
            case 1: // Letter
                settings.PaperWidthMm = 215.9;
                settings.PaperHeightMm = 279.4;
                break;
            case 5: // Legal
                settings.PaperWidthMm = 215.9;
                settings.PaperHeightMm = 355.6;
                break;
            case 8: // A3
                settings.PaperWidthMm = 297.0;
                settings.PaperHeightMm = 420.0;
                break;
            case 9: // A4
                settings.PaperWidthMm = 210.0;
                settings.PaperHeightMm = 297.0;
                break;
            case 11: // A5
                settings.PaperWidthMm = 148.0;
                settings.PaperHeightMm = 210.0;
                break;
            case 12: // B4 (JIS)
                settings.PaperWidthMm = 257.0;
                settings.PaperHeightMm = 364.0;
                break;
            case 13: // B5 (JIS)
                settings.PaperWidthMm = 182.0;
                settings.PaperHeightMm = 257.0;
                break;
            default:
                break;
        }
    }

    private static void ReadColumnWidths(ISheet sheet, SheetModel model)
    {
        int lastColumn = GetLastColumnIndex(sheet);
        for (int col = 0; col <= lastColumn; col++)
        {
            double widthUnits = sheet.GetColumnWidth(col);
            double widthPt = widthUnits * ColumnWidthToPointsFactor;
            model.ColumnWidths.Add(Math.Max(widthPt, 0));
        }
    }

    private static void ReadRowHeights(ISheet sheet, SheetModel model)
    {
        int lastRow = sheet.LastRowNum;
        for (int row = 0; row <= lastRow; row++)
        {
            var r = sheet.GetRow(row);
            double heightPt = r != null ? (double)r.HeightInPoints : DefaultRowHeightPt;
            model.RowHeights.Add(heightPt);
        }
    }

    private static void ReadMergedRanges(ISheet sheet, SheetModel model)
    {
        for (int i = 0; i < sheet.NumMergedRegions; i++)
        {
            var region = sheet.GetMergedRegion(i);
            model.MergedRanges.Add(new MergedRange
            {
                FirstRow = region.FirstRow,
                FirstColumn = region.FirstColumn,
                LastRow = region.LastRow,
                LastColumn = region.LastColumn,
            });
        }
    }

    private static void ReadCells(ISheet sheet, IWorkbook workbook, SheetModel model)
    {
        var formatter = new DataFormatter();
        var evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

        for (int rowIdx = 0; rowIdx <= sheet.LastRowNum; rowIdx++)
        {
            var row = sheet.GetRow(rowIdx);
            if (row == null)
            {
                continue;
            }

            for (int colIdx = 0; colIdx < row.LastCellNum; colIdx++)
            {
                var cell = row.GetCell(colIdx);
                if (cell == null)
                {
                    continue;
                }

                if (IsMergedSecondaryCell(model.MergedRanges, rowIdx, colIdx))
                {
                    ReadBorderOnlyCell(cell, rowIdx, colIdx, model);
                    continue;
                }

                var cellData = new CellData
                {
                    Row = rowIdx,
                    Column = colIdx,
                };

                cellData.Value = GetCellStringValue(cell, formatter, evaluator);
                ReadFont(cell, workbook, cellData);
                ReadAlignment(cell, cellData);
                ReadBorders(cell, cellData);
                ReadBackgroundColor(cell, cellData);

                model.Cells.Add(cellData);
            }
        }
    }

    private static bool IsMergedSecondaryCell(List<MergedRange> mergedRanges, int row, int col)
    {
        foreach (var range in mergedRanges)
        {
            if (range.Contains(row, col) && (row != range.FirstRow || col != range.FirstColumn))
            {
                return true;
            }
        }

        return false;
    }

    private static void ReadBorderOnlyCell(ICell cell, int row, int col, SheetModel model)
    {
        var cellData = new CellData
        {
            Row = row,
            Column = col,
        };

        ReadBorders(cell, cellData);

        bool hasBorder = cellData.Borders.Top.Style != BorderLineStyle.None
            || cellData.Borders.Bottom.Style != BorderLineStyle.None
            || cellData.Borders.Left.Style != BorderLineStyle.None
            || cellData.Borders.Right.Style != BorderLineStyle.None;

        if (hasBorder)
        {
            model.Cells.Add(cellData);
        }
    }

    private static string GetCellStringValue(ICell cell, DataFormatter formatter, IFormulaEvaluator evaluator)
    {
        if (cell.CellType == CellType.Formula)
        {
            try
            {
                var evaluated = evaluator.Evaluate(cell);
                return evaluated?.CellType switch
                {
                    CellType.String => evaluated.StringValue ?? string.Empty,
                    CellType.Numeric => formatter.FormatCellValue(cell, evaluator),
                    CellType.Boolean => evaluated.BooleanValue.ToString(),
                    _ => string.Empty,
                };
            }
            catch
            {
                return formatter.FormatCellValue(cell);
            }
        }

        return formatter.FormatCellValue(cell);
    }

    private static void ReadFont(ICell cell, IWorkbook workbook, CellData cellData)
    {
        var style = cell.CellStyle;
        if (style == null)
        {
            return;
        }

        var font = workbook.GetFontAt(style.FontIndex);
        if (font == null)
        {
            return;
        }

        bool isDoubleUnderline = font.Underline == FontUnderlineType.Double
            || font.Underline == FontUnderlineType.DoubleAccounting;

        cellData.Font = new FontData
        {
            Name = font.FontName ?? "Yu Gothic",
            SizeInPoints = font.FontHeightInPoints,
            Bold = font.IsBold,
            Italic = font.IsItalic,
            Underline = font.Underline != FontUnderlineType.None && !isDoubleUnderline,
            DoubleUnderline = isDoubleUnderline,
            Strikethrough = font.IsStrikeout,
            Color = GetFontColor(font, workbook),
        };
    }

    private static string GetFontColor(IFont font, IWorkbook workbook)
    {
        if (font is XSSFFont xssfFont)
        {
            var color = xssfFont.GetXSSFColor();
            if (color != null)
            {
                byte[] rgb = color.RGB;
                if (rgb != null && rgb.Length >= 3)
                {
                    return $"#FF{rgb[0]:X2}{rgb[1]:X2}{rgb[2]:X2}";
                }
            }
        }

        return "#FF000000";
    }

    private static void ReadAlignment(ICell cell, CellData cellData)
    {
        var style = cell.CellStyle;
        if (style == null)
        {
            return;
        }

        cellData.Alignment = new AlignmentData
        {
            Horizontal = ConvertHorizontalAlign(style.Alignment),
            Vertical = ConvertVerticalAlign(style.VerticalAlignment),
            WrapText = style.WrapText,
            Rotation = style.Rotation,
        };
    }

    private static HorizontalAlign ConvertHorizontalAlign(NPOI.SS.UserModel.HorizontalAlignment alignment)
    {
        return alignment switch
        {
            NPOI.SS.UserModel.HorizontalAlignment.Left => HorizontalAlign.Left,
            NPOI.SS.UserModel.HorizontalAlignment.Center => HorizontalAlign.Center,
            NPOI.SS.UserModel.HorizontalAlignment.Right => HorizontalAlign.Right,
            _ => HorizontalAlign.General,
        };
    }

    private static VerticalAlign ConvertVerticalAlign(NPOI.SS.UserModel.VerticalAlignment alignment)
    {
        return alignment switch
        {
            NPOI.SS.UserModel.VerticalAlignment.Top => VerticalAlign.Top,
            NPOI.SS.UserModel.VerticalAlignment.Center => VerticalAlign.Center,
            _ => VerticalAlign.Bottom,
        };
    }

    private static void ReadBorders(ICell cell, CellData cellData)
    {
        var style = cell.CellStyle;
        if (style == null)
        {
            return;
        }

        cellData.Borders = new CellBorderData
        {
            Top = new BorderEdge
            {
                Style = ConvertBorderStyle(style.BorderTop),
                Color = GetBorderColor(cell, BorderSide.Top),
            },
            Bottom = new BorderEdge
            {
                Style = ConvertBorderStyle(style.BorderBottom),
                Color = GetBorderColor(cell, BorderSide.Bottom),
            },
            Left = new BorderEdge
            {
                Style = ConvertBorderStyle(style.BorderLeft),
                Color = GetBorderColor(cell, BorderSide.Left),
            },
            Right = new BorderEdge
            {
                Style = ConvertBorderStyle(style.BorderRight),
                Color = GetBorderColor(cell, BorderSide.Right),
            },
        };
    }

    private static string GetBorderColor(ICell cell, BorderSide side)
    {
        if (cell.CellStyle is XSSFCellStyle xssfStyle)
        {
            var color = side switch
            {
                BorderSide.Top => xssfStyle.TopBorderXSSFColor,
                BorderSide.Bottom => xssfStyle.BottomBorderXSSFColor,
                BorderSide.Left => xssfStyle.LeftBorderXSSFColor,
                BorderSide.Right => xssfStyle.RightBorderXSSFColor,
                _ => null,
            };

            if (color != null)
            {
                byte[] rgb = color.RGB;
                if (rgb != null && rgb.Length >= 3)
                {
                    return $"#FF{rgb[0]:X2}{rgb[1]:X2}{rgb[2]:X2}";
                }
            }
        }

        return "#FF000000";
    }

    private static BorderLineStyle ConvertBorderStyle(NPOI.SS.UserModel.BorderStyle style)
    {
        return style switch
        {
            NPOI.SS.UserModel.BorderStyle.Thin => BorderLineStyle.Thin,
            NPOI.SS.UserModel.BorderStyle.Medium => BorderLineStyle.Medium,
            NPOI.SS.UserModel.BorderStyle.Thick => BorderLineStyle.Thick,
            NPOI.SS.UserModel.BorderStyle.Dashed => BorderLineStyle.Dashed,
            NPOI.SS.UserModel.BorderStyle.Dotted => BorderLineStyle.Dotted,
            NPOI.SS.UserModel.BorderStyle.Double => BorderLineStyle.Double,
            NPOI.SS.UserModel.BorderStyle.Hair => BorderLineStyle.Hair,
            NPOI.SS.UserModel.BorderStyle.MediumDashed => BorderLineStyle.Dashed,
            NPOI.SS.UserModel.BorderStyle.DashDot => BorderLineStyle.Dashed,
            NPOI.SS.UserModel.BorderStyle.MediumDashDot => BorderLineStyle.Dashed,
            NPOI.SS.UserModel.BorderStyle.DashDotDot => BorderLineStyle.Dotted,
            NPOI.SS.UserModel.BorderStyle.MediumDashDotDot => BorderLineStyle.Dotted,
            NPOI.SS.UserModel.BorderStyle.SlantedDashDot => BorderLineStyle.Dashed,
            _ => BorderLineStyle.None,
        };
    }

    private static void ReadBackgroundColor(ICell cell, CellData cellData)
    {
        if (cell.CellStyle is XSSFCellStyle xssfStyle)
        {
            var color = xssfStyle.FillForegroundXSSFColor;
            if (color != null)
            {
                byte[] rgb = color.RGB;
                if (rgb != null && rgb.Length >= 3)
                {
                    cellData.BackgroundColor = $"#FF{rgb[0]:X2}{rgb[1]:X2}{rgb[2]:X2}";
                }
            }
        }
    }

    private static void ReadDrawings(ISheet sheet, SheetModel model, XSSFWorkbook workbook)
    {
        if (sheet is not XSSFSheet xssfSheet)
        {
            return;
        }

        var drawing = xssfSheet.GetDrawingPatriarch();
        if (drawing == null)
        {
            return;
        }

        foreach (var shape in drawing.GetShapes())
        {
            if (shape is XSSFPicture picture)
            {
                ReadPicture(picture, model);
            }
            else if (shape is XSSFSimpleShape simpleShape)
            {
                ReadShape(simpleShape, model, workbook);
            }
        }
    }

    private static void ReadPicture(XSSFPicture picture, SheetModel model)
    {
        var anchor = picture.ClientAnchor;
        if (anchor == null)
        {
            return;
        }

        var pictureData = picture.PictureData;
        if (pictureData == null)
        {
            return;
        }

        model.Images.Add(new ImageData
        {
            From = CreateCellAnchorFrom(anchor),
            To = CreateCellAnchorTo(anchor),
            ImageBytes = pictureData.Data,
            MimeType = pictureData.MimeType ?? "image/png",
        });
    }

    private static readonly FieldInfo? SchemeClrItemsValueField =
        typeof(CT_SchemeColor).GetField("itemsValueField", BindingFlags.NonPublic | BindingFlags.Instance);

    private static void ReadShape(XSSFSimpleShape shape, SheetModel model, XSSFWorkbook workbook)
    {
        var anchor = shape.GetAnchor();
        if (anchor is not IClientAnchor clientAnchor)
        {
            return;
        }

        var shapeData = new ShapeData
        {
            ShapeType = ConvertShapeType(shape.ShapeType),
            From = CreateCellAnchorFrom(clientAnchor),
            To = CreateCellAnchorTo(clientAnchor),
            Text = shape.Text,
        };

        if (!shape.IsNoFill)
        {
            var ctShape = shape.GetCTShape();
            var solidFill = ctShape?.spPr?.solidFill;
            var srgbClr = solidFill?.srgbClr;

            if (srgbClr?.val != null && srgbClr.val.Length >= 3)
            {
                shapeData.FillColor = $"#FF{srgbClr.val[0]:X2}{srgbClr.val[1]:X2}{srgbClr.val[2]:X2}";
            }
            else if (solidFill?.schemeClr != null)
            {
                shapeData.FillColor = ResolveSchemeColor(solidFill.schemeClr, workbook);
            }
            else if (ctShape?.spPr?.noFill == null)
            {
                // spPr に solidFill も noFill もない場合 → style.fillRef から取得
                var fillRef = ctShape?.style?.fillRef;
                if (fillRef != null && fillRef.idx > 0)
                {
                    shapeData.FillColor = ResolveFillRefColor(fillRef, workbook);
                }
            }
        }

        model.Shapes.Add(shapeData);
    }

    private static string? ResolveFillRefColor(CT_StyleMatrixReference fillRef, XSSFWorkbook workbook)
    {
        if (fillRef.srgbClr?.val != null && fillRef.srgbClr.val.Length >= 3)
        {
            var v = fillRef.srgbClr.val;
            return $"#FF{v[0]:X2}{v[1]:X2}{v[2]:X2}";
        }

        if (fillRef.schemeClr != null)
        {
            return ResolveSchemeColor(fillRef.schemeClr, workbook);
        }

        if (fillRef.sysClr?.lastClr != null && fillRef.sysClr.lastClr.Length >= 3)
        {
            var v = fillRef.sysClr.lastClr;
            return $"#FF{v[0]:X2}{v[1]:X2}{v[2]:X2}";
        }

        return null;
    }

    private static string? ResolveSchemeColor(CT_SchemeColor schemeClr, XSSFWorkbook workbook)
    {
        int themeIdx = schemeClr.val switch
        {
            ST_SchemeColorVal.dk1 or ST_SchemeColorVal.tx1 => 0,
            ST_SchemeColorVal.lt1 or ST_SchemeColorVal.bg1 => 1,
            ST_SchemeColorVal.dk2 or ST_SchemeColorVal.tx2 => 2,
            ST_SchemeColorVal.lt2 or ST_SchemeColorVal.bg2 => 3,
            ST_SchemeColorVal.accent1 => 4,
            ST_SchemeColorVal.accent2 => 5,
            ST_SchemeColorVal.accent3 => 6,
            ST_SchemeColorVal.accent4 => 7,
            ST_SchemeColorVal.accent5 => 8,
            ST_SchemeColorVal.accent6 => 9,
            _ => -1,
        };

        if (themeIdx < 0)
        {
            return null;
        }

        var rgb = workbook.GetTheme()?.GetThemeColor(themeIdx)?.RGB;
        if (rgb == null || rgb.Length < 3)
        {
            return null;
        }

        var itemTypes = schemeClr.ItemsElementName;
        var itemValues = SchemeClrItemsValueField?.GetValue(schemeClr) as List<string> ?? [];

        int lumMod = 100000;
        int lumOff = 0;
        for (int i = 0; i < itemTypes.Count; i++)
        {
            var strVal = i < itemValues.Count ? itemValues[i] : null;
            if (strVal == null || !int.TryParse(strVal, out int intVal))
            {
                continue;
            }

            if (itemTypes[i] == EG_ColorTransform.lumMod)
            {
                lumMod = intVal;
            }
            else if (itemTypes[i] == EG_ColorTransform.lumOff)
            {
                lumOff = intVal;
            }
        }

        byte r = rgb[0];
        byte g = rgb[1];
        byte b = rgb[2];

        if (lumMod != 100000 || lumOff != 0)
        {
            (r, g, b) = ApplyLuminance(r, g, b, lumMod, lumOff);
        }

        return $"#FF{r:X2}{g:X2}{b:X2}";
    }

    private static (byte R, byte G, byte B) ApplyLuminance(byte r, byte g, byte b, int lumMod, int lumOff)
    {
        RgbToHls(r, g, b, out double h, out double l, out double s);
        double newL = (l * (lumMod / 100000.0)) + (lumOff / 100000.0);
        newL = Math.Clamp(newL, 0.0, 1.0);
        HlsToRgb(h, newL, s, out byte nr, out byte ng, out byte nb);
        return (nr, ng, nb);
    }

    private static void RgbToHls(byte r, byte g, byte b, out double h, out double l, out double s)
    {
        double rf = r / 255.0;
        double gf = g / 255.0;
        double bf = b / 255.0;

        double max = Math.Max(rf, Math.Max(gf, bf));
        double min = Math.Min(rf, Math.Min(gf, bf));
        double delta = max - min;

        l = (max + min) / 2.0;

        if (delta == 0.0)
        {
            h = 0.0;
            s = 0.0;
            return;
        }

        s = l < 0.5 ? delta / (max + min) : delta / (2.0 - max - min);

        if (max == rf)
        {
            h = ((gf - bf) / delta) % 6.0;
        }
        else if (max == gf)
        {
            h = ((bf - rf) / delta) + 2.0;
        }
        else
        {
            h = ((rf - gf) / delta) + 4.0;
        }

        h = (h * 60.0 + 360.0) % 360.0;
    }

    private static void HlsToRgb(double h, double l, double s, out byte r, out byte g, out byte b)
    {
        if (s == 0.0)
        {
            byte gray = (byte)Math.Round(l * 255.0);
            r = g = b = gray;
            return;
        }

        double q = l < 0.5 ? l * (1.0 + s) : (l + s) - (l * s);
        double p = (2.0 * l) - q;

        r = (byte)Math.Round(HueToChannel(p, q, h + 120.0) * 255.0);
        g = (byte)Math.Round(HueToChannel(p, q, h) * 255.0);
        b = (byte)Math.Round(HueToChannel(p, q, h - 120.0) * 255.0);
    }

    private static double HueToChannel(double p, double q, double t)
    {
        t = (t + 360.0) % 360.0;
        if (t < 60.0)
        {
            return p + ((q - p) * t / 60.0);
        }

        if (t < 180.0)
        {
            return q;
        }

        if (t < 240.0)
        {
            return p + ((q - p) * (240.0 - t) / 60.0);
        }

        return p;
    }

    private static CellAnchor CreateCellAnchorFrom(IClientAnchor anchor)
    {
        return new CellAnchor
        {
            Row = anchor.Row1,
            Column = anchor.Col1,
            RowOffsetEmu = anchor.Dy1,
            ColumnOffsetEmu = anchor.Dx1,
        };
    }

    private static CellAnchor CreateCellAnchorTo(IClientAnchor anchor)
    {
        return new CellAnchor
        {
            Row = anchor.Row2,
            Column = anchor.Col2,
            RowOffsetEmu = anchor.Dy2,
            ColumnOffsetEmu = anchor.Dx2,
        };
    }

    private static ShapeType ConvertShapeType(int npoiShapeType)
    {
        return npoiShapeType switch
        {
            1 => ShapeType.Rectangle,
            3 => ShapeType.Ellipse,
            20 => ShapeType.Line,
            _ => ShapeType.Other,
        };
    }

    private static int GetLastColumnIndex(ISheet sheet)
    {
        int maxCol = 0;
        for (int i = 0; i <= sheet.LastRowNum; i++)
        {
            var row = sheet.GetRow(i);
            if (row != null && row.LastCellNum > maxCol)
            {
                maxCol = row.LastCellNum;
            }
        }

        return Math.Max(maxCol - 1, 0);
    }

    private enum BorderSide
    {
        Top,
        Bottom,
        Left,
        Right,
    }
}
