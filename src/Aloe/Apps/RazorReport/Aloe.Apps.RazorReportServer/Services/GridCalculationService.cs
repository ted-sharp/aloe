using Aloe.Apps.RazorReportLib.Models;

namespace Aloe.Apps.RazorReportServer.Services;

/// <summary>
/// グリッド座標計算を行うサービス
/// </summary>
public class GridCalculationService
{
    private const int COLUMNS = 36;
    private const int ROWS = 51;
    private const double WIDTH_PX = 793.7;
    private const double HEIGHT_PX = 1122.52;

    /// <summary>
    /// セルの幅（ピクセル）
    /// </summary>
    public double CellWidthPx => WIDTH_PX / COLUMNS;   // ~22.05px

    /// <summary>
    /// セルの高さ（ピクセル）
    /// </summary>
    public double CellHeightPx => HEIGHT_PX / ROWS;    // ~22.01px

    /// <summary>
    /// マウス座標をグリッドセルに変換する
    /// </summary>
    public (int column, int row) PixelToGridCell(double x, double y)
    {
        int column = (int)Math.Floor(x / this.CellWidthPx);
        int row = (int)Math.Floor(y / this.CellHeightPx);

        return (
            Math.Max(0, Math.Min(column, COLUMNS - 1)),
            Math.Max(0, Math.Min(row, ROWS - 1))
        );
    }

    /// <summary>
    /// グリッドセル座標をピクセル座標に変換する
    /// </summary>
    public (double x, double y) GridCellToPixel(int column, int row)
    {
        return (column * this.CellWidthPx, row * this.CellHeightPx);
    }

    /// <summary>
    /// グリッド位置の差分を計算する
    /// </summary>
    public (int columnDelta, int rowDelta) CalculateDelta(
        GridPosition from, GridPosition to)
    {
        return (to.Column - from.Column, to.Row - from.Row);
    }

    /// <summary>
    /// 要素の位置が有効な範囲内かどうかを検証する
    /// </summary>
    public bool ValidatePosition(GridPosition position)
    {
        return position.Column >= 0 &&
               position.Row >= 0 &&
               position.Column + position.ColumnSpan <= COLUMNS &&
               position.Row + position.RowSpan <= ROWS;
    }

    /// <summary>
    /// リサイズハンドルに基づいて新しい位置を計算する
    /// </summary>
    public GridPosition CalculateResizedPosition(
        GridPosition original,
        ResizeHandle handle,
        int newColumn,
        int newRow)
    {
        return handle switch
        {
            ResizeHandle.TopLeft => new GridPosition(
                newColumn,
                newRow,
                Math.Max(1, original.Column + original.ColumnSpan - newColumn),
                Math.Max(1, original.Row + original.RowSpan - newRow)
            ),
            ResizeHandle.Top => new GridPosition(
                original.Column,
                newRow,
                original.ColumnSpan,
                Math.Max(1, original.Row + original.RowSpan - newRow)
            ),
            ResizeHandle.TopRight => new GridPosition(
                original.Column,
                newRow,
                Math.Max(1, newColumn - original.Column + 1),
                Math.Max(1, original.Row + original.RowSpan - newRow)
            ),
            ResizeHandle.Left => new GridPosition(
                newColumn,
                original.Row,
                Math.Max(1, original.Column + original.ColumnSpan - newColumn),
                original.RowSpan
            ),
            ResizeHandle.Right => new GridPosition(
                original.Column,
                original.Row,
                Math.Max(1, newColumn - original.Column + 1),
                original.RowSpan
            ),
            ResizeHandle.BottomLeft => new GridPosition(
                newColumn,
                original.Row,
                Math.Max(1, original.Column + original.ColumnSpan - newColumn),
                Math.Max(1, newRow - original.Row + 1)
            ),
            ResizeHandle.Bottom => new GridPosition(
                original.Column,
                original.Row,
                original.ColumnSpan,
                Math.Max(1, newRow - original.Row + 1)
            ),
            ResizeHandle.BottomRight => new GridPosition(
                original.Column,
                original.Row,
                Math.Max(1, newColumn - original.Column + 1),
                Math.Max(1, newRow - original.Row + 1)
            ),
            _ => original
        };
    }
}
