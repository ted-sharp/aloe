using Aloe.Apps.MedockLib.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Aloe.Apps.MedockServer.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {

        // このページはルートパス（/）へのアクセス時に表示される。
        // 認証状態に応じて適切なページへリダイレクトする。
        // - 未ログイン → /login
        // - ログイン済み（複数施設） → /facility-select
        // - ログイン済み（単一施設） → /calendar\

        // 認証状態を確認
        try
        {
            var authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                // ユーザーIDを取得
                var userIdClaim = authState.User.FindFirst("sub")?.Value;
                if (!String.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
                {
                    // 施設数をチェック
                    var facilities = await this.AuthService.GetAccessibleFacilitiesAsync(userId);
                    if (facilities.Count > 1)
                    {
                        // TODO: 施設選択済みであれば、カレンダーへ移動したい

                        // 複数施設がある場合は施設選択画面へ
                        this.NavigationManager.NavigateTo("/facility-select", forceLoad: true);
                        return;
                    }
                    else
                    {
                        // 単一施設の場合は直接メイン画面へ
                        this.NavigationManager.NavigateTo("/calendar", forceLoad: true);
                        return;
                    }
                }
            }
        }
        catch
        {
            // 認証状態チェック失敗は無視（ログイン画面へ）
        }

        // 未ログインの場合はログイン画面へリダイレクト
        this.NavigationManager.NavigateTo("/login", forceLoad: true);
    }
}


