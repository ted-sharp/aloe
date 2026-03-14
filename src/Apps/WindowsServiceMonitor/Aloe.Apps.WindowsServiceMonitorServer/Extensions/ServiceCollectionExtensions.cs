using Aloe.Apps.WindowsServiceMonitorLib.Infrastructure;
using Aloe.Apps.WindowsServiceMonitorLib.Interfaces;
using Aloe.Apps.WindowsServiceMonitorLib.Models;
using Aloe.Apps.WindowsServiceMonitorServer.Models;
using Aloe.Apps.WindowsServiceMonitorServer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

namespace Aloe.Apps.WindowsServiceMonitorServer.Extensions;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// 認証・認可関連サービスを登録
    /// </summary>
    public static IServiceCollection AddMonitorAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthOptions>(
            configuration.GetSection(AuthOptions.SectionName));
        var authOptions = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.AccessDeniedPath = "/access-denied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(authOptions.CookieSettings.ExpireTimeMinutes);
                options.SlidingExpiration = authOptions.CookieSettings.SlidingExpiration;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.Name = "WindowsServiceMonitor.Auth";
            });

        services.AddAuthorizationBuilder();
        services.AddHttpContextAccessor();
        services.AddCascadingAuthenticationState();
        services.AddScoped<LoginService>();
        services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

        return services;
    }

    /// <summary>
    /// WindowsServiceMonitor のコアサービスを登録
    /// </summary>
    public static IServiceCollection AddMonitorServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<WindowsServiceMonitorOptions>(
            configuration.GetSection(WindowsServiceMonitorOptions.SectionName));

        var monitoredServicesPath = Path.Combine(AppContext.BaseDirectory, "appsettings.services.json");
        services.AddScoped<IMonitoredServiceRepository>(
            sp => new JsonMonitoredServiceRepository(sp.GetRequiredService<ILogger<JsonMonitoredServiceRepository>>(), monitoredServicesPath));

        var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
        services.AddSingleton<ILogRepository>(
            sp => new JsonLogRepository(logsDir, sp.GetRequiredService<ILogger<JsonLogRepository>>()));

        services.AddSingleton<IOperationTracker, OperationTracker>();
        services.AddScoped<IWin32ServiceApi, Win32ServiceApi>();
        services.AddScoped<IServiceManager, ServiceManager>();
        services.AddScoped<IServiceRegistrar, ServiceRegistrar>();

        return services;
    }

    /// <summary>
    /// アプリケーションログ関連サービスを登録
    /// </summary>
    public static IServiceCollection AddMonitorAppLog(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<List<AppLogConfig>>(configuration.GetSection("AppLogs"));
        services.AddScoped<IAppLogService, AppLogService>();

        return services;
    }

    /// <summary>
    /// バックグラウンドサービスを登録
    /// </summary>
    public static IServiceCollection AddMonitorHostedServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddHostedService<BackgroundWindowsServiceMonitor>();

        return services;
    }
}
