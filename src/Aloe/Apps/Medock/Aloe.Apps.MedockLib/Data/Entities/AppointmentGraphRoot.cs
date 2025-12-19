using System.Text.Json.Serialization;

namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 予約グラフ定義
/// JSONB で保持されるグラフデータのルートクラス
/// </summary>
public class AppointmentGraphRoot
{
    /// <summary>グラフアイテムのリスト</summary>
    [JsonPropertyName("slots")]
    public List<AppointmentGraphItem> Slots { get; set; } = new();
}

