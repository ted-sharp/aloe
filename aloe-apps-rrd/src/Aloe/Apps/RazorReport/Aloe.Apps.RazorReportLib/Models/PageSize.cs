namespace Aloe.Apps.RazorReportLib.Models;

/// <summary>
/// 用紙サイズの定義（mm単位で管理、px単位で提供）
/// </summary>
public class PageSize
{
    /// <summary>
    /// 用紙名
    /// </summary>
    public string Name { get; set; } = String.Empty;

    /// <summary>
    /// 用紙幅（mm）
    /// </summary>
    public double WidthMm { get; set; }

    /// <summary>
    /// 用紙高さ（mm）
    /// </summary>
    public double HeightMm { get; set; }

    /// <summary>
    /// 用紙幅（px）- 96 DPI基準
    /// </summary>
    public double WidthPx => this.WidthMm * 96 / 25.4;

    /// <summary>
    /// 用紙高さ（px）- 96 DPI基準
    /// </summary>
    public double HeightPx => this.HeightMm * 96 / 25.4;

    /// <summary>
    /// A4用紙（210mm × 297mm）
    /// </summary>
    public static PageSize A4 => new() { Name = "A4", WidthMm = 210, HeightMm = 297 };

    /// <summary>
    /// A4横（297mm × 210mm）
    /// </summary>
    public static PageSize A4Landscape => new() { Name = "A4 Landscape", WidthMm = 297, HeightMm = 210 };
}
