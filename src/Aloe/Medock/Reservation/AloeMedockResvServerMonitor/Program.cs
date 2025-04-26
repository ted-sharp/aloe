using Aloe.Common.AloeCoreLib.Util;
using Aloe.Common.AloeCoreLib.Win32;
using Aloe.Medock.Reservation.AloeMedockResvServerMonitor;
using Aloe.Medock.Reservation.AloeMedockResvServerMonitor.Configuration;

// 昇格されていなければ、ここで再起動
if (!ElevationHelper.EnsureRunAsAdministrator())
{
    const string message = "The application has been restarted with administrator privileges.";
    Console.WriteLine(message);
    return;
}

// 名前付き Mutex による単一起動制御
var appName = nameof(Aloe.Medock.Reservation.AloeMedockResvServerMonitor);
using var mutex = new Mutex(true, appName, out bool createdNew);
if (!createdNew)
{
    const string message = "The application is already running.";
    Console.WriteLine(message);
    MessageBox.Show(message, "Already Running", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    return;
}

var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

builder
    .BindSection<AloeMonitorOptions>()
    .AddSerilog();

builder.Services
    .AddSingleton<ServiceStatus>()
    .AddHostedService<MonitorBackgroundService>()
    .AddHostedService<TrayIconHostedService>();

var host = builder.Build();
host.Run();
