using Aloe.Apps.DashboardLib.OtelViewer.Models;
using Aloe.Apps.DashboardLib.OtelViewer.Services;
using Aloe.Apps.DashboardLib.OtelViewer.Storage;

namespace Aloe.Apps.DashboardServer.Extensions;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// OTel ビューアー関連サービスを登録
    /// </summary>
    public static IServiceCollection AddOtelViewer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OtelViewerOptions>(
            configuration.GetSection(OtelViewerOptions.SectionName));

        services.AddSingleton<IOtelStore, InMemoryOtelStore>();
        services.AddSingleton<IOtelIngestionService, OtelIngestionService>();

        return services;
    }
}
