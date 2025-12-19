using System.Text.Json.Serialization;

namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 予約スロットアイテム
/// 個別のスロット定義を表す
/// </summary>
public class AppointmentSlotItem
{
    /// <summary>時間（例: "08:00", "AM", "PM"）</summary>
    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    /// <summary>最大予約数</summary>
    [JsonPropertyName("max")]
    public int Max { get; set; }

    /// <summary>継続時間（分）</summary>
    [JsonPropertyName("duration")]
    public int Duration { get; set; }
}

