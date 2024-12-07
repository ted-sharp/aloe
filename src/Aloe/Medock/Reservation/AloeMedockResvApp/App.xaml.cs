using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services;
using Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Login;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using CommunityToolkit.Diagnostics;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Aloe.Common.AloeCoreLib.Ini;
using System.Reflection;
using static Aloe.Medock.Reservation.AloeMedockResvApp.Program;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvApp;

/// <summary>
/// アプリケーション本体の処理の開始点です。
/// Program.Main → WpfHostService.StartAsync → App と呼び出されます。
/// </summary>
public partial class App : Application
{
    private static readonly AssemblyName s_asmName = Assembly.GetExecutingAssembly().GetName();

    private static readonly string s_iniFilePath =
        System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            App.s_asmName.Name ?? "AppName",
            "app.ini");

    private readonly Arguments _arguments;
    private IHost? _host;
    private ILogger? _logger;

    private TaskbarIcon? _notifyIcon;

    public App(Arguments arguments)
    {
        Timestamper.Global.Stamp("App ctor.");

        this._arguments = arguments;
        this.RegisterUnhandledExceptionHandlers();
    }

    protected async override void OnStartup(StartupEventArgs e)
    {
        try
        {
            Timestamper.Global.Stamp("App OnStartup.");

            base.OnStartup(e);

            var ini = LoginIni.Load(App.s_iniFilePath);

            var loginWindow = new LoginWindow(ini);
            Application.Current.MainWindow = loginWindow;

            Timestamper.Global.Stamp("App OnStartup: Create LoginWindow.");

            var task = Task.Run(async () =>
            {
                // _host を起点に処理するので最初に初期化する
                this._host = this.InitializeHost();
                this._logger = this._host.Services.GetService<ILogger<App>>();


                if (this._arguments.IsDevelopment)
                {
                    // 開発中ならサンプルデータを追加
                    await this.InitializeSeedAsync();
                }
                else
                {
                    // 開発中じゃなければタスクトレイ起動
                    this.Dispatcher.Invoke(this.InitializeNotifyIcon);
                }

                this.Dispatcher.Invoke(() =>
                {
                    // ロードが終わったらインジケーターを止める
                    loginWindow.StopIndicator();
                });
            });


            if (this._arguments.IsDevelopment)
            {
                //開発中は特定のWindowを直接表示したい
                await Task.WhenAll(task);

                this.ShowDevelWindow();
                //this.ShowFirstLoginWindow();
                return;
            }

            // TODO: arguments で指定された場合も許可したい
            if (ini.IsReadyForAutoLogin)
            {
                // IHost が使えるようになるまで待つ
                await Task.WhenAll(task);

                var auth = this._host?.Services.GetRequiredService<IAuthGrpcService>();
                var request = new LoginRequest()
                {
                    LoginName = ini.User!,
                    Password = ini.Password!,
                    ClientAppName = $"{App.s_asmName.Name} {App.s_asmName.Version}",
                };
                var result = await auth!.LoginAsync(request);

                if (result.IsSuccess)
                {
                    App.Session = result.SessionDto;
                    // TODO: Window の表示
                    // とりあえず loginwindow を表示しておく
                    loginWindow.Show();
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage);
                }
            }
            else
            {
                loginWindow.Show();
            }
        }
        catch (Exception ex)
        {
            this._logger?.LogError(ex, "Error!");
            throw;
        }
        finally
        {
            Timestamper.Global.Stamp("App OnStartup: finally.");
            Timestamper.Global.Dump();
        }
    }

    private void ShowDevelWindow()
    {
        //var window = this._windowService.CreateWindow<ReservationMainWindow>();
        var window = this._host!.Services.GetRequiredService<ReservationEquipWindow>();

        window.ActivateOrShow();
        Application.Current.MainWindow = window;
    }

    /// <summary>
    /// Generic Host(IHost) で Configuration, ILogger, IServiceProvider を使用できるようにします。
    /// </summary>
    private IHost InitializeHost()
    {
        var args = Environment.GetCommandLineArgs();

        // Generic Host を使って設定を共通化している
        var host = Host.CreateApplicationBuilder(args)
            .ConfigureBuilder(this._arguments)
            .Build();

        // App.Run と競合するため IHost.Run はしない
        //host.Run();

        // 必要なら手動で実行する
        //host.StartAsync();

        return host;
    }

    /// <summary>
    /// 必要なサンプルデータを作成します。
    /// すでにデータが存在する場合は何もしません。
    /// </summary>
    private async ValueTask InitializeSeedAsync()
    {
        try
        {
            var seed = this._host?.Services.GetService<ISeedGrpcService>();
            if (seed is not null)
            {
                await seed.SeedAsync();
            }
        }
        catch (Exception ex)
        {
            this._logger?.LogError(ex, "Error!");
        }
    }

    private void InitializeNotifyIcon()
    {
        try
        {
            this._notifyIcon = this.CreateNotifyIcon();

            // Window なしでタスクトレイに表示
            this._notifyIcon.ForceCreate();

            // タスクトレイにあることを明示
            this._notifyIcon.ShowNotification(
                "予約システム",
                "予約システムが起動しました。\n常駐しますので、タスクトレイから操作してください。",
                NotificationIcon.Info,
                largeIcon: true,
                sound: false);
        }
        catch (Exception ex)
        {
            this._logger?.LogError(ex, "Error!");
        }
    }

    private TaskbarIcon CreateNotifyIcon()
    {
        var resources = new ResourceDictionary
        {
            Source = new Uri("/Views/Tray/NotifyIconResources.xaml", UriKind.Relative),
        };

        // 必要なリソースを参照して使用する
        if (resources["NotifyIcon"] is TaskbarIcon notifyIcon)
        {

            notifyIcon.DataContext = this._host?.Services.GetService<NotifyIconViewModel>();
            return notifyIcon;
        }

        throw new InvalidOperationException("Can Not CreateNotifyIcon.");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // アイコンのクリーンアップ
        this._notifyIcon?.Dispose();
        this._notifyIcon = null;

        this.UnregisterUnhandledExceptionHandlers();

        base.OnExit(e);
    }

}

