using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Aloe.Medock.Reservation.AloeMedockResvLib.Logging;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Aloe.Medock.Reservation.AloeMedockResvServer;

// ここでは汎用的な設定を行うため、CA1859 は抑制します。
#pragma warning disable IDE0079 // 不要な抑制を削除する (IDE0079)
#pragma warning disable CA1859 // パフォーマンスの向上のために可能な場合は具象型を使用する

internal static class HostExtensions
{
    #region ConfigureSeeder

    /// <summary>
    /// 構成の追加を行います。
    /// </summary>
    internal static T ConfigureSeeder<T>(this T builder, bool isSqlLoggingEnabled, string connectionStringName)
        where T : IHostApplicationBuilder
    {
        builder
            .AddPostgreSql(isSqlLoggingEnabled, connectionStringName)
            .AddSeederServices()
            .AddSerilog();

        return builder;
    }

    /// <summary>
    /// PostgreSQL(EFCore) と関連クラスを追加します。
    /// </summary>
    private static IHostApplicationBuilder AddPostgreSql(this IHostApplicationBuilder builder,
        bool isSqlLoggingEnabled,
        string connectionStringName = "DefaultConnection")
    {
        // DateTime は EFCore 6.0 以降は with timezone にマッピングされるので、それを without timezone にします。
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var connStr = builder.Configuration.GetConnectionString(connectionStringName);
        ArgumentException.ThrowIfNullOrWhiteSpace(connStr, nameof(connStr));

        // EFCore はスレッドセーフではないので、ファクトリから都度生成します。
        builder.Services.AddDbContextFactory<AppDbContext>((services, options) =>
        {
            options.UseNpgsql(connStr);

            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
            }

            if (!isSqlLoggingEnabled)
            {
                // ログを出力しない
                options.UseLoggerFactory(NullLoggerFactory.Instance);
            }
        });

        return builder;
    }

    /// <summary>
    /// サーバー用サービスクラスを追加します。
    /// </summary>
    private static IHostApplicationBuilder AddSeederServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<Seeder>();

        return builder;
    }

    #endregion ConfigureSeeder

    #region ConfigureServer

    /// <summary>
    /// 構成の追加を行います。
    /// </summary>
    internal static T ConfigureServer<T>(this T builder, bool isSqlLoggingEnabled, string connectionStringName)
        where T : IHostApplicationBuilder
    {
        builder
            .AddPostgreSql(isSqlLoggingEnabled, connectionStringName)
            .AddHealthChecks()
            .AddServerServices()
            .AddDomainServices()
            .AddSerilog();

        return builder;
    }

    /// <summary>
    /// 正常性チェックサービスを追加します。
    /// </summary>
    private static IHostApplicationBuilder AddHealthChecks(this IHostApplicationBuilder builder)
    {
        // API として host.MapHealthChecks の登録が必要です。
        //builder.Services.AddHealthChecks();

        builder.Services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("db_context_check",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "postgres"]);

        return builder;
    }

    /// <summary>
    /// サーバー用サービスクラスを追加します。
    /// </summary>
    private static IHostApplicationBuilder AddServerServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();

        return builder;
    }

    /// <summary>
    /// ドメインサービスクラスを追加します。
    /// </summary>
    private static IHostApplicationBuilder AddDomainServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<IAuthService, AuthService>();
        builder.Services.AddTransient<IHolidayService, HolidayService>();

        builder.Services.AddScoped<IPermissionService, PermissionService>();
        builder.Services.AddTransient<IPolicyService, PolicyService>();
        builder.Services.AddTransient<IPreferenceService, PreferenceService>();

        builder.Services.AddTransient<IReservationDailyService, ReservationDailyService>();
        builder.Services.AddTransient<IReservationEquipmentService, ReservationEquipmentService>();
        return builder;
    }

    #region ConfigureServer / Kestrel

    internal static WebApplicationBuilder ConfigureKestrel(this WebApplicationBuilder builder)
    {
        builder
            .AddMagicOnionServer()
            .AddSwagger();

        builder.WebHost.ConfigureKestrel((context, options) =>
        {
            options.Configure(context.Configuration.GetSection("Kestrel"));
        });

        return builder;
    }

    /// <summary>
    /// gRPC と MagicOnion と関連クラスを追加します。
    /// </summary>
    /// <remarks>
    /// 多量のバイナリを扱う場合は StreamingHub で MemoryPack を使用することを検討します。
    /// </remarks>
    private static IHostApplicationBuilder AddMagicOnionServer(this IHostApplicationBuilder builder)
    {
        builder.Services.AddGrpc();
        builder.Services.AddMagicOnion(options =>
        {
            options.IsReturnExceptionStackTraceInErrorDetail = true;
        });

        // 各種サービス登録は不要で、Build() したあとに host.MapMagicOnionService(); を呼べばよい
        //builder.Services.AddScoped<IAuthGrpcService, AuthGrpcService>();

        return builder;
    }

    /// <summary>
    /// Swagger を追加します。
    /// </summary>
    private static IHostApplicationBuilder AddSwagger(this IHostApplicationBuilder builder)
    {
        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        return builder;
    }

    #endregion ConfigureServer / Kestrel

    #endregion ConfigureServer

    #region ConfigureServerApp

    /// <summary>
    /// ホストを設定します。
    /// </summary>
    internal static WebApplication ConfigureServerWebApp(this WebApplication host)
    {
        // Configure the HTTP request pipeline.
        if (host.Environment.IsDevelopment())
        {
            host.UseSwagger();
            host.UseSwaggerUI();

            host.UseDeveloperExceptionPage();
        }

        host.MapMagicOnionService();
        host.MapApi();

        return host;
    }

    private static WebApplication MapApi(this WebApplication host)
    {
        host.MapHealthChecks("/health");

        return host;
    }

    #endregion ConfigureServerApp
}
