namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 予約スロット定義エンティティ
/// フロアごとの予約可能時間帯をJSONBで保持
/// </summary>
[Table("appointment_slots")]
public class AppointmentSlot : IAuditableEntity
{
    /// <summary>予約スロットID (PK)</summary>
    [Key]
    [Column("appt_slot_id")]
    public Guid ApptSlotId { get; set; }

    /// <summary>フロアID (FK)</summary>
    [Column("floor_id")]
    [ForeignKey("Floor")]
    public Guid FloorId { get; set; }

    /// <summary>
    /// 予約スロット定義（JSONB）
    /// 例: { "slots": [{ "time": "08:00", "max": 5, "duration": 60 }, ...] }
    /// </summary>
    [Column("appt_slots")]
    public string ApptSlots { get; set; } = "{}";

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
    public virtual Floor Floor { get; set; } = null!;
}

