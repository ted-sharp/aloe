using System.Management;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class GpuChecker : IChecker
{
    public string SectionKey => "gpu";

    public string SectionTitle => "GPU";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Name, DriverVersion, DriverDate, AdapterRAM, VideoProcessor, "
                + "CurrentHorizontalResolution, CurrentVerticalResolution, Status "
                + "FROM Win32_VideoController");

            foreach (var obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString() ?? "Unknown";
                writer.WriteLine($"  {name}");

                var driverVersion = obj["DriverVersion"]?.ToString() ?? "N/A";
                writer.WriteLine($"    {"Driver Version",-16}: {driverVersion}");

                var driverDateStr = obj["DriverDate"]?.ToString();
                var driverDate = ParseWmiDateTime(driverDateStr);
                writer.WriteLine($"    {"Driver Date",-16}: {driverDate}");

                var adapterRam = obj["AdapterRAM"];
                var vramDisplay = FormatVram(adapterRam);
                writer.WriteLine($"    {"VRAM",-16}: {vramDisplay}");

                var hRes = obj["CurrentHorizontalResolution"];
                var vRes = obj["CurrentVerticalResolution"];
                if (hRes is not null && vRes is not null)
                {
                    writer.WriteLine($"    {"Resolution",-16}: {hRes} x {vRes}");
                }

                var status = obj["Status"]?.ToString() ?? "Unknown";
                writer.WriteLine($"    {"Status",-16}: {status}");
            }
        }
        catch (Exception ex)
        {
            writer.WriteLine($"  Failed to query Win32_VideoController: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private static string ParseWmiDateTime(string? wmiDate)
    {
        if (string.IsNullOrEmpty(wmiDate) || wmiDate.Length < 8)
        {
            return "N/A";
        }

        // WMI datetime format: yyyyMMddHHmmss.ffffff+zzz
        if (DateTime.TryParseExact(
                wmiDate[..8],
                "yyyyMMdd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var dt))
        {
            return dt.ToString("yyyy-MM-dd");
        }

        return wmiDate;
    }

    private static string FormatVram(object? adapterRam)
    {
        if (adapterRam is null)
        {
            return "N/A";
        }

        // AdapterRAM is uint32 in WMI, which caps at 4 GB.
        // For GPUs with >4 GB VRAM, it may wrap around or show 4 GB.
        var bytes = Convert.ToUInt64(adapterRam);
        var gb = bytes / (1024.0 * 1024 * 1024);
        return $"{gb:F1} GB";
    }
}
