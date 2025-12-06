namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// セッションエンティティ
/// </summary>
[Table("sessions")]
public class Session
{
    /// <summary>セッションID (PK)</summary>
    [Key]
    [Column("session_id")]
    public Guid SessionId { get; set; }

    /// <summary>ユーザーID (FK)</summary>
    [Column("user_id")]
    [ForeignKey("User")]
    public Guid UserId { get; set; }

    /// <summary>ユーザー表示名</summary>
    [Column("user_display_name")]
    [MaxLength(100)]
    public string UserDisplayName { get; set; } = String.Empty;

    /// <summary>クライアントアプリ名（バージョン含む）</summary>
    [Column("client_app_name")]
    [MaxLength(100)]
    public string ClientAppName { get; set; } = String.Empty;

    /// <summary>クライアントエンドポイント（IP, Port含む）</summary>
    [Column("client_endpoint")]
    [MaxLength(100)]
    public string ClientEndpoint { get; set; } = String.Empty;

    /// <summary>ログイン日時</summary>
    [Column("login_at")]
    public DateTimeOffset LoginAt { get; set; }

    /// <summary>ログアウト日時</summary>
    [Column("logout_at")]
    public DateTimeOffset? LogoutAt { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
}


