using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Repositories;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockMcp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Aloe.Apps.MedockMcp.Extensions;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// MedockDbContext を DI コンテナに登録
    /// </summary>
    public static IServiceCollection AddMcpDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<MedockDbContext>(
            o => o.UseNpgsql(connectionString).UseLoggerFactory(NullLoggerFactory.Instance),
            ServiceLifetime.Scoped);

        services.AddDbContextFactory<MedockDbContext>(
            o => o.UseNpgsql(connectionString).UseLoggerFactory(NullLoggerFactory.Instance),
            ServiceLifetime.Scoped);

        return services;
    }

    /// <summary>
    /// コアサービス（認証・ユーザーコンテキスト等）を登録
    /// </summary>
    public static IServiceCollection AddMcpCoreServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, JstDateTimeProvider>();
        services.AddSingleton(_ => PasswordHasher.Default);
        services.Configure<CookieSettings>(configuration.GetSection("Cookie"));
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IUserContextService, McpUserContextService>();

        return services;
    }

    /// <summary>
    /// リポジトリを登録
    /// </summary>
    public static IServiceCollection AddMcpRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IAppointmentStatsRepository, AppointmentStatsRepository>();
        services.AddScoped<IHolidayRepository, HolidayRepository>();

        return services;
    }

    /// <summary>
    /// アプリケーションサービスを登録
    /// </summary>
    public static IServiceCollection AddMcpAppServices(this IServiceCollection services)
    {
        services.AddScoped<IAppointmentResourceAssignmentService, AppointmentResourceAssignmentService>();
        services.AddScoped<IAppointmentStatsUpdateService, AppointmentStatsUpdateService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IFacilityService, FacilityService>();
        services.AddScoped<IAppointmentFormService, AppointmentFormService>();

        return services;
    }
}
