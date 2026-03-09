using Aloe.Apps.RazorReportLib.Models;
using Aloe.Apps.RazorReportServer.Services;

namespace Aloe.Apps.RazorReportServer.Extensions;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// RazorReport のサービスを登録
    /// </summary>
    public static IServiceCollection AddRazorReportServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<PageConfigService>();
        services.AddScoped<ZoomService>();
        services.AddScoped<DocumentService>();
        services.AddScoped<UndoRedoService>();
        services.AddScoped<ClipboardService>();
        services.AddSingleton<ReportStateService>();
        services.AddSingleton<SelectionService>();
        services.AddScoped<GridCalculationService>();
        services.AddScoped<AlignmentService>();
        services.AddScoped<FileInteropService>();
        services.AddScoped<PlacementModeService>();
        services.AddScoped<ComponentFactoryService>();

        services.Configure<ComponentsConfiguration>(
            configuration.GetSection("ComponentsConfiguration"));

        return services;
    }
}
