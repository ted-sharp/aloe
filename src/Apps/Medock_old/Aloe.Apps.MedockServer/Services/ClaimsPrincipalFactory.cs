using System.Security.Claims;
using Aloe.Apps.MedockLib.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Aloe.Apps.MedockServer.Services;

/// <summary>
/// AuthResult から ClaimsPrincipal を作成する共通ファクトリ
/// </summary>
public static class ClaimsPrincipalFactory
{
    /// <summary>
    /// AuthResult と isMobile から ClaimsIdentity と ClaimsPrincipal を生成します。
    /// </summary>
    public static (ClaimsIdentity ClaimsIdentity, ClaimsPrincipal ClaimsPrincipal) Create(AuthResult result, bool isMobile = false)
    {
        if (result.UserId == null)
        {
            throw new InvalidOperationException("Cannot create claims from AuthResult with null UserId");
        }

        var claims = new List<Claim>
        {
            new Claim("sub", result.UserId.Value.ToString()),
            new Claim("user_code", result.UserCode ?? ""),
            new Claim("email", result.Email ?? ""),
            new Claim("preferred_username", result.UserCode ?? ""),
            new Claim(ClaimTypes.Name, result.UserCode ?? ""),
            new Claim(ClaimTypes.Email, result.Email ?? ""),
            new Claim("user_display_name", result.DisplayName ?? ""),
            new Claim("is_system_admin", result.IsSystemAdmin.ToString().ToLower()),
            new Claim("is_facility_admin", result.IsFacilityAdmin.ToString().ToLower()),
            new Claim("is_mobile", isMobile.ToString().ToLower())
        };

        if (result.SessionId.HasValue)
        {
            claims.Add(new Claim("session_id", result.SessionId.Value.ToString()));
        }

        if (result.TenantId.HasValue)
        {
            claims.Add(new Claim("tenant_id", result.TenantId.Value.ToString()));
            claims.Add(new Claim("tenant_name", result.TenantName ?? ""));
        }

        if (result.FacilityId.HasValue)
        {
            claims.Add(new Claim("facility_id", result.FacilityId.Value.ToString()));
            claims.Add(new Claim("facility_name", result.FacilityName ?? ""));
        }

        if (result.Roles != null)
        {
            foreach (var role in result.Roles)
            {
                claims.Add(new Claim("roles", role));
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        return (claimsIdentity, claimsPrincipal);
    }
}
