namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// ユーザーロール（ユーザーとロールの関連）エンティティ
/// </summary>
public class UserRole : IAuditableEntity
{
    /// <summary>ユーザーロールID (PK)</summary>
    public Guid UserRoleId { get; set; }

    /// <summary>ユーザーID (FK)</summary>
    public Guid UserId { get; set; }

    /// <summary>ロールコード (FK)</summary>
    public string RoleCode { get; set; } = string.Empty;

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
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}

