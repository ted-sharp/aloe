using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;
using AloeReservationGrid.App.ReservationApp.ViewModels;
using AloeReservationGrid.Lib.CoreLib.Logging;
using H.NotifyIcon;
using Microsoft.Extensions.Logging;

namespace AloeReservationGrid.App.ReservationApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly ILogger _logger;

    private TaskbarIcon? _notifyIcon;

    public App(
        ILogger<App> logger,
        MainWindow mainWindow)
    {
        this._logger = logger;
        this.MainWindow = mainWindow;

        this.MainWindow.Show();

        // グローバル例外処理のイベントハンドラを設定
        AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += this.TaskScheduler_UnobservedTaskException;
        this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;

    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var resourceDictionary = new ResourceDictionary
        {
            Source = new Uri("/Views/NotifyIconResources.xaml", UriKind.Relative),
        };

        // 必要なリソースを参照して使用する
        this._notifyIcon = (TaskbarIcon)resourceDictionary["NotifyIcon"];
        this._notifyIcon.DataContext = new NotifyIconViewModel();
        this._notifyIcon.ForceCreate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // アイコンのクリーンアップ
        this._notifyIcon?.Dispose();

        // グローバル例外処理のイベントハンドラを解除
        AppDomain.CurrentDomain.UnhandledException -= this.CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException -= this.TaskScheduler_UnobservedTaskException;
        this.DispatcherUnhandledException -= this.App_DispatcherUnhandledException;

        base.OnExit(e);
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
}

