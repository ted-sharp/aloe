using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        // TODO: 認証状態のチェック
        // var isAuthenticated = await CheckAuthentication();
        // var tenantCount = await GetUserTenantCount();

        // デモ用: 常にログイン画面へリダイレクト
        await Task.Delay(100);
        this.NavigationManager.NavigateTo("/login");
    }
}


