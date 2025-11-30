using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Common.AloeCoreLib.Wpf.Extensions;
using Aloe.Medock.Reservation.AloeMedockResvApp.Configuration;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services;
using Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Cust;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Login;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Maint;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using Grpc.Net.Client;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
    private readonly AloeClientArgs _configArgs;

    private IHost? _host;

    private LoginWindow? _loginWindow;
    private LogWindow? _logWindow;

    public App(IConfigurationRoot config)
    {
        this._ts.Stamp("Ctor");

        this._config = config;

        var configArgs = config.BindSection<AloeClientArgs>();
        this._configArgs = configArgs;

        this.RegisterUnhandledExceptionHandlers();

        this.InitializeComponent();

        App.s_appInstance = this;

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
        var isEnabled = this._configArgs?.IsFirstChanceExceptionLogging ?? false;
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

            var userOptions = this._config.BindSection<UserOptions>();

            var isResident = this._configArgs.IsResident;

            // ログイン画面を表示するかどうか
            var isDefault = this._configArgs.ScreenCode.IsDefault();
            if (isDefault)
            {
                this._ts.Stamp("LoginWindow Creating...");
                // IHost 作成前から使う
                this._loginWindow = new LoginWindow(userOptions, isResident);
                Application.Current.MainWindow = this._loginWindow;

                this._loginWindow.Show();

                this._ts.Stamp("LoginWindow Created");
            }

            this._ts.Stamp("LogWindow Creating...");
            // IHost 初期化時にログを出力する RichTextBox を渡すために事前に作成しておき、
            // 閉じても非表示になるだけなので、以降インスタンスは維持される
            this._logWindow = new LogWindow();
            this._ts.Stamp("LogWindow Created");

            // 別スレッドで処理
            var task = Task.Run(async () =>
            {
                this._ts.Stamp("OnStartup Task.Run...");
                try
                {
                    this._host = this.InitializeHost(this._config, this._logWindow?.LogRichTextBox);
                    this._loginWindow?.InvokeCompleteInitHost("Host initialized.");

                    App.s_services = this.Host.Services;
                    this._logger = this.Host.Services.GetService<ILogger<App>>();
                    //var confRoot = this.Host.Services.GetService<IConfigurationRoot>();

                    if (this._configArgs.IsStandalone)
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
                if (isResident)
                {
                    // 常駐する場合はトレイアイコンを作成する
                    this.InitializeNotifyIcon();
                }

                // ロードが終わったらインジケーターを止める
                this._loginWindow?.InvokeCompleteInitTask("Startup finished.");

                this._ts.Stamp("OnStartup Task.Run finished");
                this._ts.DumpAsync();
            });

            if (!String.IsNullOrWhiteSpace(this._configArgs.User))
            {
                // 引数指定があるとき
                var isSuccessful = await this.LoginAsync(task,
                    this._configArgs.User,
                    this._configArgs.Password,
                    this._configArgs.ScreenCode);

                if (isSuccessful)
                {
                    this._ts.Stamp("OnStartup finished");
                    this._ts.DumpAsync();
                    return;
                }
            }
            else if (userOptions.IsReadyForAutoLogin)
            {
                // 自動ログインのとき
                var isSuccessful = await this.LoginAsync(task,
                    userOptions.User,
                    userOptions.Password);

                if (isSuccessful)
                {
                    this._ts.Stamp("OnStartup finished");
                    this._ts.DumpAsync();
                    return;
                }
            }
            else
            {
                this._ts.Stamp("OnStartup finished");

                //if (!isResident)
                //{
                //    // 画面指定があって開けなかったら終了
                //    this.Shutdown(0);
                //}
            }

            //await Task.WhenAll(task);
        }
        catch (Exception ex)
        {
            this.SetError(ex, "OnStartup failed.");
            this._ts.DumpAsync();
        }
        finally
        {
            // ダンプするタイミングは、タスクが終わったときとする
            //this._ts.DumpAsync();
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

        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();

        if (this._configArgs.IsStandalone)
        {
            builder.ConfigureStandalone(config, logTextBox);
        }
        else
        {
            builder.ConfigureClient(config, logTextBox);
        }

        var host = builder.Build();

        this.SetStatus("Host initialized.");

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
            App.User = result.UserDto;

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
                App.s_notifyIcon = this.CreateNotifyIcon();

                // Window なしでタスクトレイに表示
                App.s_notifyIcon.ForceCreate();

                // タスクトレイにあることを明示
                App.s_notifyIcon.ShowNotification(
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
        if (App.s_notifyIcon is not null)
        {
            App.s_notifyIcon.Visibility = Visibility.Hidden;
            App.s_notifyIcon.Dispose();
            App.s_notifyIcon = null;
        }

        this.UnregisterUnhandledExceptionHandlers();

        Serilog.Log.CloseAndFlush();

        // 終了がキャンセルされないように念の為閉じる
        this._loginWindow?.ForceClose();
        this._logWindow?.ForceClose();

        base.OnExit(e);
    }
}
