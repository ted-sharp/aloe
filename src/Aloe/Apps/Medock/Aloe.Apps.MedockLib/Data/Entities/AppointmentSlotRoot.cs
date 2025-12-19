using System.Text.Json.Serialization;

namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 予約スロット定義
/// JSONB で保持されるスロット定義のルートクラス
/// </summary>
public class AppointmentSlotRoot
{
    /// <summary>スロットアイテムのリスト</summary>
    [JsonPropertyName("slots")]
    public List<AppointmentSlotItem> Slots { get; set; } = new();
}

