// <copyright file="PositionCalculator.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportLib.Models;

namespace Aloe.Apps.ExcelReportLib.Services;

/// <summary>
/// セルの列幅・行高の累積からポイント(pt)単位の絶対座標を計算する。
/// EMUオフセットのポイント変換や結合セルの矩形計算もこのクラスで行う。
/// </summary>
public class PositionCalculator
{
    /// <summary>1 EMU = 1/12700 pt。</summary>
    private const double PointsPerEmu = 1.0 / 12700.0;

    /// <summary>
    /// 指定列の左端X座標(pt)を返す。列0〜columnIndex-1の幅の合計。
    /// </summary>
    public double GetColumnLeft(SheetModel model, int columnIndex)
    {
        ArgumentNullException.ThrowIfNull(model);

        double x = 0;
        int count = Math.Min(columnIndex, model.ColumnWidths.Count);
        for (int i = 0; i < count; i++)
        {
            x += model.ColumnWidths[i];
        }

        return x;
    }

    /// <summary>
    /// 指定行の上端Y座標(pt)を返す。行0〜rowIndex-1の高さの合計。
    /// </summary>
    public double GetRowTop(SheetModel model, int rowIndex)
    {
        ArgumentNullException.ThrowIfNull(model);

        double y = 0;
        int count = Math.Min(rowIndex, model.RowHeights.Count);
        for (int i = 0; i < count; i++)
        {
            y += model.RowHeights[i];
        }

        return y;
    }

    /// <summary>
    /// 指定列の幅(pt)を返す。範囲外の場合はデフォルト幅(8.43文字幅相当)を返す。
    /// </summary>
    public double GetColumnWidth(SheetModel model, int columnIndex)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (columnIndex >= 0 && columnIndex < model.ColumnWidths.Count)
        {
            return model.ColumnWidths[columnIndex];
        }

        return 48.0;
    }

    /// <summary>
    /// 指定行の高さ(pt)を返す。範囲外の場合はデフォルト行高(15pt)を返す。
    /// </summary>
    public double GetRowHeight(SheetModel model, int rowIndex)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (rowIndex >= 0 && rowIndex < model.RowHeights.Count)
        {
            return model.RowHeights[rowIndex];
        }

        return 15.0;
    }

    /// <summary>
    /// <see cref="CellAnchor"/> からポイント(pt)単位の絶対座標を計算する。
    /// セル左上の座標にEMUオフセット分を加算する。
    /// </summary>
    public PointD GetAbsolutePosition(SheetModel model, CellAnchor anchor)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(anchor);

        double x = GetColumnLeft(model, anchor.Column) + EmuToPoints(anchor.ColumnOffsetEmu);
        double y = GetRowTop(model, anchor.Row) + EmuToPoints(anchor.RowOffsetEmu);
        return new PointD(x, y);
    }

    /// <summary>
    /// セルの矩形(pt)を返す。結合セルが指定された場合は結合範囲全体の矩形を返す。
    /// </summary>
    public RectD GetCellRect(SheetModel model, int row, int col, MergedRange? merged = null)
    {
        ArgumentNullException.ThrowIfNull(model);

        int startRow = merged?.FirstRow ?? row;
        int startCol = merged?.FirstColumn ?? col;
        int endRow = merged?.LastRow ?? row;
        int endCol = merged?.LastColumn ?? col;

        double x = GetColumnLeft(model, startCol);
        double y = GetRowTop(model, startRow);

        double width = 0;
        for (int c = startCol; c <= endCol; c++)
        {
            width += GetColumnWidth(model, c);
        }

        double height = 0;
        for (int r = startRow; r <= endRow; r++)
        {
            height += GetRowHeight(model, r);
        }

        return new RectD(x, y, width, height);
    }

    /// <summary>
    /// 図形またはオブジェクトのTwo-Cell Anchorから矩形(pt)を計算する。
    /// </summary>
    public RectD GetAnchorRect(SheetModel model, CellAnchor from, CellAnchor to)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        var topLeft = GetAbsolutePosition(model, from);
        var bottomRight = GetAbsolutePosition(model, to);

        return new RectD(
            topLeft.X,
            topLeft.Y,
            bottomRight.X - topLeft.X,
            bottomRight.Y - topLeft.Y);
    }

    /// <summary>
    /// EMU(English Metric Units)をポイント(pt)に変換する。
    /// </summary>
    public static double EmuToPoints(long emu)
    {
        return emu * PointsPerEmu;
    }
}
