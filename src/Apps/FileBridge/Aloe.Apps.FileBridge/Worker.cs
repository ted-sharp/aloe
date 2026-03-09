using Aloe.Apps.FileBridgeLib.Services;

namespace Aloe.Apps.FileBridge;

public class Worker(
    FileWatcherService fileWatcherService,
    ProcessLauncherService processLauncherService,
    OperationLogService operationLogService,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        fileWatcherService.Start();
        logger.LogInformation("ファイル監視を開始しました");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("サービスを停止しています...");

        fileWatcherService.Stop();
        processLauncherService.Dispose();
        operationLogService.Dispose();

        return base.StopAsync(cancellationToken);
    }
}
