using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Maint;
using System.Text.RegularExpressions;
using NetTopologySuite.Utilities;
using Serilog;
using System.Runtime.ExceptionServices;

namespace Aloe.Medock.Reservation.AloeMedockResvApp;

/// <remarks>
/// App.UnhandledException
/// </remarks>
public partial class App
{
    private void RegisterUnhandledExceptionHandlers()
    {
        this.SessionEnding += this.App_SessionEnding;

        AppDomain.CurrentDomain.FirstChanceException += this.CurrentDomain_FirstChanceException;
        AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += this.TaskScheduler_UnobservedTaskException;
        this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;
    }

    private void App_SessionEnding(object sender, SessionEndingCancelEventArgs e)
    {
        var actionName = (e.ReasonSessionEnding == ReasonSessionEnding.Shutdown) ? "シャットダウン"
            : (e.ReasonSessionEnding == ReasonSessionEnding.Logoff) ? "ログオフ"
            : "終了";

        this.LogInformation($"OSが{actionName}されました。");
    }

    private void UnregisterUnhandledExceptionHandlers()
    {
        AppDomain.CurrentDomain.FirstChanceException -= this.CurrentDomain_FirstChanceException;
        AppDomain.CurrentDomain.UnhandledException -= this.CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException -= this.TaskScheduler_UnobservedTaskException;
        this.DispatcherUnhandledException -= this.App_DispatcherUnhandledException;
    }

    // 例外を握りつぶしても検出できるのでデバッグ用
    private void CurrentDomain_FirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
    {
        this.LogFirstChanceException(e.Exception);
    }

    // 非UIスレッドでの未処理例外を補足する
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            this.LogError(ex);
            ErrorWindow.Show(ex);
        }
    }

    // タスクのGC時の未処理例外を補足する
    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        foreach (var ex in e.Exception.InnerExceptions)
        {
            this.LogError(ex);
        }

        ErrorWindow.Show(e.Exception);

        // 例外を「観察済み」に設定する
        e.SetObserved();
    }

    // WPFのUIスレッドでキャッチされていない例外を補足する
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        this.LogError(e.Exception);
        ErrorWindow.Show(e.Exception);

        // 例外が処理されたことを通知し、アプリケーションが強制終了しないようにする
        e.Handled = true;
    }
}
