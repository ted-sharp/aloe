using System.Management;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class MemoryChecker : IChecker
{
    public string SectionKey => "memory";

    public string SectionTitle => "Memory";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        using var searcher = new ManagementObjectSearcher(
            "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
        foreach (var obj in searcher.Get())
        {
            var totalKb = Convert.ToDouble(obj["TotalVisibleMemorySize"]);
            var freeKb = Convert.ToDouble(obj["FreePhysicalMemory"]);
            var usedKb = totalKb - freeKb;
            var usagePercent = totalKb > 0 ? usedKb / totalKb * 100.0 : 0;

            writer.WriteLine($"  {"Total",-20}: {FormatSize(totalKb)}");
            writer.WriteLine($"  {"Available",-20}: {FormatSize(freeKb)}");
            writer.WriteLine($"  {"Usage",-20}: {usagePercent:F1} %");
        }

        return Task.CompletedTask;
    }

    private static string FormatSize(double kilobytes)
    {
        var gb = kilobytes / 1024.0 / 1024.0;
        return $"{gb:F1} GB";
    }
}
