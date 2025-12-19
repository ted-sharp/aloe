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
}

