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
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using CommunityToolkit.Diagnostics;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Aloe.Medock.Reservation.AloeMedockResvApp.Utils;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Cust;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Maint;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Grpc.Net.Client;
using MagicOnion;
using Aloe.Common.AloeCoreLib.Wpf.Extensions;
using Aloe.Medock.Reservation.AloeMedockResvApp.Settings;
using System.Runtime;
using Microsoft.VisualBasic;

namespace Aloe.Medock.Reservation.AloeMedockResvApp;

/// <summary>
/// アプリケーション本体の処理の開始点です。
/// Program.Main → App.Ctor → App.OnStartup と呼び出されます。
/// </summary>
public partial class App : Application
{
    private readonly Timestamper _ts = new("App");

    private ILogger? _logger;

    private readonly IConfigurationRoot _config;
    private readonly AloeClientSettings _settings;

    private IHost? _host;

    private LoginWindow? _loginWindow;
    private LogWindow? _logWindow;
    private TaskbarIcon? _notifyIcon;

    public App(IConfigurationRoot config, AloeClientSettings settings)
    {
        this._ts.Stamp("Ctor");

        this._config = config;
        this._settings = settings;

        this.RegisterUnhandledExceptionHandlers();

        this._ts.Stamp("Ctor finished");
    }

    #region ILogger

    private void LogError(Exception ex)
    {
        this.LogError(ex, ex.Message);
    }

    private void LogError(Exception ex, string message)
    {
        if (this._logger is not null)
        {
            this._logger.LogError(ex, message);
        }
        else
        {
            Debug.WriteLine(message);
            Debug.WriteLine(ex.ToString());
        }
    }

    private void LogInformation(string message)
    {
        if (this._logger is not null)
        {
            this._logger.LogInformation(message);
        }
        else
        {
            Debug.WriteLine(message);
        }
    }

    private void LogFirstChanceException(Exception ex)
    {
        var isEnabled = this._settings?.IsFirstChanceExceptionLogging ?? false;
        if (!isEnabled)
        {
            return;
        }

        var message = "FirstChanceException: " + ex.Message;
        if (this._logger is not null)
        {
            this._logger.LogTrace(message);
        }
        else
        {
            Debug.WriteLine(message);
        }
    }

    #endregion ILogger

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            this._ts.Stamp("OnStartup");

            base.OnStartup(e);

            var ini = LoginIni.Load(App.IniFilePath);

            // 常駐するかどうか
            var isResided = this._settings.ScreenCode.IsDefault();

            if (isResided)
            {
                // IHost 作成前から使う
                this._loginWindow = new LoginWindow(ini);
                Application.Current.MainWindow = this._loginWindow;

                this._loginWindow.Show();

                // IHost 初期化時にログを出力する RichTextBox を渡すために事前に作成しておく
                this._logWindow = new LogWindow();
            }

            var task = Task.Run(async () =>
            {
                try
                {
                    this._host = this.InitializeHost(this._config, this._logWindow?.LogRichTextBox);
                    this._loginWindow?.InvokeCompleteInitHost("Host initialized.");

                    App.s_services = this.Host.Services;
                    this._logger = this.Host.Services.GetService<ILogger<App>>(); App.s_services = this.Host.Services;
                    //var confRoot = this.Host.Services.GetService<IConfigurationRoot>();

                    if (this._settings.IsStandalone)
                    {
                        var auth = this.Host.Services.GetRequiredService<IAuthGrpcService>();
                        // standalone の場合はDBホスト名を使う
                        App.HostName = await auth.GetDbHostAsync();
                        App.DatabaseName = await auth.GetDbNameAsync();

                        await this.InitializeDatabaseAsync();
                    }
                    else
                    {
                        var channel = this.Host.Services.GetRequiredService<GrpcChannel>();
                        App.HostName = channel.Target;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Debug.WriteLine(ex);
                    throw;
                }
            }).ContinueWith(_ =>
            {
                if (isResided)
                {
                    // 常駐する場合はトレイアイコンを作成する
                    this.InitializeNotifyIcon();
                }

                // ロードが終わったらインジケーターを止める
                this._loginWindow?.InvokeCompleteInitTask("Startup finished.");
            });

            if (!String.IsNullOrWhiteSpace(this._settings.User))
            {
                // 引数指定があるとき
                var isSuccessful = await this.LoginAsync(task,
                    this._settings.User,
                    this._settings.Password,
                    this._settings.ScreenCode);
                if (isSuccessful)
                {
                    return;
                }
            }
            else if (ini.IsReadyForAutoLogin)
            {
                // 自動ログインのとき
                var isSuccessful = await this.LoginAsync(task,
                    ini.User,
                    ini.Password);
                if (isSuccessful)
                {
                    return;
                }
            }

            if (!isResided)
            {
                // 画面指定があって開けなかったら終了
                this.Shutdown(0);
            }

            //await Task.WhenAll(task);
        }
        catch (Exception ex)
        {
            this.SetError(ex, "OnStartup failed.");
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
        this._loginWindow?.InvokeSetStatus(message);
    }

    private void SetError(Exception ex, string message)
    {
        this._ts.Stamp(message);

        this.LogError(ex, message);

        this._loginWindow?.InvokeSetStatus(message);
    }

    /// <summary>
    /// Generic Host(IHost) で Configuration, ILogger, IServiceProvider を使用できるようにします。
    /// </summary>
    /// <remarks>
    /// App.Run が実行済みのため、IHost.Run はしません。
    /// IHostedService を使いたい場合は、host.StartAsync を呼び出します。
    /// </remarks>
    private IHost InitializeHost(IConfigurationRoot config, RichTextBox? logTextBox)
    {
        this.SetStatus("Host Initializing...");

        var args = Environment.GetCommandLineArgs();

        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

        builder.Configuration.AddConfiguration(config);

        if (this._settings.IsStandalone)
        {
            builder.ConfigureStandalone(logTextBox, this._settings);
        }
        else
        {
            builder.ConfigureClient(logTextBox);
        }

        var host = builder.Build();

        this.SetStatus("Host initialized");

        return host;
    }

    private async Task<bool> LoginAsync(Task task, string? user, string? password, ScreenCode screenCode = ScreenCode.None)
    {
        this.SetStatus("Task Waiting...");
        await Task.WhenAll(task);

        var sp = this.Host.Services;
        var auth = sp.GetRequiredService<IAuthGrpcService>();
        var request = new LoginRequest()
        {
            LoginName = user ?? "",
            Password = password ?? "",
            ClientAppName = App.AppName,
        };

        this.SetStatus("Login trying...");
        var result = await auth.LoginAsync(request);

        if (result.IsSuccess)
        {
            this.SetStatus("Login successful.");

            App.Session = result.SessionDto;

            Window window = screenCode switch
            {
                ScreenCode.ReservationEquip => sp.GetRequiredService<ReservationEquipMonthlyWindow>(),
                ScreenCode.ReservationDaily => sp.GetRequiredService<ReservationDailyWindow>(),
                ScreenCode.OrganizationPatientSearch => sp.GetRequiredService<OrganizationPatientSearchWindow>(),
                ScreenCode.Organization => sp.GetRequiredService<OrganizationWindow>(),
                ScreenCode.Patient => sp.GetRequiredService<PatientWindow>(),
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
            var ex = new Exception(msg);
            this.SetError(ex, "Login failed.");
            return false;
        }
    }

    /// <summary>
    /// スタンドアローンで動かす場合には呼び出してください。
    /// EFCore は初回アクセス時にマッピングなどが行われるため時間がかかります。
    /// 必ず使うポリシーをロードしておきます。
    /// </summary>
    private async Task InitializeDatabaseAsync()
    {
        try
        {
            this.SetStatus("DB Initializing...");

            var auth = this.Host.Services.GetRequiredService<IAuthGrpcService>();
            await auth.PreloadAsync();

            this.SetStatus("DB initialized.");
        }
        catch (Exception ex)
        {
            this.SetError(ex, "DB initialization failed.");
        }
    }

    /// <summary>
    /// タスクトレイアイコンを作成します。
    /// </summary>
    private void InitializeNotifyIcon()
    {
        try
        {
            this.SetStatus("NotifyIcon Initializing...");

            this.Dispatcher.InvokeIfNeeded(() =>
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
            });

            this.SetStatus("NotifyIcon initialized.");
        }
        catch (Exception ex)
        {
            this.SetError(ex, "NotifyIcon initialization failed.");
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
            notifyIcon.DataContext = this.Host.Services.GetService<NotifyIconViewModel>();
            return notifyIcon;
        }

        throw new InvalidOperationException("Can Not CreateNotifyIcon.");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // デッドロックを回避しつつ同期的に待つ
        Task.Run(Task? () => App.TryLogoutAsync())
            .GetAwaiter()
            .GetResult();

        // アイコンのクリーンアップ
        if (this._notifyIcon is not null)
        {
            this._notifyIcon.Visibility = Visibility.Hidden;
            this._notifyIcon.Dispose();
            this._notifyIcon = null;
        }

        this.UnregisterUnhandledExceptionHandlers();

        Serilog.Log.CloseAndFlush();

        // 終了がキャンセルされないように念の為閉じる
        this._loginWindow?.ForceClose();
        this._logWindow?.ForceClose();

        base.OnExit(e);
    }
}
