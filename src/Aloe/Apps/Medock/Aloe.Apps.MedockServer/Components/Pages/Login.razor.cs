using Aloe.Apps.MedockLib.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Aloe.Apps.MedockServer.Components.Pages;

public partial class Login : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private ProtectedLocalStorage LocalStorage { get; set; } = default!;

    [Inject]
    private IUserContextService UserContextService { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [SupplyParameterFromForm]
    private LoginModel loginModel { get; set; } = default!;

    protected override void OnInitialized()
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

                // KeepSessionが有効ならセッション確認して自動ログイン
                var savedSessionId = await this.LocalStorage.GetAsync<string>("session_id");
                var savedAccessToken = await this.LocalStorage.GetAsync<string>("access_token");

                if (savedKeepSession.Success && savedKeepSession.Value &&
                    savedSessionId.Success && !String.IsNullOrEmpty(savedSessionId.Value) &&
                    savedAccessToken.Success && !String.IsNullOrEmpty(savedAccessToken.Value))
                {
                    // セッション検証
                    if (Guid.TryParse(savedSessionId.Value, out var sessionId))
                    {
                        var validationResult = await this.AuthService.ValidateSessionAsync(savedAccessToken.Value, sessionId);
                        if (validationResult.IsValid)
                        {
                            // 有効なセッションがあれば自動遷移
                            this.NavigationManager.NavigateTo("/calendar");
                        }
                        else
                        {
                            // セッション検証失敗 → リフレッシュトークンで更新を試行
                            var savedRefreshToken = await this.LocalStorage.GetAsync<string>("refresh_token");
                            var savedUserId = await this.LocalStorage.GetAsync<string>("user_id");

                            if (savedRefreshToken.Success && !String.IsNullOrEmpty(savedRefreshToken.Value) &&
                                savedUserId.Success && Guid.TryParse(savedUserId.Value, out var userId))
                            {
                                var refreshResult = await this.AuthService.RefreshTokenAsync(userId, savedRefreshToken.Value);
                                if (refreshResult.IsSuccess)
                                {
                                    // 新しいトークンを保存して自動遷移
                                    await this.LocalStorage.SetAsync("access_token", refreshResult.AccessToken!);
                                    if (!String.IsNullOrEmpty(refreshResult.RefreshToken))
                                    {
                                        await this.LocalStorage.SetAsync("refresh_token", refreshResult.RefreshToken);
                                    }
                                    this.NavigationManager.NavigateTo("/calendar");
                                    return;
                                }
                            }

                            // リフレッシュも失敗した場合、LocalStorageをクリア
                            await this.LocalStorage.DeleteAsync("session_id");
                            await this.LocalStorage.DeleteAsync("keep_session");
                            await this.LocalStorage.DeleteAsync("access_token");
                            await this.LocalStorage.DeleteAsync("refresh_token");
                            await this.LocalStorage.DeleteAsync("user_id");
                        }
                    }
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

            this.DebugMessage = $"Attempting login for: {this.loginModel.UserCode}";
            this.StateHasChanged();

            // AuthServiceでログイン
            // Blazorサーバー側ではHttpContextに直接アクセスできないため、固定値を設定
            var result = await this.AuthService.LoginAsync(
                this.loginModel.UserCode,
                this.loginModel.Password,
                "MedockServer",
                String.Empty, // IPアドレスはサーバー側で取得できない
                "Blazor Server"); // UserAgent

            if (result.IsSuccess)
            {
                // RememberMe: ユーザーIDを保存
                if (this.loginModel.RememberMe)
                {
                    await this.LocalStorage.SetAsync("remember_user_code", this.loginModel.UserCode);
                }
                else
                {
                    await this.LocalStorage.DeleteAsync("remember_user_code");
                }

                // KeepSession: セッション情報を保存
                if (this.loginModel.KeepSession && result.SessionId.HasValue)
                {
                    await this.LocalStorage.SetAsync("session_id", result.SessionId.Value.ToString());
                    await this.LocalStorage.SetAsync("keep_session", true);
                }
                else
                {
                    await this.LocalStorage.DeleteAsync("session_id");
                    await this.LocalStorage.DeleteAsync("keep_session");
                }

                // アクセストークンを保存
                if (!String.IsNullOrEmpty(result.AccessToken))
                {
                    await this.LocalStorage.SetAsync("access_token", result.AccessToken);
                }

                // リフレッシュトークンとユーザーIDを保存（セッション維持用）
                if (!String.IsNullOrEmpty(result.RefreshToken))
                {
                    await this.LocalStorage.SetAsync("refresh_token", result.RefreshToken);
                }
                if (result.UserId.HasValue)
                {
                    await this.LocalStorage.SetAsync("user_id", result.UserId.Value.ToString());
                }

                // 認証状態を更新
                // RevalidatingServerAuthenticationStateProviderは自動的に再検証を行うため、
                // 明示的な通知は不要です。次のGetAuthenticationStateAsync呼び出しで
                // 新しい認証状態が取得されます。

                // 遷移先を決定（複数施設なら施設選択へ）
                if (result.UserId.HasValue)
                {
                    var facilities = await this.AuthService.GetAccessibleFacilitiesAsync(result.UserId.Value);
                    if (facilities.Count > 1)
                    {
                        // 複数施設がある場合は施設選択画面へ
                        this.NavigationManager.NavigateTo("/tenant-select");
                    }
                    else
                    {
                        // 単一施設の場合は直接メイン画面へ
                        this.NavigationManager.NavigateTo("/calendar");
                    }
                }
                else
                {
                    this.NavigationManager.NavigateTo("/calendar");
                }
            }
            else
            {
                this.ErrorMessage = result.ErrorMessage ?? "ログインに失敗しました。";
                this.DebugMessage = $"Login failed: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            this.ErrorMessage = "ログイン処理中にエラーが発生しました。";
            this.DebugMessage = $"Exception: {ex.Message}";
        }
        finally
        {
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
}


