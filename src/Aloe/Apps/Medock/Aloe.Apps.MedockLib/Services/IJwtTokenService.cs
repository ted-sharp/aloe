using System.Security.Claims;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// JWT トークンサービスインターフェース
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// アクセストークンを生成します。
    /// </summary>
    string GenerateAccessToken(TokenGenerationParams tokenParams);

    /// <summary>
    /// リフレッシュトークンを生成します。
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// トークンを検証し、ClaimsPrincipalを返します。
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// リフレッシュトークンの有効期限を取得します。
    /// </summary>
    DateTime GetRefreshTokenExpiration();
}

