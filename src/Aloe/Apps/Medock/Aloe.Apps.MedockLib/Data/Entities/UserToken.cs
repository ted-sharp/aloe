namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// ユーザートークンエンティティ
/// </summary>
[Table("user_tokens")]
public class UserToken
{
    /// <summary>トークンID (PK)</summary>
    [Key]
    [Column("token_id")]
    public Guid TokenId { get; set; }

    /// <summary>ユーザーID (FK)</summary>
    [Column("user_id")]
    [ForeignKey("User")]
    public Guid UserId { get; set; }

    /// <summary>トークンプロバイダー (0: None, 1: App, 2: Totp, 3: WebAuthn, 4: Email, 5: Sms, 99: Others)</summary>
    [Column("token_provider")]
    public int TokenProvider { get; set; }

    /// <summary>トークン名</summary>
    [Column("token_name")]
    [MaxLength(64)]
    public string TokenName { get; set; } = String.Empty;

    /// <summary>トークン値</summary>
    [Column("token_value")]
    public string TokenValue { get; set; } = String.Empty;

    // Navigation Properties
    public virtual User User { get; set; } = null!;
}

