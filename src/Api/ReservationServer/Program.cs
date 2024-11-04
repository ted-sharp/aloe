
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Net;
using System.Reflection.PortableExecutable;
using AloeReservationGrid.Api.ReservationServer.Data.EFCore;
using AloeReservationGrid.Api.ReservationServer.Data.Repos;
using Microsoft.EntityFrameworkCore;
using AloeReservationGrid.Api.ReservationServer.Grpc.Services;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Services;
using AloeReservationGrid.Lib.CoreLib.Interfaces;
using AloeReservationGrid.Api.ReservationServer.Uuid;

namespace AloeReservationGrid.Api.ReservationServer;

internal static class Program
{
    internal static void Main(string[] args)
    {
        var host = WebApplication.CreateSlimBuilder(args)
            .ConfigureBuilder()
            .ConfigureKestrel()
            .Build();

        host.ConfigureApp()
            .Run();
    }

    #region ConfigureBuilder

    /// <summary>
    /// 構成の追加を行います。
    /// </summary>
    private static WebApplicationBuilder ConfigureBuilder(this WebApplicationBuilder builder)
    {
        builder
            .AddSerilog()
            .AddSwagger()
            .AddMagicOnion()
            .AddPostgreSql();

        return builder;
    }

    /// <summary>
    /// Serilog を有効にします。
    /// </summary>
    private static IHostApplicationBuilder AddSerilog(this IHostApplicationBuilder builder)
    {
        var template = "API [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (TID: {ThreadId}){NewLine}{Exception}";

        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.WithThreadId()
            .WriteTo.Debug(outputTemplate: template)
            .WriteTo.Console(theme: AnsiConsoleTheme.Literate, outputTemplate: template)
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog();

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
    /// gRPC と MagicOnion と関連クラスを追加します。
    /// </summary>
    /// <remarks>
    /// 多量のバイナリを扱う場合は StreamingHub で MemoryPack を使用することを検討します。
    /// </remarks>
    private static IHostApplicationBuilder AddMagicOnion(this IHostApplicationBuilder builder)
    {
        builder.Services.AddGrpc();
        builder.Services.AddMagicOnion();

        builder.Services.AddScoped<IAuthService, AuthService>();

        return builder;
    }

    /// <summary>
    /// PostgreSQL(EFCore) と関連クラスを追加します。
    /// </summary>
    private static IHostApplicationBuilder AddPostgreSql(this IHostApplicationBuilder builder)
    {
        var connStr = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connStr));

        builder.Services.AddSingleton<IUuidGenerator, PostgreSqlUuidGenerator>();

        builder.Services.AddScoped<IAuthService, AuthService>();

        return builder;
    }

    #endregion ConfigureBuilder

    #region ConfigureKestrel

    private static WebApplicationBuilder ConfigureKestrel(this WebApplicationBuilder builder)
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
    private static WebApplication ConfigureApp(this WebApplication host)
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
        public int TemperatureF => 32 + (int)(this.TemperatureC / 0.5556);
    }

    #endregion ConfigureApp
}
