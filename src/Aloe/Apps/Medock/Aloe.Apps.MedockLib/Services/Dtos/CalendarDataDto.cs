using System.Text.Json.Serialization;

namespace Aloe.Apps.MedockLib.Services.Dtos;

/// <summary>
/// カレンダー表示用データDTO（JavaScript用）
/// </summary>
public class CalendarDataDto
{
    public List<AppointmentDataDto> Appointments { get; set; } = new();
    public Dictionary<string, ResourceStatSlotsDto> MainStats { get; set; } = new();
    public Dictionary<string, Dictionary<string, ResourceStatSlotsDto>> EquipmentStats { get; set; } = new();
    public Dictionary<string, string> Holidays { get; set; } = new();
}

/// <summary>
/// 予約データDTO（JavaScript用）
/// </summary>
public class AppointmentDataDto
{
    public string Id { get; set; } = String.Empty;
    public string Date { get; set; } = String.Empty;
    public int StartMin { get; set; }
    public string? PatientName { get; set; }
    public Guid? PatientId { get; set; }
    public string? OrganizationName { get; set; }
    public Guid? OrganizationId { get; set; }
    public string? FloorName { get; set; }
    public Guid? FloorId { get; set; }
    public int Status { get; set; }
}

/// <summary>
/// リソース統計スロットデータDTO（JavaScript用）
/// MainリソースとEquipmentリソースの両方で使用する統一DTO
/// 棒グラフ・折れ線グラフ描画用に最適化された配列ベース構造
/// </summary>
public class ResourceStatSlotsDto
{
    /// <summary>
    /// リソースID（Equipment用、Mainの場合はnull）
    /// </summary>
    public string? ResourceId { get; set; }

    /// <summary>
    /// リソース名（Equipment用、Mainの場合はnull）
    /// </summary>
    public string? ResourceName { get; set; }

    /// <summary>
    /// 空き数（Available）の合計（Equipment用、Mainの場合は0）
    /// </summary>
    public int TotalAvailable { get; set; }

    /// <summary>
    /// キャパシティの合計（Equipment用、Mainの場合は0）
    /// </summary>
    public int TotalCapacity { get; set; }

    /// <summary>
    /// スロット開始時刻の配列（分単位、0:00からの分数）
    /// </summary>
    public int[] SlotStartMins { get; set; } = Array.Empty<int>();

    /// <summary>
    /// スロット終了時刻の配列（分単位、0:00からの分数）
    /// </summary>
    public int[] SlotEndMins { get; set; } = Array.Empty<int>();

    /// <summary>
    /// スロット使用数の配列（Main用、Equipmentの場合は空配列）
    /// </summary>
    public int[] SlotCounts { get; set; } = Array.Empty<int>();

    /// <summary>
    /// スロットキャパシティの配列（Main/Equipment共用、空き率計算に使用）
    /// </summary>
    public int[] SlotCaps { get; set; } = Array.Empty<int>();

    /// <summary>
    /// スロット空き数の配列（計算値: cap - count）
    /// </summary>
    public int[] SlotAvailables { get; set; } = Array.Empty<int>();

    /// <summary>
    /// スロットフラグ配列
    /// ビット0: IsGrayedOut, ビット1: IsOutsideHours, ビット2: FilteredCount > 0
    /// </summary>
    public byte[]? SlotFlags { get; set; }

    /// <summary>
    /// フィルタリング後のカウント配列（オプション）
    /// </summary>
    public int[]? SlotFilteredCounts { get; set; }

    /// <summary>
    /// 日付全体がグレーアウトされているか（Main用、Equipmentの場合はfalse）
    /// </summary>
    public bool IsDayGrayedOut { get; set; }
}

