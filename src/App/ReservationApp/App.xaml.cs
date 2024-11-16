using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;
using AloeReservationGrid.App.ReservationApp.ViewModels;
using AloeReservationGrid.App.ReservationApp.Views.Login;
using AloeReservationGrid.App.ReservationApp.Views.Resv;
using AloeReservationGrid.Lib.CoreLib.Logging;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using CommunityToolkit.Diagnostics;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AloeReservationGrid.App.ReservationApp;

/// <summary>
/// アプリケーション本体の処理の開始点です。
/// Program.Main → WpfHostService.StartAsync → App と呼び出されます。
/// </summary>
public partial class App : Application
{
    #region static for ResolveExtension

    private static IServiceProvider? s_services;

    public static IServiceProvider Services
    {
        get => App.s_services ?? throw new InvalidOperationException("Service provider is not initialized.");
        private set => App.s_services = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// <see cref="ResolveExtension"/> 用です。
    /// それ以外は、<see cref="Resolve{T}"/> を使います。
    /// </summary>
    public static object? Resolve(Type type)
    {
        return App.Services.GetService(type);
    }

    public static T? Resolve<T>()
    {
        return App.Services.GetService<T>();
    }

    #endregion static for ResolveExtension

    #region Global
    public static SessionDto? Session { get; set; }

    public static bool HasSession => App.Session != null;

    public static T CreateWindow<T>()
        where T : Window
    {
        var type = typeof(T);

        var window = App.Resolve<T>();

        return window ?? throw new Exception($"Not Found Window. (Type: {type})");
    }

    public static T? GetWindow<T>()
        where T : Window
    {
        var window = Application.Current.Windows
            .OfType<T>()
            .FirstOrDefault();

        return window;
    }

    public static T GetOrCreateWindow<T>()
        where T : Window
    {
        var window = App.GetWindow<T>()
            ?? App.CreateWindow<T>();

        return window;
    }

    #endregion Global

    private readonly ILogger _logger;

    private TaskbarIcon? _notifyIcon;

    public App(
        IServiceProvider services,
        ILogger<App> logger)
    {
        App.Services = services;
        this._logger = logger;

        this.RegisterUnhandledExceptionHandlers();
    }

    #region UnhandledException

    private void RegisterUnhandledExceptionHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += this.TaskScheduler_UnobservedTaskException;
        this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;
    }

    private void UnregisterUnhandledExceptionHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException -= this.CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException -= this.TaskScheduler_UnobservedTaskException;
        this.DispatcherUnhandledException -= this.App_DispatcherUnhandledException;
    }

    // 非UIスレッドでの未処理例外を補足する
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            this._logger.Error(ex, ex.ToString());
        }
    }

    // タスクのGC時の未処理例外を補足する
    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        var aggregateException = e.Exception;

        // 内部の例外をすべて展開してログに記録
        foreach (var ex in aggregateException.InnerExceptions)
        {
            this._logger.Error(ex, ex.ToString());
        }

        // 例外を「観察済み」に設定する
        e.SetObserved();
    }

    // WPFのUIスレッドでキャッチされていない例外を補足する
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // 例外をログに記録する
        var ex = e.Exception;
        this._logger.Error(ex, ex.ToString());

        MessageBox.Show("予期しないエラーが発生しました: " + ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);

        // 例外が処理されたことを通知し、アプリケーションが強制終了しないようにする
        e.Handled = true;
    }

    #endregion UnhandledException

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        this.InitializeNotifyIcon();
        this.ShowFirstLoginWindow();
        this.ShowDevelWindow();
    }

    private void InitializeNotifyIcon()
    {
        var resources = new ResourceDictionary
        {
            Source = new Uri("/Views/Tray/NotifyIconResources.xaml", UriKind.Relative),
        };

        // 必要なリソースを参照して使用する
        if (resources["NotifyIcon"] is TaskbarIcon notifyIcon)
        {
            this._notifyIcon = notifyIcon;
            this._notifyIcon.ForceCreate();
            this._notifyIcon.ShowNotification(
                "予約システム",
                "予約システムが起動しました。\n常駐しますので、タスクトレイから操作してください。",
                NotificationIcon.Info,
                largeIcon: true,
                sound: false);
        }
    }

    private void ShowFirstLoginWindow()
    {
        var window = App.CreateWindow<LoginWindow>()
                     ?? throw new Exception($"Can't Create LoginWindow");

        window.ActivateOrShow();
        Application.Current.MainWindow = window;
    }

    private void ShowDevelWindow()
    {
        var window = App.CreateWindow<ReservationMainWindow>()
                     ?? throw new Exception($"Can't Create Window");

        window.ActivateOrShow();
        Application.Current.MainWindow = window;
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

