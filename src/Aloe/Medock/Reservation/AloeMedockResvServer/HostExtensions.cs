using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Aloe.Medock.Reservation.AloeMedockResvLib.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvServer;

// ここでは汎用的な設定を行うため、CA1859 は抑制します。
#pragma warning disable IDE0079 // 不要な抑制を削除する (IDE0079)
#pragma warning disable CA1859 // パフォーマンスの向上のために可能な場合は具象型を使用する

internal static class HostExtensions
{

    #region ConfigureBuilder

    /// <summary>
    /// 構成の追加を行います。
    /// </summary>
    internal static WebApplicationBuilder ConfigureBuilder(this WebApplicationBuilder builder, Arguments arguments)
    {
        builder
            .AddPostgreSql()
            .AddServerServices()
            .AddDomainServices()
            .AddMagicOnionServer();

        if (arguments.IsSilent)
        {
            builder.Logging.ClearProviders();
            //// コンソール出力を無効化
            //Console.SetOut(TextWriter.Null);
        }
        else
        {
            builder.AddSerilog()
                .AddSwagger();
        }

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

    /// <summary>
    /// PostgreSQL(EFCore) と関連クラスを追加します。
    /// </summary>
    private static IHostApplicationBuilder AddPostgreSql(this IHostApplicationBuilder builder)
    {
        // DateTime は EFCore 6.0 以降は with timezone にマッピングされるので、それを without timezone にします。
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var connStr = builder.Configuration.GetConnectionString("DefaultConnection");

        // EFCore はスレッドセーフではないので、ファクトリから都度生成します。
        builder.Services.AddDbContextFactory<AppDbContext>((services, options) =>
        {
            options.UseNpgsql(connStr);

            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
            }
        });

        return builder;
    }

    /// <summary>
    /// サーバー用サービスクラスを追加します。
    /// </summary>
    private static IHostApplicationBuilder AddServerServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<Seeder>();

        return builder;
    }

    /// <summary>
    /// ドメインサービスクラスを追加します。
    /// </summary>
    private static IHostApplicationBuilder AddDomainServices(this IHostApplicationBuilder builder)
    {
        //builder.Services.AddSingleton<IUuidGenerator, UuidGeneratorFriendlyPostgreSql>();

        builder.Services.AddScoped<IPolicyService, PolicyService>();

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
        //builder.Services.AddMagicOnion();
        builder.Services.AddMagicOnion(options =>
        {
            options.IsReturnExceptionStackTraceInErrorDetail = true;
        });

        // 各種サービス登録は不要で、Build() したあとに host.MapMagicOnionService(); を呼べばよい
        //builder.Services.AddScoped<IAuthGrpcService, AuthGrpcService>();

        return builder;
    }

    #endregion ConfigureBuilder

    #region ConfigureKestrel

    internal static WebApplicationBuilder ConfigureKestrel(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel((context, options) =>
        {
            options.Configure(context.Configuration.GetSection("Kestrel"));
        });
        return builder;
    }

    #endregion ConfigureKestrel

    #region ConfigureApp

    /// <summary>
    /// ホストを設定します。
    /// </summary>
    internal static WebApplication ConfigureApp(this WebApplication host)
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

        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        host.MapGet("/weatherforecast", () =>
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    (
                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        Random.Shared.Next(-20, 55),
                        summaries[Random.Shared.Next(summaries.Length)]
                    ))
                .ToArray();
            return forecast;
        })
            .WithName("GetWeatherForecast")
            .WithOpenApi();

        return host;
    }

    internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        //public int TemperatureF => 32 + (int)(this.TemperatureC / 0.5556);
    }

    #endregion ConfigureApp
}
