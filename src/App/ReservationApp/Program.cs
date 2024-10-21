using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AloeReservationGrid.Lib.CoreLib.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace AloeReservationGrid.App.ReservationApp;

internal static class Program
{
    [STAThread]
    internal static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .RegisterService()
            .Build();

        host.ConfigureGlobalDebuggingLogger();

        host.Run();
    }

    /// <summary>
    /// DIに必要なクラスを登録します。
    /// </summary>
    private static IHostBuilder RegisterService(this IHostBuilder host)
    {
        host.ConfigureServices(services =>
        {
            services.AddHostedService<WpfHostService>();
            services.AddSingleton<Application, App>();

            services.AddTransient<MainWindow>();
        });
        return host;
    }

    private static IHostBuilder UseSerilog(this IHostBuilder host)
    {
        var template = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (TID: {ThreadId}){NewLine}{Exception}";
        host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .WriteTo.Debug(outputTemplate: template)
                .WriteTo.Console(
                    theme: AnsiConsoleTheme.Literate,
                    outputTemplate: template);
        });

        return host;
    }

}
