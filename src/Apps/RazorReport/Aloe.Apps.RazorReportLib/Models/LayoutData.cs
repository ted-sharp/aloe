namespace Aloe.Apps.RazorReportLib.Models;

/// <summary>
/// レイアウトデータ
/// </summary>
public class LayoutData
{
    public string Version { get; set; } = "1.0";
    public GridSettings Grid { get; set; } = new();
    public PageSize PageSize { get; set; } = new();
    public List<DesignElement> Elements { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// グリッド設定
/// </summary>
public class GridSettings
{
    public int Columns { get; set; } = 36;
    public int Rows { get; set; } = 51;
    public double ColumnWidth { get; set; } = 22.05;
    public double RowHeight { get; set; } = 22.05;
}
