namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 予約スケジュールオーバーライドエンティティ
/// 特定日のスケジュールオーバーライドマーカー
/// </summary>
[Table("appointment_schedule_overrides")]
public class AppointmentScheduleOverride : IAuditableEntity
{
    [Key]
    [Column("appt_schedule_override_id")]
    public Guid ApptScheduleOverrideId { get; set; }

    [Column("appt_schedule_id")]
    [ForeignKey("AppointmentSchedule")]
    public Guid ApptScheduleId { get; set; }

    [Column("appt_date")]
    public DateOnly ApptDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

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
    public virtual AppointmentSchedule AppointmentSchedule { get; set; } = null!;
    public virtual ICollection<AppointmentScheduleSlotOverride> AppointmentScheduleSlotOverrides { get; set; } = new List<AppointmentScheduleSlotOverride>();
}
