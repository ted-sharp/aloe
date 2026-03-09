using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class SystemInfoChecker : IChecker
{
    public string SectionKey => "system";

    public string SectionTitle => "System Information";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        writer.WriteLine($"  {"OS",-20}: {RuntimeInformation.OSDescription}");
        writer.WriteLine($"  {"Computer Name",-20}: {Environment.MachineName}");
        writer.WriteLine($"  {"Architecture",-20}: {RuntimeInformation.OSArchitecture}");

        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        writer.WriteLine($"  {"Uptime",-20}: {FormatUptime(uptime)}");

        var tz = TimeZoneInfo.Local;
        writer.WriteLine($"  {"Timezone",-20}: {tz.DisplayName}");
        writer.WriteLine($"  {"Culture",-20}: {Thread.CurrentThread.CurrentCulture.Name}");
        writer.WriteLine($"  {"User",-20}: {Environment.UserDomainName}\\{Environment.UserName}");

        var isAdmin = IsAdministrator();
        writer.WriteLine($"  {"Is Admin",-20}: {(isAdmin ? "Yes" : "No")}");

        return Task.CompletedTask;
    }

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static string FormatUptime(TimeSpan ts) =>
        ts.Days > 0
            ? $"{ts.Days} days {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}"
            : $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
}
