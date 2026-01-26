using Aloe.Apps.RazorReportLib.Models;

namespace Aloe.Apps.RazorReportServer.Services;

/// <summary>
/// グリッド座標計算を行うサービス
/// </summary>
public class GridCalculationService
{
    private readonly PageConfigService _pageConfigService;

    public GridCalculationService(PageConfigService pageConfigService)
    {
        _pageConfigService = pageConfigService;
    }

    private int Columns => _pageConfigService.CurrentColumns;
    private int Rows => _pageConfigService.CurrentRows;
    private double WidthPx => _pageConfigService.CurrentWidthPx;
    private double HeightPx => _pageConfigService.CurrentHeightPx;

    /// <summary>
    /// セルの幅（ピクセル）
    /// </summary>
    public double CellWidthPx => WidthPx / Columns;

    /// <summary>
    /// セルの高さ（ピクセル）
    /// </summary>
    public double CellHeightPx => HeightPx / Rows;

    /// <summary>
    /// マウス座標をグリッドセルに変換する
    /// </summary>
    public (int column, int row) PixelToGridCell(double x, double y)
    {
        int column = (int)Math.Floor(x / this.CellWidthPx);
        int row = (int)Math.Floor(y / this.CellHeightPx);

        return (
            Math.Max(0, Math.Min(column, Columns - 1)),
            Math.Max(0, Math.Min(row, Rows - 1))
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
               position.Column + position.ColumnSpan <= Columns &&
               position.Row + position.RowSpan <= Rows;
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

    /// <summary>
    /// マウス座標をグリッドセルに変換する（コンテナオフセット考慮）
    /// </summary>
    public (int column, int row) MousePositionToGridCell(double mouseX, double mouseY, double containerLeft, double containerTop, double zoom)
    {
        // ズームを考慮してマウス座標を相対座標に変換
        double relativeX = (mouseX - containerLeft) / zoom;
        double relativeY = (mouseY - containerTop) / zoom;

        // グリッドセルに変換
        int column = (int)Math.Floor(relativeX / this.CellWidthPx);
        int row = (int)Math.Floor(relativeY / this.CellHeightPx);

        // 有効な範囲内にクランプ
        return (
            Math.Max(0, Math.Min(column, Columns - 1)),
            Math.Max(0, Math.Min(row, Rows - 1))
        );
    }

    /// <summary>
    /// ドラッグ範囲からグリッド位置を計算する
    /// </summary>
    public GridPosition CalculatePlacementRange(int startCol, int startRow, int endCol, int endRow)
    {
        int minCol = Math.Min(startCol, endCol);
        int minRow = Math.Min(startRow, endRow);
        int maxCol = Math.Max(startCol, endCol);
        int maxRow = Math.Max(startRow, endRow);

        int columnSpan = Math.Max(1, maxCol - minCol + 1);
        int rowSpan = Math.Max(1, maxRow - minRow + 1);

        return new GridPosition(minCol, minRow, columnSpan, rowSpan);
    }
}
