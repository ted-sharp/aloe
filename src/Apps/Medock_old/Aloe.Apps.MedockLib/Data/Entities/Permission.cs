namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// パーミッションエンティティ
/// </summary>
[Table("permissions")]
public class Permission : IAuditableEntity
{
    /// <summary>パーミッションコード (PK)</summary>
    [Key]
    [Column("permission_code")]
    [MaxLength(21)]
    public string PermissionCode { get; set; } = String.Empty;

    /// <summary>機能コード (FK)</summary>
    [Column("feature_code")]
    [MaxLength(10)]
    [ForeignKey("Feature")]
    public string FeatureCode { get; set; } = String.Empty;

    /// <summary>操作コード (FK)</summary>
    [Column("operation_code")]
    [MaxLength(10)]
    [ForeignKey("Operation")]
    public string OperationCode { get; set; } = String.Empty;

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
    public virtual Feature Feature { get; set; } = null!;
    public virtual Operation Operation { get; set; } = null!;
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}


