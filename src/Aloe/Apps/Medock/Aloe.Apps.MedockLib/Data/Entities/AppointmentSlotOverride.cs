namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 予約スロット上書きエンティティ
/// 特定日のスロット定義を上書きする
/// </summary>
[Table("appointment_slot_overrides")]
public class AppointmentSlotOverride : IAuditableEntity
{
    /// <summary>予約スロット上書きID (PK)</summary>
    [Key]
    [Column("appt_slot_override_id")]
    public Guid ApptSlotOverrideId { get; set; }

    /// <summary>予約日</summary>
    [Column("appt_date")]
    public DateOnly ApptDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>予約リソースID (FK)</summary>
    [Column("appt_res_id")]
    [ForeignKey("AppointmentResource")]
    public Guid ApptResId { get; set; }

    /// <summary>
    /// 予約スロット定義（JSONB）
    /// 例: { "slots": [{ "time": "08:00", "max": 5, "duration": 60 }, ...] }
    /// </summary>
    [Column("appt_slots")]
    public string ApptSlots { get; set; } = "{}";

    /// <summary>削除フラグ</summary>
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    // IAuditableEntity
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    [Column("created_user_id")]
    public Guid CreatedUserId { get; set; }
    [Column("created_session_id")]
    public Guid CreatedSessionId { get; set; }
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
    [Column("updated_user_id")]
    public Guid UpdatedUserId { get; set; }
    [Column("updated_session_id")]
    public Guid UpdatedSessionId { get; set; }

    // Navigation Properties
    public virtual AppointmentResource AppointmentResource { get; set; } = null!;
}

