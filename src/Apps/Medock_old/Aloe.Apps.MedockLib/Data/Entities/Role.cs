namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// ロールエンティティ
/// </summary>
[Table("roles")]
public class Role : IAuditableEntity
{
    /// <summary>ロールコード (PK)</summary>
    [Key]
    [Column("role_code")]
    [MaxLength(10)]
    public string RoleCode { get; set; } = String.Empty;

    /// <summary>ロール名</summary>
    [Column("role_name")]
    [MaxLength(100)]
    public string RoleName { get; set; } = String.Empty;

    /// <summary>ロール説明</summary>
    [Column("role_desc")]
    [MaxLength(1000)]
    public string RoleDesc { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    [Column("role_seq")]
    public int RoleSeq { get; set; }

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
    [InverseProperty("Role")]
    public virtual ICollection<FacilityUserRole> UserRoles { get; set; } = new List<FacilityUserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}


