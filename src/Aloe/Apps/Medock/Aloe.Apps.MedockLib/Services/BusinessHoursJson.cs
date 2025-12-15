using System.Text.Json.Serialization;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 営業時間のJSONB構造
/// </summary>
public class BusinessHoursJson
{
    /// <summary>始業時間（例: "09:00"）</summary>
    [JsonPropertyName("start")]
    public string Start { get; set; } = "09:00";

    /// <summary>就業時間（例: "18:00"）</summary>
    [JsonPropertyName("end")]
    public string End { get; set; } = "18:00";

    /// <summary>昼休憩時間</summary>
    [JsonPropertyName("lunch")]
    public LunchHoursJson? Lunch { get; set; }
}

/// <summary>
/// 昼休憩時間のJSONB構造
/// </summary>
public class LunchHoursJson
{
    /// <summary>昼休憩開始時間（例: "12:00"）</summary>
    [JsonPropertyName("start")]
    public string Start { get; set; } = "12:00";

    /// <summary>昼休憩終了時間（例: "13:00"）</summary>
    [JsonPropertyName("end")]
    public string End { get; set; } = "13:00";
}

