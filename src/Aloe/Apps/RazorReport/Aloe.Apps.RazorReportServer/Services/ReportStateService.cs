using Aloe.Apps.RazorReportLib.Models;

namespace Aloe.Apps.RazorReportServer.Services;

/// <summary>
/// レポートの状態管理を行うサービス
/// </summary>
public class ReportStateService
{
    /// <summary>
    /// 現在のレポートドキュメント
    /// </summary>
    public ReportDocument Document { get; private set; }

    /// <summary>
    /// グリッド表示フラグ
    /// </summary>
    public bool ShowGrid { get; set; } = true;

    /// <summary>
    /// 未保存の変更があるかどうか
    /// </summary>
    public bool IsModified { get; private set; } = false;

    /// <summary>
    /// 状態変更時のイベント
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    /// コンストラクタ - 初期データを生成
    /// </summary>
    public ReportStateService()
    {
        Document = new ReportDocument();
        Document.Elements.Add(new TextElement
        {
            Position = new GridPosition(2, 2, 12, 2),
            Text = "Sample Text",
            FontFamily = "Arial",
            FontSize = 14
        });
    }

    /// <summary>
    /// 状態変更を通知する
    /// </summary>
    public void NotifyStateChanged()
    {
        MarkAsModified();
        OnChange?.Invoke();
    }

    /// <summary>
    /// 新規ドキュメントを作成する
    /// </summary>
    public void CreateNewDocument()
    {
        Document = new ReportDocument();
        IsModified = false;
        OnChange?.Invoke();
    }

    /// <summary>
    /// ドキュメントを読込む
    /// </summary>
    public void LoadDocument(ReportDocument document)
    {
        Document = document;
        IsModified = false;
        OnChange?.Invoke();
    }

    /// <summary>
    /// ドキュメントを変更済みとしてマークする
    /// </summary>
    public void MarkAsModified()
    {
        IsModified = true;
        Document.UpdatedAt = DateTime.Now;
    }

    /// <summary>
    /// ドキュメントを保存済みとしてマークする
    /// </summary>
    public void MarkAsSaved()
    {
        IsModified = false;
    }
}
