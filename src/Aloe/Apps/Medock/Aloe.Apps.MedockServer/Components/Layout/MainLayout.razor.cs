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
            // 施設切り替えAPIを呼び出し
            var result = await this.AuthService.SwitchFacilityAsync(this.userContext.UserId, facilityId);
            if (result.IsSuccess && !String.IsNullOrEmpty(result.AccessToken))
            {
                // 新しいトークンをLocalStorageに保存
                await this.LocalStorage.SetAsync("access_token", result.AccessToken);
                if (result.RefreshToken != null)
                {
                    await this.LocalStorage.SetAsync("refresh_token", result.RefreshToken);
                }

                // ページをリロードして新しいトークンで認証
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
            // セッションIDを取得してログアウトAPIを呼び出す
            var sessionIdResult = await this.LocalStorage.GetAsync<string>("session_id");
            if (sessionIdResult.Success && !String.IsNullOrEmpty(sessionIdResult.Value))
            {
                if (Guid.TryParse(sessionIdResult.Value, out var sessionId))
                {
                    await this.AuthService.LogoutAsync(sessionId);
                }
            }
        }
        catch
        {
            // ログアウトAPIの失敗は無視（ローカルデータはクリアする）
        }
        finally
        {
            // ローカルストレージをクリア
            try
            {
                await this.LocalStorage.DeleteAsync("session_id");
                await this.LocalStorage.DeleteAsync("keep_session");
                await this.LocalStorage.DeleteAsync("access_token");
                await this.LocalStorage.DeleteAsync("remember_user_code");
            }
            catch
            {
                // ストレージクリア失敗も無視
            }

            // ログインページにリダイレクト
            this.NavigationManager.NavigateTo("/login", forceLoad: true);
        }
    }
}


