using Aloe.Apps.MedockLib.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Http;

namespace Aloe.Apps.MedockServer.Components.Pages;

public partial class FacilitySelect : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

    [Inject]
    private IUserContextService UserContextService { get; set; } = default!;

    private UserContextInfo? userContext;
    private bool IsLoading { get; set; } = true;
    private bool IsSwitching { get; set; } = false;
    private Guid SelectedFacilityId { get; set; } = Guid.Empty;
    private List<FacilityInfo> Facilities { get; set; } = [];

    private Dictionary<string, List<FacilityInfo>>? GroupedFacilities =>
        this.Facilities?.GroupBy(f => f.TenantName)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.OrderBy(f => f.FacilityName).ToList());

    protected override async Task OnInitializedAsync()
    {
        // HttpContextからClaimsPrincipalを取得
        var httpContext = this.HttpContextAccessor.HttpContext;
        if (httpContext?.User != null)
        {
            await this.UserContextService.InitializeFromClaimsAsync(httpContext.User);
            this.userContext = this.UserContextService.CurrentUser;
        }

        await this.LoadFacilities();
    }

    private async Task LoadFacilities()
    {
        this.IsLoading = true;
        this.StateHasChanged();

        try
        {
            if (this.userContext != null && this.userContext.UserId != Guid.Empty)
            {
                this.Facilities = await this.UserContextService.GetAccessibleFacilitiesAsync();
            }
        }
        catch
        {
            // エラーは無視
        }

        this.IsLoading = false;
        this.StateHasChanged();
    }

    private void SelectFacility(Guid facilityId)
    {
        this.SelectedFacilityId = facilityId;
    }

    private async Task ConfirmSelection()
    {
        if (this.SelectedFacilityId == Guid.Empty || this.userContext == null || this.userContext.UserId == Guid.Empty)
        {
            return;
        }

        this.IsSwitching = true;
        this.StateHasChanged();

        try
        {
            // 施設切り替えAPIを呼び出し（クッキーが自動的に更新される）
            var result = await this.AuthService.SwitchFacilityAsync(this.userContext.UserId, this.SelectedFacilityId);
            if (result.IsSuccess)
            {
                // メイン画面に遷移
                this.NavigationManager.NavigateTo("/calendar", forceLoad: true);
            }
            else
            {
                // エラー処理
                this.IsSwitching = false;
                this.StateHasChanged();
            }
        }
        catch
        {
            this.IsSwitching = false;
            this.StateHasChanged();
        }
    }
}
