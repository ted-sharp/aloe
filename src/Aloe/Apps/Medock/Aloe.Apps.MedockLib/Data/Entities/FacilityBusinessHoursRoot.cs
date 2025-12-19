using System.Text.Json.Serialization;

namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 施設営業時間のJSONB構造
/// JSONB で保持される営業時間定義のルートクラス
/// </summary>
public class FacilityBusinessHoursRoot
{
    /// <summary>始業時間（例: "09:00"）</summary>
    [JsonPropertyName("start")]
    public string Start { get; set; } = "09:00";

    /// <summary>就業時間（例: "18:00"）</summary>
    [JsonPropertyName("end")]
    public string End { get; set; } = "18:00";

    /// <summary>昼休憩時間</summary>
    [JsonPropertyName("lunch")]
    public LunchHoursItem? Lunch { get; set; }
}

/// <summary>
/// 昼休憩時間のJSONB構造
/// </summary>
public class LunchHoursItem
{
    /// <summary>昼休憩開始時間（例: "12:00"）</summary>
    [JsonPropertyName("start")]
    public string Start { get; set; } = "12:00";

    /// <summary>昼休憩終了時間（例: "13:00"）</summary>
    [JsonPropertyName("end")]
    public string End { get; set; } = "13:00";
}

