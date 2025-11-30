using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.ServiceProcess;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Common.AloeCoreLib.Win32;
using Aloe.Medock.Reservation.AloeMedockResvServerMonitor.Assets;
using Aloe.Medock.Reservation.AloeMedockResvServerMonitor.Configuration;

namespace Aloe.Medock.Reservation.AloeMedockResvServerMonitor;

public class TrayIconHostedService : IHostedService
{
    private readonly ILogger _logger;
    private NotifyIcon? _notifyIcon;
    private readonly IOptionsMonitor<AloeMonitorOptions> _options;
    private readonly IHostApplicationLifetime _appLifetime;
    private Thread? _uiThread;
    private readonly ServiceStatus _serviceStatus;
    private ToolStripMenuItem? _statusMenuItem;
    private ToolStripMenuItem? _registerMenuItem;
    private ToolStripMenuItem? _startStopMenuItem;
    private ToolStripMenuItem? _restartMenuItem;

    public TrayIconHostedService(
        ILogger<TrayIconHostedService> logger,
        IOptionsMonitor<AloeMonitorOptions> options,
        IHostApplicationLifetime appLifetime,
        ServiceStatus serviceStatus)
    {
        this._logger = logger;
        this._options = options;
        this._appLifetime = appLifetime;
        this._serviceStatus = serviceStatus;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var options = this._options.CurrentValue;

        // STAスレッドを作成してUIコンポーネントの生成とメッセージループを実行
        this._uiThread = new Thread(() =>
        {
            // コンテキストメニューの生成
            var contextMenu = new ContextMenuStrip();

            // サービス状態を表示するメニュー項目を作成
            var statusMenuItem = new ToolStripMenuItem("状態: " + this._serviceStatus.StateText);
            this._statusMenuItem = statusMenuItem;
            contextMenu.Items.Add(statusMenuItem);

            // "サービス フォルダを開く"
            var servicePath = options.GetWindowsServiceFullPath();
            var openServiceMenuItem = new ToolStripMenuItem("サービス フォルダを開く", null, (sender, e) => this.OpenExeFolder(servicePath));
            openServiceMenuItem.Image = Images.FolderOpen.Value;
            contextMenu.Items.Add(openServiceMenuItem);

            // "サービス登録/サービス登録解除"
            var serviceManagerMenuItem = new ToolStripMenuItem("サービス管理ツール", null, (sender, e) => this.OpenServicesMsc());
            serviceManagerMenuItem.Image = Images.Settings.Value;
            contextMenu.Items.Add(serviceManagerMenuItem);

            // セパレーター
            contextMenu.Items.Add(new ToolStripSeparator());

            // "サービス登録/サービス登録解除"
            var registerMenuItem = new ToolStripMenuItem("サービス登録/サービス登録解除", null, (sender, e) => this.ToggleServiceRegistration());
            this._registerMenuItem = registerMenuItem;
            contextMenu.Items.Add(registerMenuItem);

            // "サービス起動/サービス停止"
            var startStopMenuItem = new ToolStripMenuItem("サービス起動/サービス停止", null, (sender, e) => this.ToggleServiceStartStop());
            this._startStopMenuItem = startStopMenuItem;
            contextMenu.Items.Add(startStopMenuItem);

            // "サービス再起動"
            var restartMenuItem = new ToolStripMenuItem("サービス再起動", null, (sender, e) => this.RestartService());
            restartMenuItem.Image = Images.Restart.Value;
            this._restartMenuItem = restartMenuItem;
            contextMenu.Items.Add(restartMenuItem);

            // セパレーター
            contextMenu.Items.Add(new ToolStripSeparator());

            // "フォルダを開く"
            var exePath = Application.ExecutablePath;
            var openExeMenuItem = new ToolStripMenuItem("フォルダを開く", null, (sender, e) => this.OpenExeFolder(exePath));
            openExeMenuItem.Image = Images.FolderOpen.Value;
            contextMenu.Items.Add(openExeMenuItem);

            // TODO: "ログを開く"

            // "終了"
            var exitMenuItem = new ToolStripMenuItem("終了", null, (sender, e) => this.ExitApplication());
            exitMenuItem.Image = Images.Logout.Value;
            contextMenu.Items.Add(exitMenuItem);

            // NotifyIcon の生成
            this._notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = nameof(AloeMedockResvServerMonitor),
                Visible = true,
                ContextMenuStrip = contextMenu,
            };

            // UI 更新用タイマー（UIスレッド上で動作）
            var timer = new System.Windows.Forms.Timer();
            var interval = options.MonitoringInterval;
            timer.Interval = interval;
            timer.Tick += (sender, e) =>
            {
                this.RefreshStatus();
            };
            timer.Start();

            this.RefreshStatus();

            // メッセージループを開始
            Application.Run();
        });

        // STA に設定
        this._uiThread.SetApartmentState(ApartmentState.STA);
        this._uiThread.Start();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (this._notifyIcon is not null)
        {
            this._notifyIcon.Visible = false;
            this._notifyIcon.Dispose();
            this._notifyIcon = null;
        }

        Serilog.Log.CloseAndFlush();

        return Task.CompletedTask;
    }

    private void SetStatus(ServiceControllerStatus? state)
    {
        this._serviceStatus.SetState(state);
        this.RefreshStatus();
    }

    private void RefreshStatus()
    {
        if (this._notifyIcon is not null)
        {
            this._notifyIcon.Icon = this._serviceStatus.Icon;
        }

        if (this._statusMenuItem is not null)
        {
            this._statusMenuItem.Text = "状態: " + this._serviceStatus.StateText;
            this._statusMenuItem.Image = this._serviceStatus.Image;
        }

        if (this._registerMenuItem is not null)
        {
            this._registerMenuItem.Enabled = this._serviceStatus.CanRegisterUnregister;
            this._registerMenuItem.Text = this._serviceStatus.RegisterMenuText;
            this._registerMenuItem.Image = this._serviceStatus.RegisterMenuImage;
        }

        if (this._startStopMenuItem is not null)
        {
            this._startStopMenuItem.Enabled = this._serviceStatus.CanStartStop;
            this._startStopMenuItem.Text = this._serviceStatus.StartStopMenuText;
            this._startStopMenuItem.Image = this._serviceStatus.StartStopMenuImage;
        }

        if (this._restartMenuItem is not null)
        {
            this._restartMenuItem.Enabled = this._serviceStatus.CanStartStop;
        }
    }

    private void OpenServicesMsc()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "services.msc",
            UseShellExecute = true,
        });
    }

    private void ToggleServiceRegistration()
    {
        var options = this._options.CurrentValue;
        var serviceName = options.WindowsServiceName;

        // サービス一覧から対象サービスが存在するかチェック
        var existingService = ServiceController.GetServices()
            .FirstOrDefault(s => s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

        if (existingService == null)
        {
            var servicePath = options.WindowsServicePath;
            var serviceDescription = options.WindowsServiceDescription;
            var serviceStartType = options.WindowsServiceStartType;
            var serviceAccount = options.WindowsServiceAccount;
            var serviceDependencies = options.WindowsServiceDependencies;
            var serviceResets = options.WindowsServiceFailureResets;
            var serviceActions = options.WindowsServiceFailureActions;

            // サービス登録（sc.exe を利用）
            var isSuccessful = Sc.CreateService(
                serviceName,
                servicePath,
                serviceDescription,
                serviceStartType,
                serviceAccount,
                serviceDependencies,
                serviceResets,
                serviceActions,
                this._logger);
            if (isSuccessful)
            {
                this.SetStatus(ServiceControllerStatus.Stopped);
            }
            else
            {
                this.SetStatus(null);
            }
        }
        else
        {
            // サービス登録解除
            var isSuccessful = Sc.DeleteService(
                serviceName,
                this._logger);
            if (isSuccessful)
            {
                this.SetStatus(null);
            }
            else
            {
                // 失敗したらステータスはそのままでよい
                //this.SetStatus(ServiceControllerStatus.Stopped);
            }
        }
    }

    private void ToggleServiceStartStop()
    {
        var serviceName = this._options.CurrentValue.WindowsServiceName;
        try
        {
            using var service = new ServiceController(serviceName);
            if (service.Status == ServiceControllerStatus.Running)
            {
                this.SetStatus(ServiceControllerStatus.StopPending);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                this.SetStatus(ServiceControllerStatus.Stopped);
            }
            else if (service.Status == ServiceControllerStatus.Stopped)
            {
                this.SetStatus(ServiceControllerStatus.StartPending);
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                this.SetStatus(ServiceControllerStatus.Running);
            }
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "開始/停止時に例外が発生しました。");
        }
    }

    private void RestartService()
    {
        var serviceName = this._options.CurrentValue.WindowsServiceName;
        try
        {
            using var service = new ServiceController(serviceName);
            if (service.Status == ServiceControllerStatus.Running)
            {
                this.SetStatus(ServiceControllerStatus.StopPending);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                this.SetStatus(ServiceControllerStatus.Stopped);
            }

            this.SetStatus(ServiceControllerStatus.StartPending);
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
            this.SetStatus(ServiceControllerStatus.Running);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "再起動時に例外が発生しました。");
        }
    }

    private void OpenExeFolder(string exePath)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{exePath}\"",
            UseShellExecute = true,
        });
    }

    private void ExitApplication()
    {
        this._appLifetime.StopApplication();
        Application.ExitThread();
    }
}
