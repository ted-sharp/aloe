using Aloe.Apps.MedockLib.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Aloe.Apps.MedockServer.Components.Pages;

public partial class TenantSelect : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private AuthService AuthService { get; set; } = default!;

    [Inject]
    private ProtectedLocalStorage LocalStorage { get; set; } = default!;

    [Inject]
    private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

    [Inject]
    private UserContextService UserContextService { get; set; } = default!;

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
            this.UserContextService.InitializeFromClaims(httpContext.User);
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
            else
            {
                // ユーザー情報が取得できない場合は、LocalStorageからアクセストークンを取得してユーザーIDを抽出
                var accessTokenResult = await this.LocalStorage.GetAsync<string>("access_token");
                if (accessTokenResult.Success && !String.IsNullOrEmpty(accessTokenResult.Value))
                {
                    // JWTトークンからユーザーIDを直接抽出（簡易的な方法）
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadJwtToken(accessTokenResult.Value);
                        var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub");
                        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                        {
                            this.Facilities = await this.AuthService.GetAccessibleFacilitiesAsync(userId);
                        }
                    }
                    catch
                    {
                        // トークンパース失敗は無視
                    }
                }
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
            // 施設切り替えAPIを呼び出し
            var result = await this.AuthService.SwitchFacilityAsync(this.userContext.UserId, this.SelectedFacilityId);
            if (result.IsSuccess && !String.IsNullOrEmpty(result.AccessToken))
            {
                // 新しいトークンをLocalStorageに保存
                await this.LocalStorage.SetAsync("access_token", result.AccessToken);
                if (result.RefreshToken != null)
                {
                    await this.LocalStorage.SetAsync("refresh_token", result.RefreshToken);
                }

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


