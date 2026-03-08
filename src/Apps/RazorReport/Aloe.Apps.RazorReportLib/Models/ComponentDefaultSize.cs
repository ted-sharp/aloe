namespace Aloe.Apps.RazorReportLib.Models;

/// <summary>
/// コンポーネントのデフォルトサイズ設定
/// </summary>
public class ComponentDefaultSize
{
    /// <summary>
    /// コンポーネントのタイプ（"Text", "Image", "Table", "Container"など）
    /// </summary>
    public string ComponentType { get; set; } = String.Empty;

    /// <summary>
    /// デフォルトの列スパン
    /// </summary>
    public int ColumnSpan { get; set; } = 6;

    /// <summary>
    /// デフォルトの行スパン
    /// </summary>
    public int RowSpan { get; set; } = 1;

    /// <summary>
    /// テーブルのデフォルト列数
    /// </summary>
    public int? DefaultColumns { get; set; }

    /// <summary>
    /// テーブルのデフォルト行数
    /// </summary>
    public int? DefaultRows { get; set; }

    /// <summary>
    /// テキストのデフォルトテキスト
    /// </summary>
    public string? DefaultText { get; set; }
}
