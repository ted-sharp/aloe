using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockServer.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Aloe.Apps.MedockServer.Components.Pages;

/// <summary>
/// カレンダーページのユーザー情報と認証関連処理サービス
/// </summary>
public class CalendarUserService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IAuthService _authService;
    private readonly NavigationManager _navigationManager;

    public CalendarUserService(
        AuthenticationStateProvider authStateProvider,
        IAuthService authService,
        NavigationManager navigationManager)
    {
        this._authStateProvider = authStateProvider;
        this._authService = authService;
        this._navigationManager = navigationManager;
    }

    /// <summary>
    /// ユーザー情報をロードして状態に反映します。
    /// </summary>
    public async Task LoadUserInfoAsync(CalendarState state)
    {
        try
        {
            var authState = await this._authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                state.UserDisplayName = user.FindFirst("user_display_name")?.Value
                    ?? user.FindFirst("preferred_username")?.Value
                    ?? user.Identity.Name
                    ?? "";

                state.UserEmail = user.FindFirst("email")?.Value ?? "";
                state.TenantName = user.FindFirst("tenant_name")?.Value ?? "";
                state.FacilityName = user.FindFirst("facility_name")?.Value ?? "";

                var roles = user.FindAll("roles").Select(c => c.Value).ToList();
                state.UserRole = roles.FirstOrDefault() ?? "";

                if (Guid.TryParse(user.FindFirst("facility_id")?.Value, out var facilityId))
                {
                    state.CurrentFacilityId = facilityId;
                }

                if (!String.IsNullOrEmpty(state.UserDisplayName))
                {
                    state.UserInitial = state.UserDisplayName[..1].ToUpper();
                }

                if (Guid.TryParse(user.FindFirst("sub")?.Value, out var userId))
                {
                    state.AvailableFacilities = await this._authService.GetAccessibleFacilitiesAsync(userId);
                    state.HasMultipleFacilities = state.AvailableFacilities.Count > 1;
                }
            }
        }
        catch
        {
            // ユーザー情報取得失敗時は初期値のまま
        }
    }

    /// <summary>
    /// ログアウト処理を実行します。
    /// </summary>
    public async Task HandleLogoutAsync()
    {
        try
        {
            var authState = await this._authStateProvider.GetAuthenticationStateAsync();
            var sessionIdClaim = authState.User.FindFirst("session_id")?.Value;
            if (!String.IsNullOrEmpty(sessionIdClaim) && Guid.TryParse(sessionIdClaim, out var sessionId))
            {
                await this._authService.LogoutAsync(sessionId);
            }
        }
        catch
        {
            // ログアウトAPIの失敗は無視
        }
        finally
        {
            this._navigationManager.NavigateTo("/api/auth/logout", forceLoad: true);
        }
    }

    /// <summary>
    /// 施設切り替え処理を実行します。
    /// </summary>
    public async Task<bool> HandleFacilitySwitchAsync(Guid facilityId)
    {
        try
        {
            var authState = await this._authStateProvider.GetAuthenticationStateAsync();
            var userIdClaim = authState.User.FindFirst("sub")?.Value;

            if (String.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return false;
            }

            var result = await this._authService.SwitchFacilityAsync(userId, facilityId);
            if (result.IsSuccess)
            {
                this._navigationManager.NavigateTo("/calendar", forceLoad: true);
                return true;
            }
        }
        catch
        {
            // 施設切替失敗時は何もしない
        }

        return false;
    }
}
