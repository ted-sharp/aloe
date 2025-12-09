using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// JWT トークンの生成と検証を行うサービス
/// </summary>
/// <remarks>
/// OIDC (OpenID Connect) に準拠したクレーム構造を使用します。
/// - sub: ユーザーID
/// - preferred_username: ユーザーコード
/// - email: メールアドレス
/// - display_name: 表示名（カスタムクレーム）
/// - tenant_id: テナントID（カスタムクレーム）
/// - tenant_name: テナント名（カスタムクレーム）
/// - facility_id: 施設ID（カスタムクレーム）
/// - facility_name: 施設名（カスタムクレーム）
/// - is_system_admin: システム管理者フラグ（カスタムクレーム）
/// - roles: ロール（カスタムクレーム）
/// </remarks>
public class JwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        this._settings = settings.Value;
        this._signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._settings.SecretKey));
    }

    /// <summary>
    /// アクセストークンを生成します。
    /// </summary>
    /// <param name="tokenParams">トークン生成パラメータ</param>
    /// <returns>JWTアクセストークン</returns>
    public string GenerateAccessToken(TokenGenerationParams tokenParams)
    {
        List<Claim> claims =
        [
            // OIDC標準クレーム
            new("sub", tokenParams.UserId.ToString()),
            new("preferred_username", tokenParams.UserCode),
            new("email", tokenParams.Email),
            new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        // 表示名
        if (!String.IsNullOrEmpty(tokenParams.DisplayName))
        {
            claims.Add(new Claim("display_name", tokenParams.DisplayName));
        }

        // テナント情報（カスタムクレーム）
        if (tokenParams.TenantId.HasValue)
        {
            claims.Add(new Claim("tenant_id", tokenParams.TenantId.Value.ToString()));
        }
        if (!String.IsNullOrEmpty(tokenParams.TenantName))
        {
            claims.Add(new Claim("tenant_name", tokenParams.TenantName));
        }

        // 施設情報（カスタムクレーム）
        if (tokenParams.FacilityId.HasValue)
        {
            claims.Add(new Claim("facility_id", tokenParams.FacilityId.Value.ToString()));
        }
        if (!String.IsNullOrEmpty(tokenParams.FacilityName))
        {
            claims.Add(new Claim("facility_name", tokenParams.FacilityName));
        }

        // 管理者フラグ
        claims.Add(new Claim("is_system_admin", tokenParams.IsSystemAdmin.ToString().ToLower()));

        // ロール（カスタムクレーム）
        if (tokenParams.Roles != null)
        {
            foreach (var role in tokenParams.Roles)
            {
                claims.Add(new Claim("roles", role));
            }
        }

        // セッションID（カスタムクレーム）
        if (tokenParams.SessionId.HasValue)
        {
            claims.Add(new Claim("session_id", tokenParams.SessionId.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(this._settings.AccessTokenExpirationMinutes),
            Issuer = this._settings.Issuer,
            Audience = this._settings.Audience,
            SigningCredentials = new SigningCredentials(this._signingKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// リフレッシュトークンを生成します。
    /// </summary>
    /// <returns>Base64エンコードされたリフレッシュトークン</returns>
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// トークンを検証し、ClaimsPrincipalを返します。
    /// </summary>
    /// <param name="token">検証するJWTトークン</param>
    /// <returns>検証成功時はClaimsPrincipal、失敗時はnull</returns>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = this._settings.Issuer,
            ValidAudience = this._settings.Audience,
            IssuerSigningKey = this._signingKey,
            ClockSkew = TimeSpan.Zero // 有効期限の猶予なし
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// リフレッシュトークンの有効期限を取得します。
    /// </summary>
    public DateTime GetRefreshTokenExpiration()
    {
        return DateTime.UtcNow.AddDays(this._settings.RefreshTokenExpirationDays);
    }
}

/// <summary>
/// トークン生成パラメータ
/// </summary>
public class TokenGenerationParams
{
    public Guid UserId { get; init; }
    public string UserCode { get; init; } = "";
    public string Email { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public Guid? TenantId { get; init; }
    public string? TenantName { get; init; }
    public Guid? FacilityId { get; init; }
    public string? FacilityName { get; init; }
    public bool IsSystemAdmin { get; init; }
    public bool IsFacilityAdmin { get; init; }
    public IEnumerable<string>? Roles { get; init; }
    public Guid? SessionId { get; init; }
}



