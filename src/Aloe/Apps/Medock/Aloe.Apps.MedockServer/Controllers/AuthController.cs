using Aloe.Apps.MedockLib.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aloe.Apps.MedockServer.Controllers;

/// <summary>
/// 認証APIコントローラー
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        this._authService = authService;
    }

    /// <summary>
    /// ログイン
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var clientEndpoint = this.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var clientAppName = this.Request.Headers.UserAgent.ToString();

        var result = await this._authService.LoginAsync(
            request.UserCode,
            request.Password,
            clientAppName,
            clientEndpoint);

        if (!result.IsSuccess)
        {
            return this.Unauthorized(new { message = result.ErrorMessage });
        }

        return this.Ok(new LoginResponse
        {
            AccessToken = result.AccessToken!,
            RefreshToken = result.RefreshToken!,
            RefreshTokenExpiration = result.RefreshTokenExpiration!.Value,
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
            Roles = result.Roles!
        });
    }

    /// <summary>
    /// トークン更新
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await this._authService.RefreshTokenAsync(request.UserId, request.RefreshToken, request.FacilityId);

        if (!result.IsSuccess)
        {
            return this.Unauthorized(new { message = result.ErrorMessage });
        }

        return this.Ok(new RefreshResponse
        {
            AccessToken = result.AccessToken!,
            RefreshToken = result.RefreshToken!,
            RefreshTokenExpiration = result.RefreshTokenExpiration!.Value
        });
    }

    /// <summary>
    /// 施設切替
    /// </summary>
    [HttpPost("switch-facility")]
    [Authorize]
    public async Task<IActionResult> SwitchFacility([FromBody] SwitchFacilityRequest request)
    {
        var userIdClaim = this.User.FindFirst("sub")?.Value;
        if (String.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return this.Unauthorized(new { message = "Invalid user" });
        }

        var result = await this._authService.SwitchFacilityAsync(userId, request.FacilityId);

        if (!result.IsSuccess)
        {
            return this.BadRequest(new { message = result.ErrorMessage });
        }

        return this.Ok(new SwitchFacilityResponse
        {
            AccessToken = result.AccessToken!,
            RefreshToken = result.RefreshToken!,
            RefreshTokenExpiration = result.RefreshTokenExpiration!.Value,
            FacilityId = result.FacilityId!.Value,
            FacilityName = result.FacilityName!,
            TenantId = result.TenantId!.Value,
            TenantName = result.TenantName!
        });
    }

    /// <summary>
    /// アクセス可能な施設一覧を取得
    /// </summary>
    [HttpGet("accessible-facilities")]
    [Authorize]
    public async Task<IActionResult> GetAccessibleFacilities()
    {
        var userIdClaim = this.User.FindFirst("sub")?.Value;
        if (String.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return this.Unauthorized(new { message = "Invalid user" });
        }

        var facilities = await this._authService.GetAccessibleFacilitiesAsync(userId);

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

    /// <summary>
    /// ログアウト
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var success = await this._authService.LogoutAsync(request.SessionId);

        if (!success)
        {
            return this.BadRequest(new { message = "Logout failed" });
        }

        return this.Ok(new { message = "Logged out successfully" });
    }
}

// Request DTOs
public record LoginRequest(string UserCode, string Password);
public record RefreshRequest(Guid UserId, string RefreshToken, Guid? FacilityId = null);
public record SwitchFacilityRequest(Guid FacilityId);
public record LogoutRequest(Guid SessionId);

// Response DTOs
public record LoginResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime RefreshTokenExpiration { get; init; }
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
}

public record RefreshResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime RefreshTokenExpiration { get; init; }
}

public record SwitchFacilityResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
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
