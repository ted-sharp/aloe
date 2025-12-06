namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// テナントエンティティ
/// </summary>
[Table("tenants")]
public class Tenant : IAuditableEntity
{
    /// <summary>テナントID (PK)</summary>
    [Key]
    [Column("tenant_id")]
    public Guid TenantId { get; set; }

    /// <summary>テナント名</summary>
    [Column("tenant_name")]
    [MaxLength(100)]
    public string TenantName { get; set; } = String.Empty;

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
    public DateTimeOffset CreatedAt { get; set; }
    [Column("created_user_id")]
    public Guid CreatedUserId { get; set; }
    [Column("created_session_id")]
    public Guid CreatedSessionId { get; set; }
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
    [Column("updated_user_id")]
    public Guid UpdatedUserId { get; set; }
    [Column("updated_session_id")]
    public Guid UpdatedSessionId { get; set; }

    // Navigation Properties
    public virtual ICollection<Facility> Facilities { get; set; } = new List<Facility>();
}


