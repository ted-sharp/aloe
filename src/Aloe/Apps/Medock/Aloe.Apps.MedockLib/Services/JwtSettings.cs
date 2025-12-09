namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// JWT認証の設定
/// </summary>
public class JwtSettings
{
    /// <summary>設定セクション名</summary>
    public const string SectionName = "Jwt";

    /// <summary>トークン署名用の秘密鍵（最低32文字以上推奨）</summary>
    public required string SecretKey { get; set; }

    /// <summary>トークン発行者</summary>
    public required string Issuer { get; set; }

    /// <summary>トークン対象者</summary>
    public required string Audience { get; set; }

    /// <summary>アクセストークンの有効期限（分）</summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>リフレッシュトークンの有効期限（日）</summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}



