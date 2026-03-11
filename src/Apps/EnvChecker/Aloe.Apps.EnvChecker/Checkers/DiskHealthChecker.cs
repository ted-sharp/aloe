using System.Diagnostics;
using System.Management;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class DiskHealthChecker : IChecker
{
    private static readonly string[] DiskEventSources =
        ["disk", "Ntfs", "volmgr", "volsnap", "storflt", "Chkdsk"];

    public string SectionKey => "diskhealth";

    public string SectionTitle => "Disk Health";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        var config = profile.DiskHealth;

        WriteDiskDriveStatus(writer);
        writer.WriteLine();
        WriteSmartPrediction(writer);
        writer.WriteLine();
        WriteDiskEventLog(writer, config.EventLogHours, config.MaxEventEntries);

        return Task.CompletedTask;
    }

    private static void WriteDiskDriveStatus(TextWriter writer)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT DeviceID, Model, Status, MediaType, Size FROM Win32_DiskDrive");

            foreach (var obj in searcher.Get())
            {
                var deviceId = obj["DeviceID"]?.ToString() ?? "Unknown";
                var model = obj["Model"]?.ToString() ?? "Unknown";
                var status = obj["Status"]?.ToString() ?? "Unknown";
                var mediaType = obj["MediaType"]?.ToString() ?? "Unknown";
                var sizeBytes = obj["Size"] is ulong s ? s : 0UL;
                var sizeGb = sizeBytes / (1024.0 * 1024 * 1024);

                var warning = !string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase)
                    ? " [WARNING]"
                    : string.Empty;

                writer.WriteLine($"  {deviceId} ({model})");
                writer.WriteLine($"    {"Status",-16}: {status}{warning}");
                writer.WriteLine($"    {"Media Type",-16}: {mediaType}");
                writer.WriteLine($"    {"Size",-16}: {sizeGb:F1} GB");
            }
        }
        catch (Exception ex)
        {
            writer.WriteLine($"  Failed to query Win32_DiskDrive: {ex.Message}");
        }
    }

    private static void WriteSmartPrediction(TextWriter writer)
    {
        writer.WriteLine("  SMART Failure Prediction:");

        try
        {
            // Build a map of drive index -> model name from Win32_DiskDrive
            var driveModels = new Dictionary<uint, string>();
            using (var driveSearcher = new ManagementObjectSearcher(
                "SELECT Index, Model FROM Win32_DiskDrive"))
            {
                foreach (var obj in driveSearcher.Get())
                {
                    if (obj["Index"] is uint index)
                    {
                        driveModels[index] = obj["Model"]?.ToString() ?? "Unknown";
                    }
                }
            }

            using var searcher = new ManagementObjectSearcher(
                @"root\WMI",
                "SELECT InstanceName, PredictFailure, Reason FROM MSStorageDriver_FailurePredictStatus");

            var found = false;
            foreach (var obj in searcher.Get())
            {
                found = true;
                var instanceName = obj["InstanceName"]?.ToString() ?? "Unknown";
                var predictFailure = obj["PredictFailure"] is bool pf && pf;

                // Extract drive index from instance name (e.g., "..._0" -> 0)
                var model = ExtractModelFromInstance(instanceName, driveModels);
                var label = model is not null ? $"{instanceName} ({model})" : instanceName;

                if (predictFailure)
                {
                    var reason = obj["Reason"]?.ToString() ?? "Unknown";
                    writer.WriteLine($"    {label} : FAILING [CRITICAL] (Reason: {reason})");
                }
                else
                {
                    writer.WriteLine($"    {label} : OK");
                }
            }

            if (!found)
            {
                writer.WriteLine("    SMART data not available (VM or unsupported controller)");
            }
        }
        catch (ManagementException ex) when (ex.ErrorCode == ManagementStatus.AccessDenied)
        {
            writer.WriteLine("    Requires administrator privileges");
        }
        catch (ManagementException)
        {
            writer.WriteLine("    SMART data not available (VM or unsupported controller)");
        }
        catch (Exception ex)
        {
            writer.WriteLine($"    Failed to query SMART status: {ex.Message}");
        }
    }

    private static string? ExtractModelFromInstance(string instanceName, Dictionary<uint, string> driveModels)
    {
        // Instance names often end with _0, _1, etc. corresponding to drive index
        var lastUnderscore = instanceName.LastIndexOf('_');
        if (lastUnderscore >= 0 && uint.TryParse(instanceName.AsSpan(lastUnderscore + 1), out var index))
        {
            return driveModels.GetValueOrDefault(index);
        }

        return null;
    }

    private static void WriteDiskEventLog(TextWriter writer, int hours, int maxEntries)
    {
        writer.WriteLine($"  Disk Event Log (past {hours} hours):");

        try
        {
            var since = DateTime.Now.AddHours(-hours);
            using var log = new System.Diagnostics.EventLog("System");

            var entries = log.Entries.Cast<EventLogEntry>()
                .Where(e => e.TimeGenerated >= since
                    && DiskEventSources.Contains(e.Source, StringComparer.OrdinalIgnoreCase)
                    && (e.EntryType == EventLogEntryType.Error || e.EntryType == EventLogEntryType.Warning))
                .OrderByDescending(e => e.TimeGenerated)
                .ToList();

            writer.WriteLine($"    Error/Warning count: {entries.Count}");

            foreach (var entry in entries.Take(maxEntries))
            {
                var msg = Truncate(entry.Message, 120);
                writer.WriteLine($"    {entry.TimeGenerated:yyyy-MM-dd HH:mm:ss}  [{entry.Source}] ({entry.EntryType})");
                writer.WriteLine($"      {msg}");
            }
        }
        catch (Exception ex)
        {
            writer.WriteLine($"    Failed to read event log: {ex.Message}");
        }
    }

    private static string Truncate(string text, int maxLength)
    {
        var singleLine = text.ReplaceLineEndings(" ");
        return singleLine.Length <= maxLength
            ? singleLine
            : string.Concat(singleLine.AsSpan(0, maxLength), "...");
    }
}
