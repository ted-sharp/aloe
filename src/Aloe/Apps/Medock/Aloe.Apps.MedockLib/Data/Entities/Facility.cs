namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 施設エンティティ
/// </summary>
[Table("facilities")]
public class Facility : IAuditableEntity
{
    /// <summary>施設ID (PK)</summary>
    [Key]
    [Column("facility_id")]
    public Guid FacilityId { get; set; }

    /// <summary>テナントID (FK)</summary>
    [Column("tenant_id")]
    [ForeignKey("Tenant")]
    public Guid TenantId { get; set; }

    /// <summary>医療機関コード</summary>
    [Column("medical_institution_code")]
    [MaxLength(100)]
    public string MedicalInstitutionCode { get; set; } = String.Empty;

    /// <summary>施設名</summary>
    [Column("facility_name")]
    [MaxLength(100)]
    public string FacilityName { get; set; } = String.Empty;

    /// <summary>施設表示名</summary>
    [Column("facility_name_display")]
    [MaxLength(100)]
    public string FacilityNameDisplay { get; set; } = String.Empty;

    /// <summary>有効フラグ</summary>
    [Column("is_active")]
    public bool IsActive { get; set; }

    /// <summary>有効開始日</summary>
    [Column("active_from")]
    public DateOnly ActiveFrom { get; set; }

    /// <summary>有効終了日</summary>
    [Column("active_to")]
    public DateOnly? ActiveTo { get; set; }

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
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<FacilityUser> FacilityUsers { get; set; } = new List<FacilityUser>();
    public virtual ICollection<Floor> Floors { get; set; } = new List<Floor>();
    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
    public virtual ICollection<FacilityBusinessHours> FacilityBusinessHours { get; set; } = new List<FacilityBusinessHours>();
}


