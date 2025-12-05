using Aloe.Apps.MedockLib.Data.Entities;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 設備データと統計情報を取得するサービス
/// </summary>
public interface IEquipmentService
{
    /// <summary>指定テナントの設備一覧を取得</summary>
    /// <param name="tenantId">テナントID</param>
    /// <returns>設備情報のリスト</returns>
    Task<List<EquipmentDto>> GetEquipmentsByTenantAsync(Guid tenantId);

    /// <summary>設備別予約統計を取得</summary>
    /// <param name="equipmentIds">設備IDのリスト</param>
    /// <param name="date">対象日付</param>
    /// <returns>設備別予約統計のリスト</returns>
    Task<List<EquipmentAppointmentStatsDto>> GetEquipmentStatsAsync(List<Guid> equipmentIds, DateOnly date);
}

/// <summary>
/// 設備情報のDTO
/// </summary>
public class EquipmentDto
{
    public Guid EquipId { get; set; }
    public string EquipName { get; set; } = string.Empty;
    public string EquipDesc { get; set; } = string.Empty;
    public int EquipSeq { get; set; }
}

/// <summary>
/// 設備別予約統計のDTO
/// </summary>
public class EquipmentAppointmentStatsDto
{
    public Guid EquipId { get; set; }
    public DateOnly ApptDate { get; set; }
    public int ApptCount { get; set; }
    public int ApptMax { get; set; }
    public EquipmentApptGraphData ApptGraph { get; set; } = new();
}

/// <summary>
/// appt_graphフィールドのデシリアライズ用クラス
/// </summary>
public class EquipmentApptGraphData
{
    public List<EquipmentTimeSlot> Slots { get; set; } = new();
}

/// <summary>
/// 時間スロット情報（午前/午後などの緩いスロット対応）
/// </summary>
public class EquipmentTimeSlot
{
    /// <summary>時間スロット名（"08:00", "09:00" または "AM", "PM"など）</summary>
    public string Time { get; set; } = string.Empty;

    /// <summary>予約数</summary>
    public int Count { get; set; }

    /// <summary>最大予約数</summary>
    public int Max { get; set; }

    /// <summary>緩いスロットフラグ（午前/午後など）</summary>
    public bool IsCoarseSlot { get; set; }
}
