using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockServer.Services;

/// <summary>
/// カスタム認証状態プロバイダー
/// HttpContextから認証情報を取得して認証状態を管理します。
/// </summary>
public class CustomAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor)
        : base(loggerFactory)
    {
        this._httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 再検証間隔を取得します。
    /// </summary>
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(5);

    /// <summary>
    /// 認証状態を取得します。
    /// </summary>
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = this._httpContextAccessor.HttpContext;
        var user = httpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

        return Task.FromResult(new AuthenticationState(user));
    }

    /// <summary>
    /// 認証状態を再検証します。
    /// </summary>
    protected override Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        var httpContext = this._httpContextAccessor.HttpContext;
        var currentUser = httpContext?.User;

        // 認証されていない場合は無効
        if (currentUser == null || currentUser.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult(false);
        }

        // 現在の認証状態と比較
        var currentUserId = authenticationState.User.FindFirst("sub")?.Value;
        var newUserId = currentUser.FindFirst("sub")?.Value;

        // ユーザーIDが一致する場合は有効
        return Task.FromResult(currentUserId == newUserId);
    }
}

