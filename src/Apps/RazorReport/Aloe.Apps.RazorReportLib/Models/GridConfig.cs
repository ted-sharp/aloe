namespace Aloe.Apps.RazorReportLib.Models;

/// <summary>
/// グリッドの設定（列数、行数）
/// </summary>
public record GridConfig(int Columns, int Rows)
{
    /// <summary>
    /// グリッドの列数
    /// </summary>
    public int Columns { get; } = Columns;

    /// <summary>
    /// グリッドの行数
    /// </summary>
    public int Rows { get; } = Rows;

    /// <summary>
    /// A4縦書き標準設定
    /// </summary>
    public static GridConfig A4Portrait => new(36, 51);
}
