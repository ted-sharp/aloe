namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// テナントエンティティ
/// </summary>
public class Tenant : IAuditableEntity
{
    /// <summary>テナントID (PK)</summary>
    public Guid TenantId { get; set; }

    /// <summary>テナント名</summary>
    public string TenantName { get; set; } = string.Empty;

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
    public virtual ICollection<TenantUser> TenantUsers { get; set; } = new List<TenantUser>();
    public virtual ICollection<Facility> Facilities { get; set; } = new List<Facility>();
    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
}


