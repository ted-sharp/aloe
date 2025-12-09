namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 団体エンティティ
/// </summary>
[Table("organizations")]
public class Organization : IAuditableEntity
{
    [Key][Column("org_id")] public Guid OrgId { get; set; }
    [Column("facility_id")][ForeignKey("Facility")] public Guid FacilityId { get; set; }
    [Column("parent_org_id")] public Guid? ParentOrgId { get; set; }
    [Column("org_code")][MaxLength(100)] public string OrgCode { get; set; } = String.Empty;
    [Column("org_name")][MaxLength(200)] public string OrgName { get; set; } = String.Empty;
    [Column("org_name_katakana")][MaxLength(200)] public string OrgNameKatakana { get; set; } = String.Empty;
    [Column("org_name_katakana_compat")][MaxLength(200)] public string OrgNameKatakanaCompat { get; set; } = String.Empty;
    [Column("org_name_display")][MaxLength(200)] public string OrgNameDisplay { get; set; } = String.Empty;
    [Column("org_name_print")][MaxLength(200)] public string OrgNamePrint { get; set; } = String.Empty;
    [Column("org_memo")][MaxLength(500)] public string OrgMemo { get; set; } = String.Empty;
    [Column("is_deleted")] public bool IsDeleted { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; }
    [Column("created_user_id")] public Guid CreatedUserId { get; set; }
    [Column("created_session_id")] public Guid CreatedSessionId { get; set; }
    [Column("updated_at")] public DateTime UpdatedAt { get; set; }
    [Column("updated_user_id")] public Guid UpdatedUserId { get; set; }
    [Column("updated_session_id")] public Guid UpdatedSessionId { get; set; }
    public virtual Facility Facility { get; set; } = null!;
    public virtual Organization? ParentOrganization { get; set; }
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<EquipmentAppointment> EquipmentAppointments { get; set; } = new List<EquipmentAppointment>();
}


