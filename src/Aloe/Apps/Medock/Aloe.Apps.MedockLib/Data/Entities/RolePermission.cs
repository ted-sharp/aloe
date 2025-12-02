namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// ロールパーミッション（ロールとパーミッションの関連）エンティティ
/// </summary>
public class RolePermission : IAuditableEntity
{
    /// <summary>ロールパーミッションコード (PK)</summary>
    public string RolePermissionCode { get; set; } = string.Empty;

    /// <summary>ロールコード (FK)</summary>
    public string RoleCode { get; set; } = string.Empty;

    /// <summary>パーミッションコード (FK)</summary>
    public string PermissionCode { get; set; } = string.Empty;

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
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}

