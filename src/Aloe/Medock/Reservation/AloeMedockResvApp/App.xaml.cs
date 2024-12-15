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
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using CommandLine;

namespace Aloe.Medock.Reservation.AloeMedockResvApp;

/// <summary>
/// アプリケーション本体の処理の開始点です。
/// Program.Main → App.Ctor → App.OnStartup と呼び出されます。
/// </summary>
public partial class App : Application
{
    private readonly Timestamper _ts = new Timestamper("App");

    private readonly Arguments _arguments;
    private ILogger? _logger;
    private IHost? _host;

    private LoginWindow? _loginWindow;
    private TaskbarIcon? _notifyIcon;

    public App(Arguments arguments)
    {
        this._ts.Stamp("Ctor");

        this._arguments = arguments;
        this.RegisterUnhandledExceptionHandlers();

        this._ts.Stamp("Ctor finished");
    }

    protected async override void OnStartup(StartupEventArgs e)
    {
        try
        {
            this._ts.Stamp("OnStartup");

            base.OnStartup(e);

            var ini = LoginIni.Load(App.IniFilePath);

            if (this._arguments.ScreenCode.IsDefault())
            {
                this._loginWindow = new LoginWindow(ini);
                Application.Current.MainWindow = this._loginWindow;
                this._loginWindow.Show();
            }

            var task = Task.Run(async () =>
            {
                this._host = this.InitializeHost(this._arguments);
                App.s_services = this.Host.Services;
                this._logger = this.Host.Services.GetService<ILogger<App>>();

                if (this._arguments.Standalone)
                {
                    await this.InitializeDatabaseAsync();
                }

                if (this._arguments.IsSeed)
                {
                    await this.InitializeSeedAsync();
                }
            }).ContinueWith(_ =>
            {
                if (this._loginWindow is not null)
                {
                    // 別スレッドからUIスレッドを操作
                    this.Dispatcher.Invoke(() =>
                    {
                        this.InitializeNotifyIcon();

                        // ロードが終わったらインジケーターを止める
                        this._loginWindow.CompleteInitializingTask("Startup finished.");
                    });
                }
            });

            if (!String.IsNullOrWhiteSpace(this._arguments.User))
            {
                var isSuccessful = await this.LoginAsync(task, this._arguments.User, this._arguments.Password);
                if (isSuccessful)
                {
                    return;
                }
            }
            else if (ini.IsReadyForAutoLogin)
            {
                var isSuccessful = await this.LoginAsync(task, ini.User!, ini.Password!);
                if (isSuccessful)
                {
                    return;
                }
            }

            if (this._loginWindow is null)
            {
                // 画面指定があったら開けないので終了
                this.Shutdown(0);
            }
        }
        catch (Exception ex)
        {
            this._logger?.LogError(ex, "Error!");
            throw;
        }
        finally
        {
            this._ts.Stamp("OnStartup finally");
            this._ts.DumpAsync();
        }
    }

    private void SetStatus(string message)
    {
        this._ts.Stamp(message);
        if (this._loginWindow is not null)
        {
            this.Dispatcher.Invoke(() =>
            {
                this._loginWindow.SetStatus(message);
            });
        }
    }

    /// <summary>
    /// Generic Host(IHost) で Configuration, ILogger, IServiceProvider を使用できるようにします。
    /// </summary>
    private IHost InitializeHost(Arguments arguments)
    {
        this.SetStatus("Host Initializing...");

        var args = Environment.GetCommandLineArgs();

        // Generic Host を使って設定を共通化している
        var host = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args)
            .ConfigureBuilder(arguments)
            .Build();

        // App.Run と競合するため IHost.Run はしない
        //host.Run();

        // 必要なら手動で実行する
        //host.StartAsync();

        this._ts.Stamp("Host initialized");

        return host;
    }

    private async Task<bool> LoginAsync(Task task, string user, string password)
    {
        // TODO: クラサバのときini から HostUrl が取れる場合は待たずに生成してしまえばよい
        this.SetStatus("Task Waiting...");
        await Task.WhenAll(task);

        var sp = this.Host.Services;
        var auth = sp.GetRequiredService<IAuthGrpcService>();
        var request = new LoginRequest()
        {
            LoginName = user,
            Password = password,
            ClientAppName = App.AppName,
        };

        this.SetStatus("Login trying...");
        var result = await auth.LoginAsync(request);

        if (result.IsSuccess)
        {
            this.SetStatus("Login successful.");

            App.Session = result.SessionDto;

            Window window = this._arguments.ScreenCode switch
            {
                ScreenCode.ReservationEquip => sp.GetRequiredService<ReservationEquipWindow>(),
                _ => sp.GetRequiredService<ReservationMainWindow>(),
            };

            window.Show();
            this._loginWindow?.Close();
            Application.Current.MainWindow = window;
            return true;
        }
        else
        {
            var msg = $"Login failed. {result.ErrorMessage}";
            this.SetStatus(msg);
            this._logger?.LogInformation(msg);
            return false;
        }
    }

    /// <summary>
    /// ポリシーを事前にロードしておきます。
    /// EFCore は初回アクセス時にマッピングなどが行われるため時間がかかります。
    /// スタンドアローンで動かす場合には非同期で呼び出してください。
    /// </summary>
    private async ValueTask InitializeDatabaseAsync()
    {
        this.SetStatus("DB Initializing...");

        try
        {
            var auth = this.Host.Services.GetRequiredService<IAuthGrpcService>();
            await auth.LoadPoliciesAsync();
        }
        catch (Exception ex)
        {
            this._logger?.LogError(ex, "Error!");
        }

        this._ts.Stamp("DB initialized");
    }

    /// <summary>
    /// 必要なサンプルデータを作成します。
    /// すでにデータが存在する場合は何もしません。
    /// </summary>
    private async ValueTask InitializeSeedAsync()
    {
        this.SetStatus("DB Seeding...");

        try
        {
            var seed = this.Host.Services.GetService<ISeedGrpcService>();
            if (seed is not null)
            {
                await seed.SeedAsync();
            }
        }
        catch (Exception ex)
        {
            this._logger?.LogError(ex, "Error!");
        }

        this._ts.Stamp("DB Seeded");
    }

    /// <summary>
    /// タスクトレイアイコンを作成します。
    /// </summary>
    private void InitializeNotifyIcon()
    {
        this.SetStatus("NotifyIcon Initializing...");

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

        this._ts.Stamp("NotifyIcon initialized");
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
            notifyIcon.DataContext = this.Host.Services.GetService<NotifyIconViewModel>();
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
