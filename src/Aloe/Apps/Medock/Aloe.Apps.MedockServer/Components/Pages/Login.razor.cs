using Aloe.Apps.MedockLib.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace Aloe.Apps.MedockServer.Components.Pages;

public partial class Login : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private ProtectedLocalStorage LocalStorage { get; set; } = default!;

    [Inject]
    private IUserContextService UserContextService { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [SupplyParameterFromForm]
    private LoginModel loginModel { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        this.loginModel ??= new LoginModel();


    }

    private bool IsLoading { get; set; } = false;
    private string? ErrorMessage { get; set; }
    private string? DebugMessage { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // RememberMeが保存されていれば読み込む
            try
            {
                var savedUserCode = await this.LocalStorage.GetAsync<string>("remember_user_code");
                if (savedUserCode.Success && !String.IsNullOrEmpty(savedUserCode.Value))
                {
                    this.loginModel.UserCode = savedUserCode.Value;
                    this.loginModel.RememberMe = true;
                    this.StateHasChanged();
                }

                // KeepSession: UIに復元
                var savedKeepSession = await this.LocalStorage.GetAsync<bool>("keep_session");
                if (savedKeepSession.Success && savedKeepSession.Value)
                {
                    this.loginModel.KeepSession = true;
                    this.StateHasChanged();
                }
            }
            catch
            {
                // ローカルストレージアクセス失敗は無視
            }
        }
    }

    private async Task HandleLogin()
    {
        this.IsLoading = true;
        this.ErrorMessage = null;
        this.DebugMessage = null;
        this.StateHasChanged();

        try
        {
            if (String.IsNullOrEmpty(this.loginModel.UserCode) || String.IsNullOrEmpty(this.loginModel.Password))
            {
                this.ErrorMessage = "ユーザーIDとパスワードを入力してください。";
                return;
            }

            // RememberMe: ユーザーIDを保存
            if (this.loginModel.RememberMe)
            {
                await this.LocalStorage.SetAsync("remember_user_code", this.loginModel.UserCode);
            }
            else
            {
                await this.LocalStorage.DeleteAsync("remember_user_code");
            }

            // KeepSession: UI状態を保存
            if (this.loginModel.KeepSession)
            {
                await this.LocalStorage.SetAsync("keep_session", true);
            }
            else
            {
                await this.LocalStorage.DeleteAsync("keep_session");
            }

            // JavaScript interopを使ってクライアント側からAPIを呼び出す
            // 重要な点: HttpOnlyクッキーは正しく設定されます
            // - Set-Cookieヘッダーはサーバー（AuthController）から返される
            // - ブラウザがSet-Cookieヘッダーを処理してクッキーを保存する
            // - HttpOnly属性が含まれているため、JavaScriptからは読み取れない
            // - これはJavaScript interopとは無関係で、サーバー側の設定（Program.cs）で制御される
            var loginRequest = new
            {
                UserCode = this.loginModel.UserCode,
                Password = this.loginModel.Password,
                KeepSession = this.loginModel.KeepSession
            };

            var loginResponse = await this.JSRuntime.InvokeAsync<LoginResponse?>("loginApi.login", loginRequest);

            if (loginResponse != null && loginResponse.SessionId != Guid.Empty)
            {
                // UserContextServiceにセッションIDを設定
                this.UserContextService.SetSessionId(loginResponse.SessionId);

                // 遷移先を決定（複数施設なら施設選択へ）
                // forceLoad: true でページをリロードし、新しいクッキーの認証情報を反映させる
                var facilities = await this.AuthService.GetAccessibleFacilitiesAsync(loginResponse.UserId);
                var calendarPath = loginResponse.IsMobile ? "/m/calendar" : "/calendar";
                if (facilities.Count > 1)
                {
                    // 複数施設がある場合は施設選択画面へ
                    this.NavigationManager.NavigateTo("/facility-select", forceLoad: true);
                }
                else
                {
                    // 単一施設の場合は直接メイン画面へ
                    this.NavigationManager.NavigateTo(calendarPath, forceLoad: true);
                }
            }
            else
            {
                this.ErrorMessage = "ログインに失敗しました。";
                this.DebugMessage = "Login failed";
            }
        }
        catch (Exception ex)
        {
            this.ErrorMessage = "ログイン処理中にエラーが発生しました。";
            this.DebugMessage = $"Exception: {ex.Message}";
            this.IsLoading = false;
            this.StateHasChanged();
        }
    }

    public class LoginModel
    {
        public string UserCode { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public bool RememberMe { get; set; } = false;
        public bool KeepSession { get; set; } = false;
    }

    // JavaScript interop用のレスポンスクラス
    private class LoginResponse
    {
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public string UserCode { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string DisplayName { get; set; } = String.Empty;
        public Guid? TenantId { get; set; }
        public string TenantName { get; set; } = String.Empty;
        public Guid? FacilityId { get; set; }
        public string FacilityName { get; set; } = String.Empty;
        public bool IsSystemAdmin { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
        public bool IsMobile { get; set; }
    }
}


