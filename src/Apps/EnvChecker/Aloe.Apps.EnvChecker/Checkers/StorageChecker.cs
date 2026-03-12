using System.Management;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class StorageChecker : IChecker
{
    public string SectionKey => "storage";

    public string SectionTitle => "Storage (RAID / Virtual Disk)";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        WritePhysicalDisks(writer);
        writer.WriteLine();
        WriteStoragePools(writer);
        writer.WriteLine();
        WriteVirtualDisks(writer);
        writer.WriteLine();
        writer.WriteLine("  Note: Hardware RAID details require vendor-specific tools (storcli, hpssacli, etc.)");

        return Task.CompletedTask;
    }

    private static void WritePhysicalDisks(TextWriter writer)
    {
        writer.WriteLine("  Physical Disks:");

        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\Microsoft\Windows\Storage",
                "SELECT FriendlyName, DeviceId, MediaType, BusType, HealthStatus, Size FROM MSFT_PhysicalDisk");

            var found = false;
            foreach (var obj in searcher.Get())
            {
                found = true;
                var name = obj["FriendlyName"]?.ToString() ?? "Unknown";
                var deviceId = obj["DeviceId"]?.ToString() ?? "?";
                var mediaType = FormatMediaType(obj["MediaType"]);
                var busType = FormatBusType(obj["BusType"]);
                var healthStatus = FormatHealthStatus(obj["HealthStatus"]);
                var sizeBytes = obj["Size"] is ulong s ? s : 0UL;
                var sizeDisplay = sizeBytes > 0 ? $", {sizeBytes / (1024.0 * 1024 * 1024):F0} GB" : string.Empty;

                writer.WriteLine($"    Disk {deviceId}: {name} ({mediaType}, {busType}{sizeDisplay}) [{healthStatus}]");
            }

            if (!found)
            {
                writer.WriteLine("    (No physical disks found via MSFT_PhysicalDisk)");
            }
        }
        catch (ManagementException ex) when (ex.ErrorCode == ManagementStatus.InvalidNamespace)
        {
            writer.WriteLine("    Storage namespace not available (root\\Microsoft\\Windows\\Storage)");
        }
        catch (Exception ex)
        {
            writer.WriteLine($"    Failed to query MSFT_PhysicalDisk: {ex.Message}");
        }
    }

    private static void WriteStoragePools(TextWriter writer)
    {
        writer.WriteLine("  Storage Pools:");

        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\Microsoft\Windows\Storage",
                "SELECT FriendlyName, HealthStatus, OperationalStatus, Size, AllocatedSize, IsPrimordial FROM MSFT_StoragePool");

            var found = false;
            foreach (var obj in searcher.Get())
            {
                // Skip the primordial pool (system default)
                if (obj["IsPrimordial"] is bool isPrimordial && isPrimordial)
                {
                    continue;
                }

                found = true;
                var name = obj["FriendlyName"]?.ToString() ?? "Unknown";
                var healthStatus = obj["HealthStatus"]?.ToString() ?? "Unknown";
                var sizeBytes = obj["Size"] is ulong s ? s : 0UL;
                var allocatedBytes = obj["AllocatedSize"] is ulong a ? a : 0UL;
                var sizeGb = sizeBytes / (1024.0 * 1024 * 1024);
                var allocGb = allocatedBytes / (1024.0 * 1024 * 1024);

                writer.WriteLine($"    {name}");
                writer.WriteLine($"      {"Health",-16}: {healthStatus}");
                writer.WriteLine($"      {"Size",-16}: {sizeGb:F1} GB (Allocated: {allocGb:F1} GB)");
            }

            if (!found)
            {
                writer.WriteLine("    (No Storage Spaces pools configured)");
            }
        }
        catch (ManagementException ex) when (ex.ErrorCode == ManagementStatus.InvalidNamespace)
        {
            writer.WriteLine("    Storage namespace not available");
        }
        catch (Exception ex)
        {
            writer.WriteLine($"    Failed to query MSFT_StoragePool: {ex.Message}");
        }
    }

    private static void WriteVirtualDisks(TextWriter writer)
    {
        writer.WriteLine("  Virtual Disks (Storage Spaces):");

        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\Microsoft\Windows\Storage",
                "SELECT FriendlyName, HealthStatus, OperationalStatus, ResiliencySettingName, Size, FootprintOnPool FROM MSFT_VirtualDisk");

            var found = false;
            foreach (var obj in searcher.Get())
            {
                found = true;
                var name = obj["FriendlyName"]?.ToString() ?? "Unknown";
                var healthStatus = obj["HealthStatus"]?.ToString() ?? "Unknown";
                var resiliency = obj["ResiliencySettingName"]?.ToString() ?? "Unknown";
                var sizeBytes = obj["Size"] is ulong s ? s : 0UL;
                var footprintBytes = obj["FootprintOnPool"] is ulong f ? f : 0UL;
                var sizeGb = sizeBytes / (1024.0 * 1024 * 1024);
                var footprintGb = footprintBytes / (1024.0 * 1024 * 1024);

                writer.WriteLine($"    {name}");
                writer.WriteLine($"      {"Health",-16}: {healthStatus}");
                writer.WriteLine($"      {"Resiliency",-16}: {resiliency}");
                writer.WriteLine($"      {"Size",-16}: {sizeGb:F1} GB (Footprint: {footprintGb:F1} GB)");
            }

            if (!found)
            {
                writer.WriteLine("    (No virtual disks configured)");
            }
        }
        catch (ManagementException ex) when (ex.ErrorCode == ManagementStatus.InvalidNamespace)
        {
            writer.WriteLine("    Storage namespace not available");
        }
        catch (Exception ex)
        {
            writer.WriteLine($"    Failed to query MSFT_VirtualDisk: {ex.Message}");
        }
    }

    private static string FormatMediaType(object? value)
    {
        // MSFT_PhysicalDisk MediaType: 0=Unspecified, 3=HDD, 4=SSD, 5=SCM
        if (value is not ushort mediaType)
        {
            return "Unknown";
        }

        return mediaType switch
        {
            3 => "HDD",
            4 => "SSD",
            5 => "SCM",
            _ => "Unspecified",
        };
    }

    private static string FormatBusType(object? value)
    {
        // MSFT_PhysicalDisk BusType
        if (value is not ushort busType)
        {
            return "Unknown";
        }

        return busType switch
        {
            1 => "SCSI",
            2 => "ATAPI",
            3 => "ATA",
            5 => "USB",
            6 => "Fibre Channel",
            7 => "iSCSI",
            8 => "SAS",
            9 => "SATA",
            10 => "SD",
            11 => "MMC",
            17 => "NVMe",
            _ => $"Other({busType})",
        };
    }

    private static string FormatHealthStatus(object? value)
    {
        // MSFT_PhysicalDisk HealthStatus: 0=Healthy, 1=Warning, 2=Unhealthy
        if (value is not ushort healthStatus)
        {
            return "Unknown";
        }

        return healthStatus switch
        {
            0 => "Healthy",
            1 => "Warning",
            2 => "Unhealthy",
            _ => $"Unknown({healthStatus})",
        };
    }
}
