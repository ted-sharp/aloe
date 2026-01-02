namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("appointments")]
public class Appointment : IAuditableEntity
{
    [Key][Column("appt_id")] public Guid ApptId { get; set; }
    [Column("floor_id")][ForeignKey("Floor")] public Guid FloorId { get; set; }
    [Column("org_id")][ForeignKey("Organization")] public Guid? OrgId { get; set; }
    [Column("pt_id")][ForeignKey("Patient")] public Guid? PtId { get; set; }
    [Column("appt_date")] public DateOnly? ApptDate { get; set; }
    [Column("appt_start_time")] public TimeOnly? ApptStartTime { get; set; }
    [Column("appt_duration_min")] public int? ApptDurationMin { get; set; }
    [Column("appt_status_code")] public int ApptStatusCode { get; set; } = 0;
    [Column("appt_memo")][MaxLength(1000)] public string ApptMemo { get; set; } = String.Empty;
    [Column("is_deleted")] public bool IsDeleted { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; }
    [Column("created_user_id")] public Guid CreatedUserId { get; set; }
    [Column("created_session_id")] public Guid CreatedSessionId { get; set; }
    [Column("updated_at")] public DateTime UpdatedAt { get; set; }
    [Column("updated_user_id")] public Guid UpdatedUserId { get; set; }
    [Column("updated_session_id")] public Guid UpdatedSessionId { get; set; }
    public virtual Floor Floor { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
    public virtual Patient Patient { get; set; } = null!;
    public virtual ICollection<AppointmentResourceAssignment> AppointmentResourceReservations { get; set; } = new List<AppointmentResourceAssignment>();
}


