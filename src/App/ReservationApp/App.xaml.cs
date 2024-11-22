using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;
using AloeReservationGrid.App.ReservationApp.ViewModels;
using AloeReservationGrid.App.ReservationApp.Views.Login;
using AloeReservationGrid.App.ReservationApp.Views.Resv;
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

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        this.ShowDevelWindow();
        //this.InitializeNotifyIcon();
        //this.ShowFirstLoginWindow();
    }

    private void ShowDevelWindow()
    {
        var window = App.CreateWindow<ReservationEquipWindow>();
        //var window = App.CreateWindow<ReservationMainWindow>();

        window.ActivateOrShow();
        Application.Current.MainWindow = window;
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
            this._notifyIcon.DataContext = App.Resolve<NotifyIconViewModel>();
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

    protected override void OnExit(ExitEventArgs e)
    {
        // アイコンのクリーンアップ
        this._notifyIcon?.Dispose();
        this._notifyIcon = null;

        this.UnregisterUnhandledExceptionHandlers();

        base.OnExit(e);
    }

}

