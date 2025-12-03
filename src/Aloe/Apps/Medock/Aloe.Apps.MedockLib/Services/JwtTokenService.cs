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
/// - tenant_id: テナントID（カスタムクレーム）
/// - roles: ロール（カスタムクレーム）
/// </remarks>
public class JwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
    }

    /// <summary>
    /// アクセストークンを生成します。
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="userCode">ユーザーコード（ログインID）</param>
    /// <param name="email">メールアドレス</param>
    /// <param name="tenantId">テナントID（オプション）</param>
    /// <param name="roles">ロール一覧（オプション）</param>
    /// <returns>JWTアクセストークン</returns>
    public string GenerateAccessToken(
        Guid userId,
        string userCode,
        string email,
        Guid? tenantId = null,
        IEnumerable<string>? roles = null)
    {
        var claims = new List<Claim>
        {
            // OIDC標準クレーム
            new("sub", userId.ToString()),
            new("preferred_username", userCode),
            new("email", email),
            new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // テナントID（カスタムクレーム）
        if (tenantId.HasValue)
        {
            claims.Add(new Claim("tenant_id", tenantId.Value.ToString()));
        }

        // ロール（カスタムクレーム）
        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim("roles", role));
            }
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature)
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
            ValidIssuer = _settings.Issuer,
            ValidAudience = _settings.Audience,
            IssuerSigningKey = _signingKey,
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
        return DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);
    }
}


