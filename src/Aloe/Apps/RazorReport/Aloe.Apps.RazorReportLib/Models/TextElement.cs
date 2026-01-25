namespace Aloe.Apps.RazorReportLib.Models;

/// <summary>
/// テキスト要素
/// </summary>
public class TextElement : DesignElement
{
    /// <summary>
    /// 表示するテキスト
    /// </summary>
    public string Text { get; set; } = String.Empty;

    /// <summary>
    /// フォントファミリー
    /// </summary>
    public string FontFamily { get; set; } = "Arial";

    /// <summary>
    /// フォントサイズ（pt）
    /// </summary>
    public int FontSize { get; set; } = 12;

    /// <summary>
    /// 太字か
    /// </summary>
    public bool IsBold { get; set; } = false;

    /// <summary>
    /// イタリックか
    /// </summary>
    public bool IsItalic { get; set; } = false;

    /// <summary>
    /// 要素のタイプ名
    /// </summary>
    public override string ElementType => "Text";
}
