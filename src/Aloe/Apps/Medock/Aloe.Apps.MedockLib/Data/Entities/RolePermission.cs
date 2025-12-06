namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// ロールパーミッション（ロールとパーミッションの関連）エンティティ
/// </summary>
[Table("role_permissions")]
public class RolePermission : IAuditableEntity
{
    /// <summary>ロールパーミッションコード (PK)</summary>
    [Key]
    [Column("role_permission_code")]
    [MaxLength(31)]
    public string RolePermissionCode { get; set; } = String.Empty;

    /// <summary>ロールコード (FK)</summary>
    [Column("role_code")]
    [MaxLength(10)]
    [ForeignKey("Role")]
    public string RoleCode { get; set; } = String.Empty;

    /// <summary>パーミッションコード (FK)</summary>
    [Column("permission_code")]
    [MaxLength(21)]
    [ForeignKey("Permission")]
    public string PermissionCode { get; set; } = String.Empty;

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
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}


