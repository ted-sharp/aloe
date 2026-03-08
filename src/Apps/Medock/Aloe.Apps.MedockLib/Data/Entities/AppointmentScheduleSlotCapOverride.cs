namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 予約スケジュールスロット容量オーバーライドエンティティ
/// 特定日の特定スロットの容量のみをオーバーライド
/// </summary>
[Table("appointment_schedule_slot_cap_overrides")]
public class AppointmentScheduleSlotCapOverride : IAuditableEntity
{
    [Key]
    [Column("appt_schedule_slot_cap_override_id")]
    public Guid ApptScheduleSlotCapOverrideId { get; set; }

    [Column("appt_schedule_slot_id")]
    [ForeignKey("AppointmentScheduleSlot")]
    public Guid ApptScheduleSlotId { get; set; }

    [Column("appt_date")]
    public DateOnly ApptDate { get; set; } = DateOnly.MinValue;

    [Column("slot_cap")]
    public int SlotCap { get; set; } = 0;

    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    // IAuditableEntity properties
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
    public virtual AppointmentScheduleSlot AppointmentScheduleSlot { get; set; } = null!;
}
