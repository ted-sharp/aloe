using Microsoft.Extensions.Options;
using System.ServiceProcess;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvServerMonitor.Configuration;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Aloe.Medock.Reservation.AloeMedockResvServerMonitor;

public class MonitorBackgroundService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly ServiceStatus _serviceStatus;
    private readonly IOptionsMonitor<AloeMonitorOptions> _options;

    public MonitorBackgroundService(
        ILogger<MonitorBackgroundService> logger,
        IOptionsMonitor<AloeMonitorOptions> options,
        ServiceStatus serviceStatus)
    {
        this._logger = logger;
        this._options = options;
        this._serviceStatus = serviceStatus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                this.RefreshStatus();

                var interval = this._options.CurrentValue.MonitoringInterval;
                await Task.Delay(interval, stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            // キャンセルなので何もしない
        }
    }

    /// <summary>
    /// サービスの登録状態および実行状態をチェックしてステータスを更新します。
    /// </summary>
    private void RefreshStatus()
    {
        var serviceName = this._options.CurrentValue.WindowsServiceName;
        var existingService = ServiceController.GetServices()
            .FirstOrDefault(s => s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

        if (existingService is null)
        {
            var serviceFullPath = this._options.CurrentValue.GetWindowsServiceFullPath();
            if (!File.Exists(serviceFullPath))
            {
                // ファイルが存在しなければ専用のステータスとする
                this._serviceStatus.SetNotFound();
                this._logger.LogInformation($"Status: NotFound at {DateTimeOffset.Now} [{serviceFullPath}]");
                return;
            }
        }

        var state = existingService?.Status;
        this._serviceStatus.SetState(state);

        if (this._logger.IsEnabled(LogLevel.Information))
        {
            var status = this._serviceStatus.StateText;
            this._logger.LogInformation($"Status: {status} at {DateTimeOffset.Now}");
        }

    }
}
