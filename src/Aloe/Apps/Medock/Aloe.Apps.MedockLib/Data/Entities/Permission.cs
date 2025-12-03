namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// パーミッションエンティティ
/// </summary>
public class Permission : IAuditableEntity
{
    /// <summary>パーミッションコード (PK)</summary>
    public string PermissionCode { get; set; } = string.Empty;

    /// <summary>リソースコード (FK)</summary>
    public string ResourceCode { get; set; } = string.Empty;

    /// <summary>操作コード (FK)</summary>
    public string OperationCode { get; set; } = string.Empty;

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
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}


