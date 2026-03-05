using System.Security.Claims;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockServer.ApplicationServices.Mobile;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockServer.Controllers;

/// <summary>
/// 認証APIコントローラー
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        this._authService = authService;
        this._logger = logger;
    }

    /// <summary>
    /// ログイン
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var ipAddress = this.HttpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty;
            var userAgent = this.Request.Headers.UserAgent.ToString();
            var appName = "MedockServer";

            var result = await this._authService.LoginAsync(
                request.UserCode,
                request.Password,
                appName,
                ipAddress,
                userAgent);

            if (!result.IsSuccess)
            {
                this._logger.LogWarning("Login failed for user code {UserCode}: {ErrorMessage}", request.UserCode, result.ErrorMessage);
                return this.Unauthorized(new { message = result.ErrorMessage });
            }

            var isMobile = MobileDetectionHelper.IsMobile(userAgent);
            var (claimsIdentity, claimsPrincipal) = this.CreateClaimsFromAuthResult(result, isMobile);

            // KeepSessionフラグに基づいて認証プロパティを設定
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = request.KeepSession,
                ExpiresUtc = request.KeepSession
                    ? result.RefreshTokenExpiration?.ToUniversalTime() ?? DateTimeOffset.UtcNow.AddDays(30)
                    : null // KeepSessionがfalseの場合はセッションクッキー（ブラウザを閉じると無効）
            };

            await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

            this._logger.LogInformation("User logged in successfully: UserId={UserId}, UserCode={UserCode}", result.UserId, result.UserCode);

            return this.Ok(new LoginResponse
            {
                SessionId = result.SessionId!.Value,
                UserId = result.UserId!.Value,
                UserCode = result.UserCode!,
                Email = result.Email!,
                DisplayName = result.DisplayName ?? "",
                TenantId = result.TenantId,
                TenantName = result.TenantName ?? "",
                FacilityId = result.FacilityId,
                FacilityName = result.FacilityName ?? "",
                IsSystemAdmin = result.IsSystemAdmin,
                Roles = result.Roles!,
                IsMobile = isMobile
            });
        }
        catch (ArgumentException ex)
        {
            this._logger.LogWarning(ex, "Invalid argument in Login: {Message}", ex.Message);
            return this.BadRequest(new { message = "Invalid credentials provided" });
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error during login");
            return this.StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// トークン更新
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        try
        {
            var result = await this._authService.RefreshTokenAsync(request.UserId, request.FacilityId);

            if (!result.IsSuccess)
            {
                this._logger.LogWarning("Token refresh failed for user {UserId}: {ErrorMessage}", request.UserId, result.ErrorMessage);
                return this.Unauthorized(new { message = result.ErrorMessage });
            }

            var (claimsIdentity, claimsPrincipal) = this.CreateClaimsFromAuthResult(result);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = result.RefreshTokenExpiration?.ToUniversalTime()
            };

            await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

            this._logger.LogInformation("Token refreshed successfully for user {UserId}", request.UserId);

            return this.Ok(new RefreshResponse
            {
                RefreshTokenExpiration = result.RefreshTokenExpiration!.Value
            });
        }
        catch (ArgumentException ex)
        {
            this._logger.LogWarning(ex, "Invalid argument in Refresh: {Message}", ex.Message);
            return this.BadRequest(new { message = "Invalid user ID provided" });
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error during token refresh");
            return this.StatusCode(500, new { message = "An error occurred during token refresh" });
        }
    }

    /// <summary>
    /// 施設切替
    /// </summary>
    [HttpPost("switch-facility")]
    [Authorize]
    public async Task<IActionResult> SwitchFacility([FromBody] SwitchFacilityRequest request)
    {
        try
        {
            var userIdClaim = this.User.FindFirst("sub")?.Value;
            if (String.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                this._logger.LogWarning("Switch facility failed: invalid user in claims");
                return this.Unauthorized(new { message = "Invalid user" });
            }

            var result = await this._authService.SwitchFacilityAsync(userId, request.FacilityId);

            if (!result.IsSuccess)
            {
                this._logger.LogWarning("Facility switch failed for user {UserId} to facility {FacilityId}: {ErrorMessage}",
                    userId, request.FacilityId, result.ErrorMessage);
                return this.BadRequest(new { message = result.ErrorMessage });
            }

            // 既存のクレームを取得して更新
            var existingClaims = this.User.Claims.ToList();
            var claims = new List<Claim>();

            // 既存のクレームをコピー（facility_idとfacility_name以外）
            foreach (var claim in existingClaims)
            {
                if (claim.Type != "facility_id" && claim.Type != "facility_name" &&
                    claim.Type != "tenant_id" && claim.Type != "tenant_name")
                {
                    claims.Add(claim);
                }
            }

            // 施設情報を更新
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

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = result.RefreshTokenExpiration?.ToUniversalTime()
            };

            await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

            this._logger.LogInformation("Facility switched successfully for user {UserId} to facility {FacilityId}",
                userId, request.FacilityId);

            return this.Ok(new SwitchFacilityResponse
            {
                RefreshTokenExpiration = result.RefreshTokenExpiration!.Value,
                FacilityId = result.FacilityId!.Value,
                FacilityName = result.FacilityName!,
                TenantId = result.TenantId!.Value,
                TenantName = result.TenantName!
            });
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error during facility switch");
            return this.StatusCode(500, new { message = "An error occurred during facility switch" });
        }
    }

    /// <summary>
    /// アクセス可能な施設一覧を取得
    /// </summary>
    [HttpGet("accessible-facilities")]
    [Authorize]
    public async Task<IActionResult> GetAccessibleFacilities()
    {
        try
        {
            var userIdClaim = this.User.FindFirst("sub")?.Value;
            if (String.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                this._logger.LogWarning("Get accessible facilities failed: invalid user in claims");
                return this.Unauthorized(new { message = "Invalid user" });
            }

            var facilities = await this._authService.GetAccessibleFacilitiesAsync(userId);

            this._logger.LogInformation("Retrieved {FacilityCount} accessible facilities for user {UserId}",
                facilities.Count, userId);

            return this.Ok(new AccessibleFacilitiesResponse
            {
                Facilities = facilities.Select(f => new FacilityDto
                {
                    FacilityId = f.FacilityId,
                    FacilityName = f.FacilityName,
                    TenantId = f.TenantId,
                    TenantName = f.TenantName,
                    IsFacilityAdmin = f.IsFacilityAdmin
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving accessible facilities");
            return this.StatusCode(500, new { message = "An error occurred while retrieving accessible facilities" });
        }
    }

    /// <summary>
    /// ログアウト
    /// </summary>
    [HttpPost("logout")]
    [HttpGet("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request = null)
    {
        try
        {
            // セッションIDをクレームから取得（リクエストボディがなくてもクレームから取得可能）
            var sessionIdClaim = this.User.FindFirst("session_id")?.Value;
            if (!String.IsNullOrEmpty(sessionIdClaim) && Guid.TryParse(sessionIdClaim, out var sessionId))
            {
                await this._authService.LogoutAsync(sessionId);
                this._logger.LogInformation("User logged out successfully: SessionId={SessionId}", sessionId);
            }
            else if (request != null)
            {
                // リクエストボディからセッションIDを取得（フォールバック）
                await this._authService.LogoutAsync(request.SessionId);
                this._logger.LogInformation("User logged out successfully: SessionId={SessionId}", request.SessionId);
            }
            else
            {
                this._logger.LogWarning("Logout attempted without session ID");
            }

            // クッキーを削除
            await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // GETリクエストの場合はログインページにリダイレクト
            if (this.HttpContext.Request.Method == "GET")
            {
                return this.Redirect("/login");
            }

            return this.Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error during logout");
            return this.StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// AuthResultからクレームを作成します
    /// </summary>
    private (ClaimsIdentity ClaimsIdentity, ClaimsPrincipal ClaimsPrincipal) CreateClaimsFromAuthResult(AuthResult result, bool isMobile = false)
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

// Request DTOs
public record LoginRequest(string UserCode, string Password, bool KeepSession = false);
public record RefreshRequest(Guid UserId, Guid? FacilityId = null);
public record SwitchFacilityRequest(Guid FacilityId);
public record LogoutRequest(Guid SessionId);

// Response DTOs
public record LoginResponse
{
    public required Guid SessionId { get; init; }
    public required Guid UserId { get; init; }
    public required string UserCode { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public Guid? TenantId { get; init; }
    public required string TenantName { get; init; }
    public Guid? FacilityId { get; init; }
    public required string FacilityName { get; init; }
    public required bool IsSystemAdmin { get; init; }
    public required string[] Roles { get; init; }
    public bool IsMobile { get; init; }
}

public record RefreshResponse
{
    public required DateTime RefreshTokenExpiration { get; init; }
}

public record SwitchFacilityResponse
{
    public required DateTime RefreshTokenExpiration { get; init; }
    public required Guid FacilityId { get; init; }
    public required string FacilityName { get; init; }
    public required Guid TenantId { get; init; }
    public required string TenantName { get; init; }
}

public record AccessibleFacilitiesResponse
{
    public required List<FacilityDto> Facilities { get; init; }
}

public record FacilityDto
{
    public required Guid FacilityId { get; init; }
    public required string FacilityName { get; init; }
    public required Guid TenantId { get; init; }
    public required string TenantName { get; init; }
    public required bool IsFacilityAdmin { get; init; }
}
