namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 予約スケジュールエンティティ
/// リソースごとのマスタスケジュール定義
/// </summary>
[Table("appointment_schedules")]
public class AppointmentSchedule : IAuditableEntity
{
    [Key]
    [Column("appt_schedule_id")]
    public Guid ApptScheduleId { get; set; }

    [Column("appt_res_id")]
    [ForeignKey("AppointmentResource")]
    public Guid ApptResId { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = false;

    [Column("active_from")]
    public DateOnly ActiveFrom { get; set; } = DateOnly.MinValue;

    [Column("active_to")]
    public DateOnly ActiveTo { get; set; } = new DateOnly(9999, 12, 31);

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
    public virtual AppointmentResource AppointmentResource { get; set; } = null!;
    public virtual ICollection<AppointmentScheduleSlot> AppointmentScheduleSlots { get; set; } = new List<AppointmentScheduleSlot>();
    public virtual ICollection<AppointmentScheduleOverride> AppointmentScheduleOverrides { get; set; } = new List<AppointmentScheduleOverride>();
}
