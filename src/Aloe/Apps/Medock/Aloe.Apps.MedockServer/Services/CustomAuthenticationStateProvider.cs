using System.Security.Claims;
using Aloe.Apps.MedockLib.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockServer.Services;

/// <summary>
/// カスタム認証状態プロバイダー
/// LocalStorageからJWTトークンを取得して認証状態を管理します。
/// </summary>
public class CustomAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly IJwtTokenService _jwtTokenService;

    public CustomAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        ProtectedLocalStorage localStorage,
        IJwtTokenService jwtTokenService)
        : base(loggerFactory)
    {
        this._localStorage = localStorage;
        this._jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// 再検証間隔を取得します。
    /// </summary>
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(5);

    /// <summary>
    /// 認証状態を取得します。
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // LocalStorageからaccess_tokenを取得
            var tokenResult = await this._localStorage.GetAsync<string>("access_token");

            if (!tokenResult.Success || String.IsNullOrEmpty(tokenResult.Value))
            {
                // トークンが存在しない場合は未認証状態を返す
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // JWTトークンを検証
            var principal = this._jwtTokenService.ValidateToken(tokenResult.Value);

            if (principal == null)
            {
                // トークンが無効な場合は未認証状態を返す
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // 認証済み状態を返す
            return new AuthenticationState(principal);
        }
        catch
        {
            // エラーが発生した場合は未認証状態を返す
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    /// <summary>
    /// 認証状態を再検証します。
    /// </summary>
    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        try
        {
            // LocalStorageからaccess_tokenを取得
            var tokenResult = await this._localStorage.GetAsync<string>("access_token");

            if (!tokenResult.Success || String.IsNullOrEmpty(tokenResult.Value))
            {
                // トークンが存在しない場合は無効
                return false;
            }

            // JWTトークンを検証
            var principal = this._jwtTokenService.ValidateToken(tokenResult.Value);

            if (principal == null)
            {
                // トークンが無効な場合は無効
                return false;
            }

            // 現在の認証状態と比較
            var currentUserId = authenticationState.User.FindFirst("sub")?.Value;
            var newUserId = principal.FindFirst("sub")?.Value;

            // ユーザーIDが一致する場合は有効
            return currentUserId == newUserId;
        }
        catch
        {
            // エラーが発生した場合は無効
            return false;
        }
    }
}

