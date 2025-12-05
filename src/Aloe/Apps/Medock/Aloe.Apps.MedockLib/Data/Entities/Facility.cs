namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 施設エンティティ
/// </summary>
public class Facility : IAuditableEntity
{
    /// <summary>施設ID (PK)</summary>
    public Guid FacilityId { get; set; }

    /// <summary>テナントID (FK)</summary>
    public Guid TenantId { get; set; }

    /// <summary>医療機関コード</summary>
    public string MedicalInstitutionCode { get; set; } = String.Empty;

    /// <summary>施設名</summary>
    public string FacilityName { get; set; } = String.Empty;

    /// <summary>施設表示名</summary>
    public string FacilityNameDisplay { get; set; } = String.Empty;

    /// <summary>有効フラグ</summary>
    public bool IsActive { get; set; }

    /// <summary>有効開始日</summary>
    public DateOnly ActiveFrom { get; set; }

    /// <summary>有効終了日</summary>
    public DateOnly? ActiveTo { get; set; }

    /// <summary>削除フラグ</summary>
    public bool IsDeleted { get; set; }

    // IAuditableEntity
    public DateTimeOffset CreatedAt { get; set; }
    public Guid CreatedUserId { get; set; }
    public Guid CreatedSessionId { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid UpdatedUserId { get; set; }
    public Guid UpdatedSessionId { get; set; }

    // Navigation Properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<Floor> Floors { get; set; } = new List<Floor>();
    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
}


