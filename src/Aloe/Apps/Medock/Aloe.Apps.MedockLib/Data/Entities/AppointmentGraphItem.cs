using System.Text.Json.Serialization;

namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 予約グラフアイテム
/// 時間帯枠ごとの予約統計を表す
/// </summary>
public class AppointmentGraphItem
{
    /// <summary>開始時刻</summary>
    [JsonPropertyName("start")]
    public TimeOnly Start { get; set; }

    /// <summary>終了時刻</summary>
    [JsonPropertyName("end")]
    public TimeOnly End { get; set; }

    /// <summary>期間（End - Start）</summary>
    [JsonPropertyName("duration")]
    public TimeSpan Duration => this.End - this.Start;

    /// <summary>予約数</summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>最大予約数（キャパシティ）</summary>
    [JsonPropertyName("cap")]
    public int Cap { get; set; }

    /// <summary>利用可能数（Cap - Count）</summary>
    [JsonPropertyName("available")]
    public int Available => this.Cap - this.Count;

    /// <summary>
    /// 時間外の予約が含まれているかどうか
    /// 
    /// 注意: スロット定義（AppointmentSlot）は業務時間内（例：09:00-12:00、13:00-17:00）のみ存在しますが、
    /// 予約データ（Appointments）は時刻を自由に指定できるため、時間外（開始前・終了後・昼休み時間外）の予約も存在し得ます。
    /// 時間外の予約は、近いスロットに吸収される（集計される）が、このフラグが立つことで時間外の予約が含まれていることを示します。
    /// 
    /// 例：
    /// - 08:00の予約 → 09:00開始のスロットに吸収され、HasOutsideHours = true
    /// - 18:30の予約 → 17:00終了のスロットに吸収され、HasOutsideHours = true
    /// - 12:30の予約（昼休み時間外） → 12:00終了または13:00開始のスロットに吸収され、HasOutsideHours = true
    /// </summary>
    [JsonPropertyName("hasOutsideHours")]
    public bool HasOutsideHours { get; set; } = false;
}

