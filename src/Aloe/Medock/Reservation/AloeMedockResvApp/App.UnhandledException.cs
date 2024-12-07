using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;

namespace Aloe.Medock.Reservation.AloeMedockResvApp;

public partial class App
{

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
            if (this._logger is null)
            {
                Debug.WriteLine(ex.ToString());
            }
            else
            {
                this._logger.LogError(ex, ex.ToString());
            }
        }
    }

    // タスクのGC時の未処理例外を補足する
    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        var aggregateException = e.Exception;

        // 内部の例外をすべて展開してログに記録
        foreach (var ex in aggregateException.InnerExceptions)
        {
            if (this._logger is null)
            {
                Debug.WriteLine(ex.ToString());
            }
            else
            {
                this._logger.LogError(ex, ex.ToString());
            }
        }

        // 例外を「観察済み」に設定する
        e.SetObserved();
    }

    // WPFのUIスレッドでキャッチされていない例外を補足する
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // 例外をログに記録する
        var ex = e.Exception;

        if (this._logger is null)
        {
            Debug.WriteLine(ex.ToString());
        }
        else
        {
            this._logger.LogError(ex, ex.ToString());
        }

        MessageBox.Show("予期しないエラーが発生しました: " + ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);

        // 例外が処理されたことを通知し、アプリケーションが強制終了しないようにする
        e.Handled = true;
    }

    #endregion UnhandledException
}
