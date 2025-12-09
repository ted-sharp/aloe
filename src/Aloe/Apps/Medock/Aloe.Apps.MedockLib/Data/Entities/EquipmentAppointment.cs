namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("equipment_appointments")]
public class EquipmentAppointment : IAuditableEntity
{
    [Key][Column("equip_appt_id")] public Guid EquipApptId { get; set; }
    [Column("equip_id")][ForeignKey("Equipment")] public Guid EquipId { get; set; }
    [Column("org_id")][ForeignKey("Organization")] public Guid OrgId { get; set; }
    [Column("pt_id")][ForeignKey("Patient")] public Guid PtId { get; set; }
    [Column("appt_date")] public DateOnly? ApptDate { get; set; }
    [Column("appt_start_at")] public DateTime? ApptStartAt { get; set; }
    [Column("appt_end_at")] public DateTime? ApptEndAt { get; set; }
    [Column("appt_status_code")] public int ApptStatusCode { get; set; }
    [Column("appt_memo")][MaxLength(500)] public string ApptMemo { get; set; } = String.Empty;
    [Column("is_deleted")] public bool IsDeleted { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; }
    [Column("created_user_id")] public Guid CreatedUserId { get; set; }
    [Column("created_session_id")] public Guid CreatedSessionId { get; set; }
    [Column("updated_at")] public DateTime UpdatedAt { get; set; }
    [Column("updated_user_id")] public Guid UpdatedUserId { get; set; }
    [Column("updated_session_id")] public Guid UpdatedSessionId { get; set; }
    public virtual Equipment Equipment { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
    public virtual Patient Patient { get; set; } = null!;
}
