using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockServer.ApplicationServices.Mobile;
using Aloe.Apps.MedockServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Aloe.Apps.MedockServer.Components.Pages;

public partial class Login : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

    [Inject]
    private IWebHostEnvironment Environment { get; set; } = default!;

    [SupplyParameterFromForm]
    private LoginModel loginModel { get; set; } = default!;

    private bool IsLoading { get; set; }
    private string? ErrorMessage { get; set; }
    private string? DebugMessage { get; set; }

    private const string RememberUserCodeCookieName = "remember_user_code";
    private const string KeepSessionCookieName = "keep_session";

    protected override void OnInitialized()
    {
        this.loginModel ??= new LoginModel();
        var httpContext = this.HttpContextAccessor.HttpContext;
        if (httpContext?.Request.Cookies != null)
        {
            if (httpContext.Request.Cookies.TryGetValue(RememberUserCodeCookieName, out var savedUserCode) && !string.IsNullOrEmpty(savedUserCode))
            {
                this.loginModel.UserCode = savedUserCode;
                this.loginModel.RememberMe = true;
            }
            if (httpContext.Request.Cookies.TryGetValue(KeepSessionCookieName, out var keepSession) && keepSession == "true")
            {
                this.loginModel.KeepSession = true;
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
            if (string.IsNullOrEmpty(this.loginModel.UserCode) || string.IsNullOrEmpty(this.loginModel.Password))
            {
                this.ErrorMessage = "ユーザーIDとパスワードを入力してください。";
                this.IsLoading = false;
                this.StateHasChanged();
                return;
            }

            var httpContext = this.HttpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                this.ErrorMessage = "ログイン処理に失敗しました。";
                this.IsLoading = false;
                this.StateHasChanged();
                return;
            }

            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            var userAgent = httpContext.Request.Headers.UserAgent.ToString();
            var appName = "MedockServer";

            var result = await this.AuthService.LoginAsync(
                this.loginModel.UserCode,
                this.loginModel.Password,
                appName,
                ipAddress,
                userAgent);

            if (!result.IsSuccess)
            {
                this.ErrorMessage = result.ErrorMessage ?? "ログインに失敗しました。";
                this.IsLoading = false;
                this.StateHasChanged();
                return;
            }

            var isMobile = MobileDetectionHelper.IsMobile(userAgent);
            var (_, claimsPrincipal) = ClaimsPrincipalFactory.Create(result, isMobile);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = this.loginModel.KeepSession,
                ExpiresUtc = this.loginModel.KeepSession
                    ? result.RefreshTokenExpiration?.ToUniversalTime() ?? DateTimeOffset.UtcNow.AddDays(30)
                    : null
            };

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

            // RememberMe: UserCode を Cookie で保存/削除
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = !this.Environment.IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                Path = "/"
            };
            if (this.loginModel.RememberMe)
            {
                cookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(90);
                httpContext.Response.Cookies.Append(RememberUserCodeCookieName, this.loginModel.UserCode, cookieOptions);
            }
            else
            {
                httpContext.Response.Cookies.Delete(RememberUserCodeCookieName, new CookieOptions { Path = "/" });
            }

            // KeepSession: UI 復元用に Cookie で保存/削除
            if (this.loginModel.KeepSession)
            {
                cookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(90);
                httpContext.Response.Cookies.Append(KeepSessionCookieName, "true", cookieOptions);
            }
            else
            {
                httpContext.Response.Cookies.Delete(KeepSessionCookieName, new CookieOptions { Path = "/" });
            }

            var facilities = await this.AuthService.GetAccessibleFacilitiesAsync(result.UserId!.Value);
            var calendarPath = isMobile ? "/m/calendar" : "/calendar";
            var redirectUrl = facilities.Count > 1 ? "/facility-select" : calendarPath;

            this.NavigationManager.NavigateTo(redirectUrl, forceLoad: true);
        }
        catch (Exception ex)
        {
            this.ErrorMessage = "ログイン処理中にエラーが発生しました。";
            this.DebugMessage = ex.Message;
            this.IsLoading = false;
            this.StateHasChanged();
        }
    }

    public class LoginModel
    {
        public string UserCode { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public bool KeepSession { get; set; }
    }
}
