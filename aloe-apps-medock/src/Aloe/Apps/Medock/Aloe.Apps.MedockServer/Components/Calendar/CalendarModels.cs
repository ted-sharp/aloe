namespace Aloe.Apps.MedockServer.Components.Calendar;

/// <summary>
/// 時間帯別統計データ
/// </summary>
public class TimeSlotStats
{
    /// <summary>時間範囲（"HH:mm-HH:mm"形式）</summary>
    public string Time { get; set; } = String.Empty;
    public int Count { get; set; }
    public int Cap { get; set; }
    public bool IsGrayedOut { get; set; }

    /// <summary>
    /// 条件検索に合致する予約数（検索・設備条件フィルター用）
    /// </summary>
    public int FilteredCount { get; set; }
}
