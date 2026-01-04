namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 予約スケジュールスロットオーバーライドエンティティ
/// オーバーライド日の置換スロット定義
/// </summary>
[Table("appointment_schedule_slot_overrides")]
public class AppointmentScheduleSlotOverride : IAuditableEntity
{
    [Key]
    [Column("appt_schedule_slot_override_id")]
    public Guid ApptScheduleSlotOverrideId { get; set; }

    [Column("appt_schedule_override_id")]
    [ForeignKey("AppointmentScheduleOverride")]
    public Guid ApptScheduleOverrideId { get; set; }

    /// <summary>スロット開始時刻（分単位、0:00からの分数）</summary>
    [Column("slot_start_min")]
    public int SlotStartMin { get; set; } = 540; // 9:00 = 540 minutes

    /// <summary>スロット終了時刻（分単位、0:00からの分数）</summary>
    [Column("slot_end_min")]
    public int SlotEndMin { get; set; } = 570; // 9:30 = 570 minutes

    /// <summary>スロット容量</summary>
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
    public virtual AppointmentScheduleOverride AppointmentScheduleOverride { get; set; } = null!;

    // Helper Properties
    [NotMapped]
    public TimeOnly SlotStartTime
    {
        get => TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(this.SlotStartMin));
        set => this.SlotStartMin = value.Hour * 60 + value.Minute;
    }

    [NotMapped]
    public TimeOnly SlotEndTime
    {
        get => TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(this.SlotEndMin));
        set => this.SlotEndMin = value.Hour * 60 + value.Minute;
    }

    [NotMapped]
    public int DurationMin => this.SlotEndMin - this.SlotStartMin;
}
