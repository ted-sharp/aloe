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
        _authService = authService;
    }

    /// <summary>
    /// ログイン
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var clientEndpoint = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var clientAppName = Request.Headers.UserAgent.ToString();

        var result = await _authService.LoginAsync(
            request.UserCode,
            request.Password,
            clientAppName,
            clientEndpoint);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        return Ok(new LoginResponse
        {
            AccessToken = result.AccessToken!,
            RefreshToken = result.RefreshToken!,
            RefreshTokenExpiration = result.RefreshTokenExpiration!.Value,
            SessionId = result.SessionId!.Value,
            UserId = result.UserId!.Value,
            UserCode = result.UserCode!,
            Email = result.Email!,
            TenantId = result.TenantId,
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
        var result = await _authService.RefreshTokenAsync(request.UserId, request.RefreshToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        return Ok(new RefreshResponse
        {
            AccessToken = result.AccessToken!,
            RefreshToken = result.RefreshToken!,
            RefreshTokenExpiration = result.RefreshTokenExpiration!.Value
        });
    }

    /// <summary>
    /// ログアウト
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var success = await _authService.LogoutAsync(request.SessionId);

        if (!success)
        {
            return BadRequest(new { message = "Logout failed" });
        }

        return Ok(new { message = "Logged out successfully" });
    }
}

// Request DTOs
public record LoginRequest(string UserCode, string Password);
public record RefreshRequest(Guid UserId, string RefreshToken);
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
    public Guid? TenantId { get; init; }
    public required string[] Roles { get; init; }
}

public record RefreshResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime RefreshTokenExpiration { get; init; }
}


