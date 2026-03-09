using Aloe.Apps.FileBridgeLib.Models;
using Aloe.Apps.FileBridgeLib.Services;

namespace Aloe.Apps.FileBridge.Extensions;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// FileBridge のサービスを登録
    /// </summary>
    public static IServiceCollection AddFileBridgeServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var fileBridgeOptions = configuration.GetSection("FileBridge").Get<FileBridgeOptions>() ?? new FileBridgeOptions();
        services.Configure<FileBridgeOptions>(configuration.GetSection("FileBridge"));

        services.AddSingleton<OperationLogService>(
            sp => new OperationLogService(fileBridgeOptions));

        services.AddSingleton<ProcessLauncherService>(sp =>
        {
            var logService = sp.GetRequiredService<OperationLogService>();
            var logger = sp.GetService<ILogger<ProcessLauncherService>>();
            return new ProcessLauncherService(fileBridgeOptions, logService, logger);
        });

        services.AddSingleton<FileWatcherService>(sp =>
        {
            var logService = sp.GetRequiredService<OperationLogService>();
            var processLauncher = sp.GetRequiredService<ProcessLauncherService>();
            var logger = sp.GetService<ILogger<FileWatcherService>>();
            return new FileWatcherService(fileBridgeOptions, logService, processLauncher, logger);
        });

        services.AddHostedService<Worker>();

        return services;
    }
}
