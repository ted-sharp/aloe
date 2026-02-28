using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Repositories;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.VoiceCommand;
using Aloe.Apps.MedockServer.Components.Pages;
using Aloe.Apps.MedockServer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Aloe.Apps.MedockServer.Extensions;

/// <summary>
/// IServiceCollection の拡張メソッド群
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// MedockDbContext を DI コンテナに登録
    /// Factory と Scoped の両方を登録し、SQL ログ設定を適用
    /// </summary>
    public static IServiceCollection AddMedockDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var showSqlLogs = GetShowSqlLogsConfiguration(configuration);

        // DbContextFactory (Blazor Server では各操作で短命なコンテキストを生成)
        services.AddDbContextFactory<MedockDbContext>((_, options) =>
        {
            ConfigureDbContextOptions(options, connectionString, showSqlLogs, environment);
        }, ServiceLifetime.Scoped);

        // DbContext (リポジトリで使用)
        services.AddDbContext<MedockDbContext>((_, options) =>
        {
            ConfigureDbContextOptions(options, connectionString, showSqlLogs, environment);
        }, ServiceLifetime.Scoped);

        return services;
    }

    /// <summary>
    /// Cookie 認証を設定
    /// </summary>
    public static IServiceCollection AddMedockAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Cookie Settings
        services.Configure<CookieSettings>(configuration.GetSection(CookieSettings.SectionName));
        var cookieSettings = configuration.GetSection(CookieSettings.SectionName).Get<CookieSettings>() ?? new CookieSettings();

        // Authentication State Provider
        services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
        services.AddScoped<ProtectedLocalStorage>();

        // Cookie Authentication
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "MedockAuth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieSettings.ExpireTimeSpanMinutes ?? AuthenticationConstants.DefaultCookieExpireMinutes);
                options.SlidingExpiration = cookieSettings.SlidingExpiration ?? true;
                options.LoginPath = "/login";
                options.LogoutPath = "/api/auth/logout";
                options.AccessDeniedPath = "/login";
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// リポジトリを DI コンテナに登録
    /// </summary>
    public static IServiceCollection AddMedockRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IAppointmentStatsRepository, AppointmentStatsRepository>();
        services.AddScoped<IHolidayRepository, HolidayRepository>();

        return services;
    }

    /// <summary>
    /// アプリケーションサービスを DI コンテナに登録
    /// </summary>
    public static IServiceCollection AddMedockServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton(PasswordHasher.Default);
        services.AddSingleton<IDateTimeProvider, JstDateTimeProvider>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserContextService, UserContextService>();
        services.AddScoped<IAppointmentResourceAssignmentService, AppointmentResourceAssignmentService>();
        services.AddScoped<IAppointmentStatsUpdateService, AppointmentStatsUpdateService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IFacilityService, FacilityService>();
        services.AddScoped<ICalendarDataService, CalendarDataService>();
        services.AddScoped<IAppointmentStatsNotificationService, AppointmentStatsNotificationService>();
        services.AddScoped<IAppointmentFormService, AppointmentFormService>();

        services.Configure<AzureOpenAiSettings>(
            configuration.GetSection(AzureOpenAiSettings.SectionName));
        var azureSettings = configuration
            .GetSection(AzureOpenAiSettings.SectionName)
            .Get<AzureOpenAiSettings>() ?? new AzureOpenAiSettings();

        if (azureSettings.IsConfigured)
        {
            services.AddSingleton<VoiceCommandParser>();
            services.AddSingleton<IVoiceCommandParser, AzureOpenAiVoiceCommandParser>();
        }
        else
        {
            services.AddSingleton<IVoiceCommandParser, VoiceCommandParser>();
        }

        return services;
    }

    /// <summary>
    /// カレンダー関連サービスを DI コンテナに登録
    /// </summary>
    public static IServiceCollection AddMedockCalendarServices(this IServiceCollection services)
    {
        // Calendar State & Filter
        services.AddScoped<CalendarState>();
        services.AddScoped<ICalendarResourceFilterService, CalendarResourceFilterService>();
        services.AddScoped<Components.Pages.CalendarFilterService>();

        // Calendar Application Services (Loaders + Facade)
        services.AddScoped<ApplicationServices.Calendar.DataLoaders.IStatsLoader,
            ApplicationServices.Calendar.DataLoaders.StatsLoader>();
        services.AddScoped<ApplicationServices.Calendar.DataLoaders.IAppointmentLoader,
            ApplicationServices.Calendar.DataLoaders.AppointmentLoader>();
        services.AddScoped<ApplicationServices.Calendar.DataLoaders.IMetadataLoader,
            ApplicationServices.Calendar.DataLoaders.MetadataLoader>();
        services.AddScoped<ApplicationServices.Calendar.CalendarApplicationService>();

        // Calendar Orchestrator (Facade for reducing component dependencies)
        services.AddScoped<ApplicationServices.Calendar.ICalendarOrchestrator,
            ApplicationServices.Calendar.CalendarOrchestrator>();

        return services;
    }

    /// <summary>
    /// HttpClient を Blazor Server コンポーネントから API 呼び出しするために設定
    /// </summary>
    public static IServiceCollection AddMedockHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            var navigationManager = sp.GetRequiredService<NavigationManager>();

            httpClient.BaseAddress = new Uri(navigationManager.BaseUri);

            return httpClient;
        });

        return services;
    }

    /// <summary>
    /// SQL ログ表示設定を取得（appsettings.json または環境変数）
    /// </summary>
    private static bool GetShowSqlLogsConfiguration(IConfiguration configuration)
    {
        var showSqlLogs = configuration.GetValue<bool>("Features:ShowSqlLogs", false);

        // 環境変数で上書き可能（例: Features__ShowSqlLogs=true）
        var envValue = Environment.GetEnvironmentVariable("Features__ShowSqlLogs");
        if (!String.IsNullOrEmpty(envValue) && Boolean.TryParse(envValue, out var parsedValue))
        {
            showSqlLogs = parsedValue;
        }

        return showSqlLogs;
    }

    /// <summary>
    /// DbContextOptions を設定（Factory と Scoped で共通化）
    /// </summary>
    private static void ConfigureDbContextOptions(
        DbContextOptionsBuilder options,
        string? connectionString,
        bool showSqlLogs,
        IWebHostEnvironment environment)
    {
        options.UseNpgsql(connectionString);

        if (!showSqlLogs)
        {
            options.UseLoggerFactory(NullLoggerFactory.Instance);
        }
        else if (environment.IsDevelopment())
        {
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
        }
    }
}
