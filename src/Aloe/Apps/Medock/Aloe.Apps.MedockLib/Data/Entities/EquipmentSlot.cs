namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 設備スロット定義エンティティ
/// 設備ごとの予約可能時間帯をJSONBで保持
/// </summary>
public class EquipmentSlot : IAuditableEntity
{
    /// <summary>設備スロットID (PK)</summary>
    public Guid EquipSlotId { get; set; }

    /// <summary>設備ID (FK)</summary>
    public Guid EquipId { get; set; }

    /// <summary>
    /// 設備スロット定義（JSONB）
    /// 例: { "slots": [{ "time": "08:00", "max": 1, "duration": 30 }, ...] }
    /// </summary>
    public string EquipSlots { get; set; } = "{}";

    /// <summary>有効フラグ</summary>
    public bool IsActive { get; set; }

    /// <summary>有効開始日</summary>
    public DateOnly ActiveFrom { get; set; }

    /// <summary>有効終了日</summary>
    public DateOnly ActiveTo { get; set; } = new DateOnly(9999, 12, 31);

    /// <summary>削除フラグ</summary>
    public bool IsDeleted { get; set; }

    // IAuditableEntity
    public DateTimeOffset CreatedAt { get; set; }
    public Guid CreatedUserId { get; set; }
    public Guid CreatedSessionId { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid UpdatedUserId { get; set; }
    public Guid UpdatedSessionId { get; set; }

    // Navigation Properties
    public virtual Equipment Equipment { get; set; } = null!;
}

