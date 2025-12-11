using Aloe.Apps.MedockLib.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Aloe.Apps.MedockServer.Components.Layout;

public partial class MainLayout : LayoutComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private ProtectedLocalStorage LocalStorage { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private IUserContextService UserContextService { get; set; } = default!;

    private UserContextInfo? userContext;
    private bool hasMultipleFacilities;
    private List<FacilityInfo> availableFacilities = new();

    protected override async Task OnInitializedAsync()
    {
        // AuthenticationStateProviderから認証状態を取得
        var authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            this.UserContextService.InitializeFromClaims(user);
            this.userContext = this.UserContextService.CurrentUser;

            // アクセス可能な施設一覧を取得
            if (this.userContext != null && this.userContext.UserId != Guid.Empty)
            {
                this.availableFacilities = await this.UserContextService.GetAccessibleFacilitiesAsync();
                this.hasMultipleFacilities = this.availableFacilities.Count > 1;
            }
        }
    }

    private async Task HandleFacilitySwitch(Guid facilityId)
    {
        if (this.userContext == null || this.userContext.UserId == Guid.Empty)
        {
            return;
        }

        try
        {
            // 施設切り替えAPIを呼び出し（クッキーが自動的に更新される）
            var result = await this.AuthService.SwitchFacilityAsync(this.userContext.UserId, facilityId);
            if (result.IsSuccess)
            {
                // ページをリロードして新しい認証状態を反映
                this.NavigationManager.NavigateTo(this.NavigationManager.Uri, forceLoad: true);
            }
        }
        catch
        {
            // エラーは無視（ユーザーに通知する場合はここで処理）
        }
    }

    private async Task HandleLogout()
    {
        try
        {
            // ログアウトAPIを呼び出す（クッキーが自動的に削除される）
            var authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync();
            var sessionIdClaim = authState.User.FindFirst("session_id")?.Value;
            if (!String.IsNullOrEmpty(sessionIdClaim) && Guid.TryParse(sessionIdClaim, out var sessionId))
            {
                await this.AuthService.LogoutAsync(sessionId);
            }
        }
        catch
        {
            // ログアウトAPIの失敗は無視
        }
        finally
        {
            // ローカルストレージをクリア（remember_user_codeとkeep_sessionは残す）
            try
            {
                await this.LocalStorage.DeleteAsync("keep_session");
            }
            catch
            {
                // ストレージクリア失敗も無視
            }

            // ログアウトAPIエンドポイントを呼び出してクッキーを削除
            this.NavigationManager.NavigateTo("/api/auth/logout", forceLoad: true);
        }
    }
}


