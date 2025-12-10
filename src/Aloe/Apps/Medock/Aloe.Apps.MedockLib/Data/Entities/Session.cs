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
    public string UserDisplayName { get; set; } = String.Empty;

    /// <summary>発行日時</summary>
    [Column("issued_at")]
    public DateTime IssuedAt { get; set; }

    /// <summary>有効期限</summary>
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>無効化日時</summary>
    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }

    /// <summary>IPアドレス</summary>
    [Column("ip_address")]
    [MaxLength(256)]
    public string IpAddress { get; set; } = String.Empty;

    /// <summary>ユーザーエージェント</summary>
    [Column("user_agent")]
    [MaxLength(512)]
    public string UserAgent { get; set; } = String.Empty;

    /// <summary>アプリ名</summary>
    [Column("app_name")]
    [MaxLength(100)]
    public string AppName { get; set; } = String.Empty;

    // Navigation Properties
    public virtual User User { get; set; } = null!;
}


