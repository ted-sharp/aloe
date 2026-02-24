namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 予約スケジュールスロットエンティティ
/// 曜日ごとの繰り返しスロット定義
/// days_of_week: 0=Sunday, 1=Monday, ..., 6=Saturday
/// </summary>
[Table("appointment_schedule_slots")]
public class AppointmentScheduleSlot : IAuditableEntity
{
    [Key]
    [Column("appt_schedule_slot_id")]
    public Guid ApptScheduleSlotId { get; set; }

    [Column("appt_schedule_id")]
    [ForeignKey("AppointmentSchedule")]
    public Guid ApptScheduleId { get; set; }

    /// <summary>
    /// 曜日配列 (0=Sunday, 1=Monday, ..., 6=Saturday)
    /// 例: [1,3,5] = 月・水・金
    /// </summary>
    [Column("days_of_week")]
    public int[] DaysOfWeek { get; set; } = Array.Empty<int>();

    /// <summary>
    /// スロット開始時刻（分単位、0:00からの分数）
    /// スロットは [SlotStartMin, SlotEndMin) の半開区間で定義（開始は包含、終了は排他）
    /// </summary>
    [Column("slot_start_min")]
    public int SlotStartMin { get; set; } = 540; // 9:00 = 540 minutes

    /// <summary>
    /// スロット終了時刻（分単位、0:00からの分数）
    /// スロットは [SlotStartMin, SlotEndMin) の半開区間で定義（開始は包含、終了は排他）
    /// </summary>
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
    public virtual AppointmentSchedule AppointmentSchedule { get; set; } = null!;
    public virtual ICollection<AppointmentScheduleSlotCapOverride> CapOverrides { get; set; } = new List<AppointmentScheduleSlotCapOverride>();
}
