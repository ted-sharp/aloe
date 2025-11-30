using System.Reflection;
using System.Windows;
using Aloe.Common.AloeCoreLib.Wpf.Extensions;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Login;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Maint;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aloe.Medock.Reservation.AloeMedockResvApp;

/// <remarks>
/// App.Global
/// </remarks>
public partial class App
{
    private static App? s_appInstance;

    public new static App Current => App.s_appInstance
        ?? throw new InvalidOperationException("App is not initialized.");

    public static readonly AssemblyName AsmName = Assembly.GetExecutingAssembly().GetName();

    public static readonly string AppVersion = $"v{App.AsmName.Version?.Major ?? 0}.{App.AsmName.Version?.Minor ?? 0}";

    public static readonly string AppName = $"{App.AsmName.Name} {App.AppVersion}";

    #region Global / Resolve

    public IHost Host => this._host
        ?? throw new InvalidOperationException("IHost is not initialized.");

    private static IServiceProvider? s_services;

    public static IServiceProvider Services => App.s_services
        ?? throw new InvalidOperationException("IServiceProvider is not initialized.");

    public static T Resolve<T>()
        where T : notnull
    {
        return App.Services.GetRequiredService<T>()
            ?? throw new InvalidOperationException($"{typeof(T).Name} can not resolve.");
    }

    #endregion  Global / Resolve

    #region Global / Notification

    // TODO: INotificationService を用意するのがよさそう

    private static TaskbarIcon? s_notifyIcon;

    public static void ShowNotification(string title, string message)
    {
        //_ = App.s_notifyIcon ?? throw new InvalidOperationException("TaskbarIcon is not initialized.");
        App.s_notifyIcon?.ShowNotification(
            title,
            message,
            sound: false,
            largeIcon: false);
    }

    #endregion Global / Notification

    public static string HostName { get; set; } = "";

    public static string DatabaseName { get; set; } = "";

    public static string HostUrl { get; set; } = "";

    #region Global / Session

    public static bool HasSession => App.Session != null;

    public static SessionDto? Session { get; set; }

    public static UserDto? User { get; set; }

    public static async Task<bool> TryLogoutAsync()
    {
        try
        {
            var session = App.Session;
            if (session is null)
            {
                return false;
            }

            var auth = App.Resolve<IAuthGrpcService>();
            await auth.LogoutAsync(session);

            App.Session = null;
            App.User = null;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// LogWindow が残るため、明示的に終了を実行します。
    /// LoginWindow が残っていたら常駐のため、ログアウトして、LoginWindow を表示します。
    /// </summary>
    public async void Window_OnClosed(object? sender, EventArgs e)
    {
        try
        {
            var windows = this.Windows
                .OfType<Window>()
                // LogWindow は必ず生成しているので除外
                .Where(x => x is not LogWindow)
                // Visual Studio デバッグ中のみ AdornerWindow が追加されるので除外
                .Where(x => x.GetType().Name != "AdornerWindow")
                .ToArray();

            // Window がなければアプリケーションを終了する
            var isRunning = windows.Any();
            if (!isRunning)
            {
                Application.Current.Shutdown();
                return;
            }

            // LoginWindow だけであれば、ログアウトして表示する
            if (windows is [LoginWindow loginWindow])
            {
                await App.TryLogoutAsync();
                loginWindow.ShowOrActivate();
            }
        }
        catch (Exception ex)
        {
            this.LogError(ex);
        }
    }

    #endregion Global / Session
}
