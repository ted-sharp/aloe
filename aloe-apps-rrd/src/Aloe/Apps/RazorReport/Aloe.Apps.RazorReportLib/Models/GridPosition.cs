namespace Aloe.Apps.RazorReportLib.Models;

/// <summary>
/// グリッド上の位置とサイズを定義する
/// </summary>
public record GridPosition(int Column, int Row, int ColumnSpan = 1, int RowSpan = 1)
{
    /// <summary>
    /// 列番号（0-indexed）
    /// </summary>
    public int Column { get; } = Column;

    /// <summary>
    /// 行番号（0-indexed）
    /// </summary>
    public int Row { get; } = Row;

    /// <summary>
    /// 列スパン
    /// </summary>
    public int ColumnSpan { get; } = ColumnSpan;

    /// <summary>
    /// 行スパン
    /// </summary>
    public int RowSpan { get; } = RowSpan;
}
