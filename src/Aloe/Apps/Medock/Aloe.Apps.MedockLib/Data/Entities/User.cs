namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// ユーザーエンティティ
/// </summary>
public class User : IAuditableEntity
{
    /// <summary>ユーザーID (PK)</summary>
    public Guid UserId { get; set; }

    /// <summary>ログイン用ID</summary>
    public string UserCode { get; set; } = string.Empty;

    /// <summary>メールアドレス</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>パスワードハッシュ</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>パスワードソルト</summary>
    public string PasswordSalt { get; set; } = string.Empty;

    /// <summary>有効期限</summary>
    public DateTimeOffset ExpiresDate { get; set; } = new DateTimeOffset(9999, 12, 31, 0, 0, 0, TimeSpan.Zero);

    /// <summary>ログイン成功回数</summary>
    public int LoginSuccessCount { get; set; }

    /// <summary>ログイン失敗回数</summary>
    public int LoginFailureCount { get; set; }

    /// <summary>ログイン失敗試行回数（ログイン成功でリセット）</summary>
    public int LoginFailureAttempts { get; set; }

    /// <summary>ロック解除日時</summary>
    public DateTimeOffset LockedUntilAt { get; set; } = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>最終ログイン日時</summary>
    public DateTimeOffset LastLoginAt { get; set; }

    /// <summary>最終ログアウト日時</summary>
    public DateTimeOffset LastLogoutAt { get; set; }

    /// <summary>システム管理者フラグ</summary>
    public bool IsSystemAdmin { get; set; }

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
    public virtual ICollection<TenantUser> TenantUsers { get; set; } = new List<TenantUser>();
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}


