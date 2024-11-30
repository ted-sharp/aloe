using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using AloeReservationGrid.App.ReservationApp.Services;
using AloeReservationGrid.App.ReservationApp.ViewModels;
using AloeReservationGrid.App.ReservationApp.Views.Login;
using AloeReservationGrid.App.ReservationApp.Views.Resv;
using AloeReservationGrid.Lib.CoreLib.Util;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Services;
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
    private readonly ISeedGrpcService _seedGrpcService;
    private readonly WindowService _windowService;

    private TaskbarIcon? _notifyIcon;

    public App(
        IServiceProvider services,
        ILogger<App> logger,
        ISeedGrpcService seedGrpcService,
        WindowService windowService)
    {
        App.Services = services;
        this._logger = logger;
        this._seedGrpcService = seedGrpcService;
        this._windowService = windowService;

        this.RegisterUnhandledExceptionHandlers();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        this.ShowDevelWindow();
        //this.InitializeNotifyIcon();
        //this.ShowFirstLoginWindow();

        this._seedGrpcService.SeedAsync();
    }

    private void ShowDevelWindow()
    {
        //var window = this._windowService.CreateWindow<ReservationMainWindow>();
        var window = this._windowService.CreateWindow<ReservationEquipWindow>();

        window.ActivateOrShow();
        Application.Current.MainWindow = window;
    }

    private void InitializeNotifyIcon()
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

    private TaskbarIcon CreateNotifyIcon()
    {
        var resources = new ResourceDictionary
        {
            Source = new Uri("/Views/Tray/NotifyIconResources.xaml", UriKind.Relative),
        };

        // 必要なリソースを参照して使用する
        if (resources["NotifyIcon"] is TaskbarIcon notifyIcon)
        {
            notifyIcon.DataContext = App.Resolve<NotifyIconViewModel>();
            return notifyIcon;
        }

        throw new InvalidOperationException("Can Not CreateNotifyIcon.");
    }

    private void ShowFirstLoginWindow()
    {
        var window = this._windowService.CreateWindow<LoginWindow>()
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

