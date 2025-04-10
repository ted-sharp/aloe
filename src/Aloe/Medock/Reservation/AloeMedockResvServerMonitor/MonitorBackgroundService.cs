using Aloe.Medock.Reservation.AloeMedockResvServerMonitor.Settings;
using Microsoft.Extensions.Options;
using System.ServiceProcess;

namespace Aloe.Medock.Reservation.AloeMedockResvServerMonitor;

public class MonitorBackgroundService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly ServiceStatus _serviceStatus;
    private readonly AloeMonitorSettings _settings;

    public MonitorBackgroundService(
        ILogger<MonitorBackgroundService> logger,
        IOptions<AloeMonitorSettings> options,
        ServiceStatus serviceStatus)
    {
        this._logger = logger;
        this._settings = options.Value;
        this._serviceStatus = serviceStatus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                this.RefreshStatus();

                await Task.Delay(this._settings.MonitoringInterval, stoppingToken);
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
        var serviceName = this._settings.WindowsServiceName;
        var existingService = ServiceController.GetServices()
            .FirstOrDefault(s => s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

        var state = existingService?.Status;
        this._serviceStatus.SetState(state);

        if (this._logger.IsEnabled(LogLevel.Information))
        {
            var status = this._serviceStatus.StateText;
            this._logger.LogInformation($"Status: {status} at {DateTimeOffset.Now}");
        }

    }
}
