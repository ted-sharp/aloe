using System.Text.Json.Serialization;

namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 予約スロットアイテム
/// 個別のスロット定義を表す
/// </summary>
public class AppointmentSlotItem
{
    /// <summary>開始時刻（分単位、0:00からの分数）</summary>
    [JsonPropertyName("start")]
    public int Start { get; set; }

    /// <summary>終了時刻（分単位、0:00からの分数）</summary>
    [JsonPropertyName("end")]
    public int End { get; set; }

    /// <summary>期間（End - Start、分単位）</summary>
    [JsonPropertyName("duration")]
    public int Duration => this.End - this.Start;

    /// <summary>最大予約数（キャパシティ）</summary>
    [JsonPropertyName("cap")]
    public int Cap { get; set; }

    /// <summary>時間外スロットかどうか</summary>
    [JsonPropertyName("isOutsideHours")]
    public bool IsOutsideHours { get; set; } = false;
}

