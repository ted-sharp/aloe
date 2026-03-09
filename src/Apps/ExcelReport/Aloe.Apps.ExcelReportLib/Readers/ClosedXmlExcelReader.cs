// <copyright file="ClosedXmlExcelReader.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportLib.Abstractions;
using Aloe.Apps.ExcelReportLib.Models;
using ClosedXML.Excel;

namespace Aloe.Apps.ExcelReportLib.Readers;

/// <summary>
/// ClosedXMLを使用した <see cref="IExcelReader"/> の実装。
/// セル値・書式・罫線・結合セルを読み取る。
/// ClosedXMLは図形(Shape)のサポートが限定的なため、図形が必要な場合はNPOI版を推奨。
/// </summary>
public class ClosedXmlExcelReader : IExcelReader
{
    private const double DefaultRowHeightPt = 15.0;

    /// <summary>ClosedXML列幅(文字単位)をポイントに変換する概算係数。</summary>
    private const double CharacterWidthToPointsFactor = 7.0017 * 72.0 / 96.0;

    /// <inheritdoc />
    public SheetModel Read(Stream stream, ExcelReadOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var workbook = new XLWorkbook(stream);
        int sheetIndex = (options?.SheetIndex ?? 0) + 1;
        var worksheet = workbook.Worksheet(sheetIndex);

        var model = new SheetModel();

        ReadPageSettings(worksheet, model);
        ReadColumnWidths(worksheet, model);
        ReadRowHeights(worksheet, model);
        ReadMergedRanges(worksheet, model);
        ReadCells(worksheet, model);

        return model;
    }

    private static void ReadPageSettings(IXLWorksheet worksheet, SheetModel model)
    {
        var settings = model.PageSettings;
        var pageSetup = worksheet.PageSetup;

        if (pageSetup.PageOrientation == XLPageOrientation.Landscape)
        {
            settings.Orientation = PageOrientation.Landscape;
        }

        SetPaperDimensions(settings, pageSetup.PaperSize);

        var margins = pageSetup.Margins;
        settings.MarginTopMm = margins.Top * 25.4;
        settings.MarginBottomMm = margins.Bottom * 25.4;
        settings.MarginLeftMm = margins.Left * 25.4;
        settings.MarginRightMm = margins.Right * 25.4;

        if (settings.Orientation == PageOrientation.Landscape)
        {
            (settings.PaperWidthMm, settings.PaperHeightMm) = (settings.PaperHeightMm, settings.PaperWidthMm);
        }
    }

    private static void SetPaperDimensions(PageSettings settings, XLPaperSize paperSize)
    {
        switch (paperSize)
        {
            case XLPaperSize.LetterPaper:
                settings.PaperWidthMm = 215.9;
                settings.PaperHeightMm = 279.4;
                break;
            case XLPaperSize.LegalPaper:
                settings.PaperWidthMm = 215.9;
                settings.PaperHeightMm = 355.6;
                break;
            case XLPaperSize.A3Paper:
                settings.PaperWidthMm = 297.0;
                settings.PaperHeightMm = 420.0;
                break;
            case XLPaperSize.A4Paper:
                settings.PaperWidthMm = 210.0;
                settings.PaperHeightMm = 297.0;
                break;
            case XLPaperSize.A5Paper:
                settings.PaperWidthMm = 148.0;
                settings.PaperHeightMm = 210.0;
                break;
            case XLPaperSize.B4Paper:
                settings.PaperWidthMm = 257.0;
                settings.PaperHeightMm = 364.0;
                break;
            case XLPaperSize.B5Paper:
                settings.PaperWidthMm = 182.0;
                settings.PaperHeightMm = 257.0;
                break;
            default:
                break;
        }
    }

    private static void ReadColumnWidths(IXLWorksheet worksheet, SheetModel model)
    {
        int lastColumn = GetLastColumnIndex(worksheet);
        for (int col = 1; col <= lastColumn; col++)
        {
            double width = worksheet.Column(col).Width;
            double widthPt = width * CharacterWidthToPointsFactor;
            model.ColumnWidths.Add(Math.Max(widthPt, 0));
        }
    }

    private static void ReadRowHeights(IXLWorksheet worksheet, SheetModel model)
    {
        int lastRow = GetLastRowIndex(worksheet);
        for (int row = 1; row <= lastRow; row++)
        {
            double height = worksheet.Row(row).Height;
            model.RowHeights.Add(height > 0 ? height : DefaultRowHeightPt);
        }
    }

    private static void ReadMergedRanges(IXLWorksheet worksheet, SheetModel model)
    {
        foreach (var range in worksheet.MergedRanges)
        {
            model.MergedRanges.Add(new MergedRange
            {
                FirstRow = range.FirstRow().RowNumber() - 1,
                FirstColumn = range.FirstColumn().ColumnNumber() - 1,
                LastRow = range.LastRow().RowNumber() - 1,
                LastColumn = range.LastColumn().ColumnNumber() - 1,
            });
        }
    }

    private static void ReadCells(IXLWorksheet worksheet, SheetModel model)
    {
        foreach (var cell in worksheet.CellsUsed())
        {
            int row = cell.Address.RowNumber - 1;
            int col = cell.Address.ColumnNumber - 1;

            if (IsMergedSecondaryCell(model.MergedRanges, row, col))
            {
                ReadBorderOnlyCell(cell, row, col, model);
                continue;
            }

            var cellData = new CellData
            {
                Row = row,
                Column = col,
            };

            cellData.Value = GetCellStringValue(cell);
            ReadFont(cell, cellData);
            ReadAlignment(cell, cellData);
            ReadBorders(cell, cellData);
            ReadBackgroundColor(cell, cellData);

            model.Cells.Add(cellData);
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

    private static void ReadBorderOnlyCell(IXLCell cell, int row, int col, SheetModel model)
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

    private static string GetCellStringValue(IXLCell cell)
    {
        if (cell.HasFormula)
        {
            try
            {
                return cell.CachedValue.ToString();
            }
            catch
            {
                return cell.GetFormattedString();
            }
        }

        return cell.GetFormattedString();
    }

    private static void ReadFont(IXLCell cell, CellData cellData)
    {
        var font = cell.Style.Font;
        cellData.Font = new FontData
        {
            Name = font.FontName ?? "Yu Gothic",
            SizeInPoints = font.FontSize,
            Bold = font.Bold,
            Italic = font.Italic,
            Underline = font.Underline != XLFontUnderlineValues.None,
            Strikethrough = font.Strikethrough,
            Color = ConvertXlColor(font.FontColor),
        };
    }

    private static void ReadAlignment(IXLCell cell, CellData cellData)
    {
        var alignment = cell.Style.Alignment;
        cellData.Alignment = new AlignmentData
        {
            Horizontal = ConvertHorizontalAlign(alignment.Horizontal),
            Vertical = ConvertVerticalAlign(alignment.Vertical),
            WrapText = alignment.WrapText,
            Rotation = alignment.TextRotation,
        };
    }

    private static void ReadBorders(IXLCell cell, CellData cellData)
    {
        var border = cell.Style.Border;
        cellData.Borders = new CellBorderData
        {
            Top = new BorderEdge
            {
                Style = ConvertBorderStyle(border.TopBorder),
                Color = ConvertXlColor(border.TopBorderColor),
            },
            Bottom = new BorderEdge
            {
                Style = ConvertBorderStyle(border.BottomBorder),
                Color = ConvertXlColor(border.BottomBorderColor),
            },
            Left = new BorderEdge
            {
                Style = ConvertBorderStyle(border.LeftBorder),
                Color = ConvertXlColor(border.LeftBorderColor),
            },
            Right = new BorderEdge
            {
                Style = ConvertBorderStyle(border.RightBorder),
                Color = ConvertXlColor(border.RightBorderColor),
            },
        };
    }

    private static void ReadBackgroundColor(IXLCell cell, CellData cellData)
    {
        var fill = cell.Style.Fill;
        if (fill.PatternType != XLFillPatternValues.None)
        {
            cellData.BackgroundColor = ConvertXlColor(fill.BackgroundColor);
        }
    }

    private static HorizontalAlign ConvertHorizontalAlign(XLAlignmentHorizontalValues alignment)
    {
        return alignment switch
        {
            XLAlignmentHorizontalValues.Left => HorizontalAlign.Left,
            XLAlignmentHorizontalValues.Center => HorizontalAlign.Center,
            XLAlignmentHorizontalValues.Right => HorizontalAlign.Right,
            _ => HorizontalAlign.General,
        };
    }

    private static VerticalAlign ConvertVerticalAlign(XLAlignmentVerticalValues alignment)
    {
        return alignment switch
        {
            XLAlignmentVerticalValues.Top => VerticalAlign.Top,
            XLAlignmentVerticalValues.Center => VerticalAlign.Center,
            _ => VerticalAlign.Bottom,
        };
    }

    private static BorderLineStyle ConvertBorderStyle(XLBorderStyleValues style)
    {
        return style switch
        {
            XLBorderStyleValues.Thin => BorderLineStyle.Thin,
            XLBorderStyleValues.Medium => BorderLineStyle.Medium,
            XLBorderStyleValues.Thick => BorderLineStyle.Thick,
            XLBorderStyleValues.Dashed => BorderLineStyle.Dashed,
            XLBorderStyleValues.Dotted => BorderLineStyle.Dotted,
            XLBorderStyleValues.Double => BorderLineStyle.Double,
            XLBorderStyleValues.Hair => BorderLineStyle.Hair,
            XLBorderStyleValues.MediumDashed => BorderLineStyle.Dashed,
            XLBorderStyleValues.DashDot => BorderLineStyle.Dashed,
            XLBorderStyleValues.MediumDashDot => BorderLineStyle.Dashed,
            XLBorderStyleValues.DashDotDot => BorderLineStyle.Dotted,
            XLBorderStyleValues.MediumDashDotDot => BorderLineStyle.Dotted,
            XLBorderStyleValues.SlantDashDot => BorderLineStyle.Dashed,
            _ => BorderLineStyle.None,
        };
    }

    private static string ConvertXlColor(XLColor color)
    {
        if (color == null || color.ColorType == XLColorType.Indexed)
        {
            return "#FF000000";
        }

        try
        {
            var c = color.Color;
            return $"#FF{c.R:X2}{c.G:X2}{c.B:X2}";
        }
        catch
        {
            return "#FF000000";
        }
    }

    private static int GetLastColumnIndex(IXLWorksheet worksheet)
    {
        var lastCell = worksheet.LastCellUsed();
        return lastCell?.Address.ColumnNumber ?? 1;
    }

    private static int GetLastRowIndex(IXLWorksheet worksheet)
    {
        var lastCell = worksheet.LastCellUsed();
        return lastCell?.Address.RowNumber ?? 1;
    }
}
