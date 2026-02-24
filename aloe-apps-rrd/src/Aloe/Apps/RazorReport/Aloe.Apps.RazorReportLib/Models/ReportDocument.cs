namespace Aloe.Apps.RazorReportLib.Models;

/// <summary>
/// レポートドキュメント全体を管理するクラス
/// </summary>
public class ReportDocument
{
    /// <summary>
    /// ドキュメントの一意識別子
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ドキュメントのタイトル
    /// </summary>
    public string Title { get; set; } = "Untitled Report";

    /// <summary>
    /// グリッド設定
    /// </summary>
    public GridConfig GridConfig { get; set; } = GridConfig.A4Portrait;

    /// <summary>
    /// 用紙サイズ
    /// </summary>
    public PageSize PageSize { get; set; } = PageSize.A4;

    /// <summary>
    /// デザイン要素のコレクション
    /// </summary>
    public List<DesignElement> Elements { get; set; } = new();

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
