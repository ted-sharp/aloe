namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// ユーザーエンティティ
/// </summary>
[Table("users")]
public class User : IAuditableEntity
{
    /// <summary>ユーザーID (PK)</summary>
    [Key]
    [Column("user_id")]
    public Guid UserId { get; set; }

    /// <summary>ログイン用ID</summary>
    [Column("user_code")]
    [MaxLength(100)]
    public string UserCode { get; set; } = String.Empty;

    /// <summary>メールアドレス</summary>
    [Column("email")]
    [MaxLength(254)]
    public string Email { get; set; } = String.Empty;

    /// <summary>表示名</summary>
    [Column("user_display_name")]
    [MaxLength(100)]
    public string UserDisplayName { get; set; } = String.Empty;

    /// <summary>パスワードハッシュ</summary>
    [Column("password_hash")]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = String.Empty;

    /// <summary>パスワードソルト</summary>
    [Column("password_salt")]
    [MaxLength(64)]
    public string PasswordSalt { get; set; } = String.Empty;

    /// <summary>有効期限</summary>
    [Column("expires_on")]
    public DateTime ExpiresOn { get; set; } = new DateTime(9999, 12, 31);

    /// <summary>ログイン成功回数</summary>
    [Column("login_success_count")]
    public int LoginSuccessCount { get; set; }

    /// <summary>ログイン失敗回数</summary>
    [Column("login_failure_count")]
    public int LoginFailureCount { get; set; }

    /// <summary>ログイン失敗試行回数（ログイン成功でリセット）</summary>
    [Column("login_failure_attempts")]
    public int LoginFailureAttempts { get; set; }

    /// <summary>ロック解除日時</summary>
    [Column("locked_until_at")]
    public DateTime LockedUntilAt { get; set; } = new DateTime(1970, 1, 1, 0, 0, 0);

    /// <summary>最終ログイン日時</summary>
    [Column("last_login_at")]
    public DateTime LastLoginAt { get; set; }

    /// <summary>最終ログアウト日時</summary>
    [Column("last_logout_at")]
    public DateTime LastLogoutAt { get; set; }

    /// <summary>システム管理者フラグ</summary>
    [Column("is_system_admin")]
    public bool IsSystemAdmin { get; set; }

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
    public virtual ICollection<FacilityUserRole> UserRoles { get; set; } = new List<FacilityUserRole>();
    public virtual ICollection<FacilityUser> FacilityUsers { get; set; } = new List<FacilityUser>();
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}


