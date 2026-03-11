using Microsoft.Win32;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class DefenderExclusionChecker : IChecker
{
    private static readonly string[] ExclusionRegistryPaths =
    [
        @"SOFTWARE\Microsoft\Windows Defender\Exclusions\Paths",
        @"SOFTWARE\Policies\Microsoft\Windows Defender\Exclusions\Paths",
    ];

    public string SectionKey => "defender";

    public string SectionTitle => "Windows Defender Exclusions";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        var config = profile.DefenderExclusion;
        var excludedPaths = LoadExcludedPaths();

        if (config.ShowRegistered && excludedPaths.Count > 0)
        {
            writer.WriteLine("  (Registered)");
            foreach (var path in excludedPaths.OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            {
                writer.WriteLine($"  {path,-44} [EXCLUDED]");
            }
        }

        var notRegistered = config.Paths
            .Where(p => !IsPathExcluded(p, excludedPaths))
            .ToList();

        if (notRegistered.Count > 0 && !config.HideIfNotExcluded)
        {
            writer.WriteLine("  (Required - not registered)");
            foreach (var path in notRegistered)
            {
                writer.WriteLine($"  {path,-44} [NOT EXCLUDED]");
            }
        }

        return Task.CompletedTask;
    }

    private static HashSet<string> LoadExcludedPaths()
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            foreach (var registryPath in ExclusionRegistryPaths)
            {
                using var key = Registry.LocalMachine.OpenSubKey(registryPath);
                if (key is null)
                {
                    continue;
                }

                foreach (var valueName in key.GetValueNames())
                {
                    if (!string.IsNullOrEmpty(valueName))
                    {
                        result.Add(valueName);
                    }
                }
            }
        }
        catch
        {
            // Cannot read Defender exclusions (requires admin or access denied)
        }

        return result;
    }

    private static bool IsPathExcluded(string targetPath, HashSet<string> excludedPaths)
    {
        // Exact match
        if (excludedPaths.Contains(targetPath))
        {
            return true;
        }

        // Normalize for comparison
        var normalizedTarget = Path.GetFullPath(targetPath).TrimEnd('\\');

        foreach (var excluded in excludedPaths)
        {
            var normalizedExcluded = Path.GetFullPath(excluded).TrimEnd('\\');

            // Exact match after normalization
            if (string.Equals(normalizedTarget, normalizedExcluded, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Parent directory exclusion covers subdirectories
            if (normalizedTarget.StartsWith(normalizedExcluded + "\\", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
