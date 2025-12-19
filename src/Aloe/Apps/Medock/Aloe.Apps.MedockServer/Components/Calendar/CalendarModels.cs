namespace Aloe.Apps.MedockServer.Components.Calendar;

/// <summary>
/// 日別統計データ（パイチャート用）
/// </summary>
public class CalendarDayStats
{
    public int AmCount { get; set; }
    public int PmCount { get; set; }
    public int AmMax { get; set; } = 10;
    public int PmMax { get; set; } = 10;

    /// <summary>
    /// 時間帯ごとの統計データ
    /// </summary>
    public List<TimeSlotStats>? Slots { get; set; }

    /// <summary>
    /// グレーアウト対象かどうか（検索フィルター用）
    /// </summary>
    public bool IsGrayedOut { get; set; }
}

/// <summary>
/// 時間帯別統計データ
/// </summary>
public class TimeSlotStats
{
    public string Time { get; set; } = String.Empty;
    public int Count { get; set; }
    public int Max { get; set; }
    public bool IsGrayedOut { get; set; }

    /// <summary>
    /// 条件検索に合致する予約数（検索・設備条件フィルター用）
    /// </summary>
    public int FilteredCount { get; set; }
}
