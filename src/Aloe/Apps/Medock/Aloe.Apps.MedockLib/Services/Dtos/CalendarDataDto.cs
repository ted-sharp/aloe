using System.Text.Json.Serialization;

namespace Aloe.Apps.MedockLib.Services.Dtos;

/// <summary>
/// カレンダー表示用データDTO（JavaScript用）
/// </summary>
public class CalendarDataDto
{
    public List<AppointmentDataDto> Appointments { get; set; } = new();
    public Dictionary<string, MainStatsDataDto> MainStats { get; set; } = new();
    public Dictionary<string, EquipmentStatsDataDto> EquipmentStats { get; set; } = new();
    public Dictionary<string, string> Holidays { get; set; } = new();
}

/// <summary>
/// 予約データDTO（JavaScript用）
/// </summary>
public class AppointmentDataDto
{
    public string Id { get; set; } = String.Empty;
    public string Date { get; set; } = String.Empty;
    public string StartTime { get; set; } = String.Empty;
    public string EndTime { get; set; } = String.Empty;
    public string? PatientName { get; set; }
    public Guid? PatientId { get; set; }
    public string? OrganizationName { get; set; }
    public Guid? OrganizationId { get; set; }
    public string? FloorName { get; set; }
    public Guid? FloorId { get; set; }
    public int Status { get; set; }
}

/// <summary>
/// Mainリソース統計データDTO（JavaScript用）
/// </summary>
public class MainStatsDataDto
{
    public List<SlotDataDto> Slots { get; set; } = new();
    public bool IsGrayedOut { get; set; }
}

/// <summary>
/// Equipmentリソース統計データDTO（JavaScript用）
/// 日付ごとに複数のEquipmentリソースのデータを保持
/// </summary>
public class EquipmentStatsDataDto
{
    /// <summary>
    /// Equipmentリソース別の統計データ（リソースID -> リソース統計データ）
    /// </summary>
    public Dictionary<string, EquipmentResourceStatsDto> Resources { get; set; } = new();
}

/// <summary>
/// Equipment個別リソースの統計データDTO（JavaScript用）
/// 折れ線グラフ描画用に最適化された配列ベース構造
/// </summary>
public class EquipmentResourceStatsDto
{
    /// <summary>
    /// リソースID
    /// </summary>
    public string ResourceId { get; set; } = String.Empty;

    /// <summary>
    /// リソース名
    /// </summary>
    public string ResourceName { get; set; } = String.Empty;

    /// <summary>
    /// 空き数（Available）の合計
    /// </summary>
    public int TotalAvailable { get; set; }

    /// <summary>
    /// キャパシティの合計
    /// </summary>
    public int TotalCapacity { get; set; }

    /// <summary>
    /// スロット開始時刻（0時からの分数の配列）
    /// 例: [540, 570, 600, ...] = [09:00, 09:30, 10:00, ...]
    /// </summary>
    public int[] SlotStartMinutes { get; set; } = Array.Empty<int>();

    /// <summary>
    /// スロット終了時刻（0時からの分数の配列）
    /// 例: [570, 600, 630, ...] = [09:30, 10:00, 10:30, ...]
    /// </summary>
    public int[] SlotEndMinutes { get; set; } = Array.Empty<int>();

    /// <summary>
    /// スロット空き数の配列（折れ線グラフのY軸値）
    /// </summary>
    public int[] SlotAvailables { get; set; } = Array.Empty<int>();

    /// <summary>
    /// スロットフラグ配列（オプション）
    /// ビット0: IsGrayedOut, ビット1: IsOutsideHours
    /// 将来のフィルタ機能用。フラグが全て0の場合はnull
    /// </summary>
    public byte[]? SlotFlags { get; set; }
}

/// <summary>
/// スロットデータDTO（JavaScript用）
/// </summary>
public class SlotDataDto
{
    public string Start { get; set; } = String.Empty;
    public string End { get; set; } = String.Empty;
    public int Count { get; set; }
    public int Cap { get; set; }
    public int Available { get; set; }
    public bool IsGrayedOut { get; set; }
    public int FilteredCount { get; set; }

    /// <summary>
    /// 時間外スロットかどうか（時間外スロットはグラフには描画されず、赤い縦ラインで存在の有無のみ表示）
    /// 
    /// 注意: スロット定義（AppointmentSlot）は業務時間内（例：09:00-12:00、13:00-17:00）のみ存在しますが、
    /// 予約データ（Appointments）は時刻を自由に指定できるため、時間外（開始前・終了後・昼休み時間外）の予約も存在し得ます。
    /// 時間外スロットは、グラフには描画されず、赤い縦ラインで存在の有無のみを表示します。
    /// </summary>
    [JsonPropertyName("isOutsideHours")]
    public bool IsOutsideHours { get; set; } = false;
}

