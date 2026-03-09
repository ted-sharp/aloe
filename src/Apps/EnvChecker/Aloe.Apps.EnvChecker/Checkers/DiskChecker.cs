namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class DiskChecker : IChecker
{
    public string SectionKey => "disk";

    public string SectionTitle => "Disk";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        var threshold = profile.Disk.WarningThresholdPercent;

        foreach (var drive in DriveInfo.GetDrives())
        {
            if (!drive.IsReady)
            {
                writer.WriteLine($"  {drive.Name,-10} ({drive.DriveType}) -- Not Ready");
                continue;
            }

            var totalGb = drive.TotalSize / (1024.0 * 1024 * 1024);
            var usedGb = (drive.TotalSize - drive.TotalFreeSpace) / (1024.0 * 1024 * 1024);
            var usagePercent = drive.TotalSize > 0
                ? (double)(drive.TotalSize - drive.TotalFreeSpace) / drive.TotalSize * 100.0
                : 0;

            var warning = usagePercent >= threshold ? $" [WARNING: >= {threshold}%]" : string.Empty;
            writer.WriteLine(
                $"  {drive.Name,-4} ({drive.DriveFormat}, {drive.DriveType})  " +
                $"{usedGb:F1} / {totalGb:F1} GB used ({usagePercent:F1} %){warning}");
        }

        return Task.CompletedTask;
    }
}
