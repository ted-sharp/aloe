
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Net;
using System.Reflection.PortableExecutable;

namespace AloeReservationGrid.Api.ReservationServer;

internal static class Program
{
    internal static void Main(string[] args)
    {
        var host = WebApplication.CreateBuilder(args)
            .ConfigureBuilder()
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
            .AddMagicOnion();

        builder.WithoutTls();

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
    /// gRPC と MagicOnion を追加します。
    /// </summary>
    private static IHostApplicationBuilder AddMagicOnion(this IHostApplicationBuilder builder)
    {
        // TODO: バイナリをやり取りする場合は別ExeでMemoryPackの待ち受けを開始するのがよさそう
        builder.Services.AddGrpc();
        builder.Services.AddMagicOnion();
        return builder;
    }

    private static WebApplicationBuilder WithoutTls(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel((context, options) =>
        {
            var kestrelConfig = context.Configuration.GetSection("gRpcConfig");
            var ip = kestrelConfig.GetValue<string>("IPAddress");
            var addr = IPAddress.Parse(ip);
            var port = kestrelConfig.GetValue<int>("Port");

            // Setup a HTTP/2 endpoint without TLS.
            options.Listen(addr, port,
                o => o.Protocols = HttpProtocols.Http2);
        });
        return builder;
    }

    ///// <summary>
    ///// DIに必要なクラスを登録します。
    ///// </summary>
    //private static IHostApplicationBuilder AddServices(this IHostApplicationBuilder builder)
    //{
    //    //builder.Services.AddHostedService<WpfHostService>();
    //    //builder.Services.AddSingleton<Application, App>();
    //    //builder.Services.AddTransient<MainWindow>();
    //    return builder;
    //}

    #endregion ConfigureBuilder

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
