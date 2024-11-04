using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AloeReservationGrid.App.ReservationApp.ViewModels;
using AloeReservationGrid.App.ReservationApp.Views.Login;
using AloeReservationGrid.App.ReservationApp.Views.Maint;
using AloeReservationGrid.App.ReservationApp.Views.Resv;
using AloeReservationGrid.Lib.CoreLib.Logging;
using AloeReservationGrid.Lib.ReservationLib.Configuation;
using MagicOnion;
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
        var host = Host.CreateApplicationBuilder(args)
            .ConfigureBuilder()
            .Build();

        host.ConfigureHost()
            .Run();
    }

    /// <summary>
    /// 構成の追加を行います。
    /// </summary>
    private static HostApplicationBuilder ConfigureBuilder(this HostApplicationBuilder builder)
    {
        builder
            .AddSerilog()
            .AddServices();

        return builder;
    }

    /// <summary>
    /// Serilog を有効にします。
    /// </summary>
    private static IHostApplicationBuilder AddSerilog(this IHostApplicationBuilder builder)
    {
        var template = "APP [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (TID: {ThreadId}){NewLine}{Exception}";

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
    /// DIに必要なクラスを登録します。
    /// </summary>
    private static IHostApplicationBuilder AddServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHostedService<WpfHostService>();

        builder.Services.Configure<GrpcConfig>(builder.Configuration.GetSection("Client:Targets:gRPC"));

        builder.Services.AddSingleton<Application, App>();
        builder.Services.AddSingleton<NotifyIconViewModel>();

        builder.Services.AddTransient<LoginWindow>();
        builder.Services.AddTransient<ReservationMainWindow>();
        builder.Services.AddTransient<ReservationEquipWindow>();
        builder.Services.AddTransient<ReservationEquipBookingWindow>();
        builder.Services.AddTransient<ReservationDailyWindow>();
        builder.Services.AddTransient<ReservationDailyBookingWindow>();
        //builder.Services.AddTransient<OrganizationWindow>();
        //builder.Services.AddTransient<PatientWindow>();
        //builder.Services.AddTransient<OrganizationPatientSearchWindow>();
        builder.Services.AddTransient<MaintenanceWindow>();

        return builder;
    }

    /// <summary>
    /// ホストを設定します。
    /// </summary>
    private static IHost ConfigureHost(this IHost host)
    {
        host.ConfigureGlobalDebuggingLogger();
        return host;
    }
}
