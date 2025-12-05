namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// ロールエンティティ
/// </summary>
public class Role : IAuditableEntity
{
    /// <summary>ロールコード (PK)</summary>
    public string RoleCode { get; set; } = String.Empty;

    /// <summary>ロール名</summary>
    public string RoleName { get; set; } = String.Empty;

    /// <summary>ロール説明</summary>
    public string RoleDesc { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    public int RoleSeq { get; set; }

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
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}


