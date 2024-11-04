using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;
using AloeReservationGrid.App.ReservationApp.ViewModels;
using AloeReservationGrid.App.ReservationApp.Views.Resv;
using AloeReservationGrid.Lib.CoreLib.Logging;
using CommunityToolkit.Diagnostics;
using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AloeReservationGrid.App.ReservationApp;

/// <summary>
/// Interaction logic for App.xaml
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

    public static object Resolve(Type type)
    {
        return App.Services.GetService(type)!;
    }

    #endregion static for ResolveExtension

    private readonly ILogger _logger;

    private TaskbarIcon? _notifyIcon;

    public App(
        IServiceProvider services,
        ILogger<App> logger,
        ReservationMainWindow mainWindow)
    {
        App.Services = services;
        this._logger = logger;

        this.RegisterUnhandledExceptionHandlers();

        this.InitializeMainWindow(mainWindow);
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

    private void InitializeMainWindow(ReservationMainWindow mainWindow)
    {
        this.MainWindow = mainWindow;

        // ウィンドウを閉じてもアプリが終了しないようにする
        this.MainWindow.Closing += (sender, e) =>
        {
            if (sender is Window w)
            {
                e.Cancel = true;
                w.Hide();
            }
        };

        // TODO: ログイン画面を表示して、ログインできたらメイン画面を表示する
        // あと各ページを表示するコマンドにログインしているかどうかの確認を含める？
        this.MainWindow.Show();

    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        this.InitializeNotifyIcon();
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
        }
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

