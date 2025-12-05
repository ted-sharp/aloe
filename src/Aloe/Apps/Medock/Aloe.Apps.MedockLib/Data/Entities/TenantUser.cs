namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// テナントユーザー（テナントとユーザーの関連）エンティティ
/// </summary>
public class TenantUser : IAuditableEntity
{
    /// <summary>テナントユーザーID (PK)</summary>
    public Guid TenantUserId { get; set; }

    /// <summary>テナントID (FK)</summary>
    public Guid TenantId { get; set; }

    /// <summary>ユーザーID (FK)</summary>
    public Guid UserId { get; set; }

    /// <summary>表示名</summary>
    public string DisplayName { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    public int TenantUserSeq { get; set; }

    /// <summary>テナント管理者フラグ</summary>
    public bool IsTenantAdmin { get; set; }

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
    public virtual User User { get; set; } = null!;
}


