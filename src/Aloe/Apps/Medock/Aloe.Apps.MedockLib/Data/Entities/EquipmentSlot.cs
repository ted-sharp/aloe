namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 設備スロット定義エンティティ
/// 設備ごとの予約可能時間帯をJSONBで保持
/// </summary>
[Table("equipment_slots")]
public class EquipmentSlot : IAuditableEntity
{
    /// <summary>設備スロットID (PK)</summary>
    [Key]
    [Column("equip_slot_id")]
    public Guid EquipSlotId { get; set; }

    /// <summary>設備ID (FK)</summary>
    [Column("equip_id")]
    [ForeignKey("Equipment")]
    public Guid EquipId { get; set; }

    /// <summary>
    /// 設備スロット定義（JSONB）
    /// 例: { "slots": [{ "time": "08:00", "max": 1, "duration": 30 }, ...] }
    /// </summary>
    [Column("equip_slots")]
    public string EquipSlots { get; set; } = "{}";

    /// <summary>有効フラグ</summary>
    [Column("is_active")]
    public bool IsActive { get; set; }

    /// <summary>有効開始日</summary>
    [Column("active_from")]
    public DateOnly ActiveFrom { get; set; }

    /// <summary>有効終了日</summary>
    [Column("active_to")]
    public DateOnly ActiveTo { get; set; } = new DateOnly(9999, 12, 31);

    /// <summary>削除フラグ</summary>
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    // IAuditableEntity
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    [Column("created_user_id")]
    public Guid CreatedUserId { get; set; }
    [Column("created_session_id")]
    public Guid CreatedSessionId { get; set; }
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
    [Column("updated_user_id")]
    public Guid UpdatedUserId { get; set; }
    [Column("updated_session_id")]
    public Guid UpdatedSessionId { get; set; }

    // Navigation Properties
    public virtual Equipment Equipment { get; set; } = null!;
}
