using System.Diagnostics;
using Microsoft.Win32;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class InstalledSoftwareChecker : IChecker
{
    public string SectionKey => "software";

    public string SectionTitle => "Installed Software";

    public async Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        foreach (var command in profile.Software.Commands)
        {
            var location = FindOnPath(command);
            if (location is null)
            {
                if (!profile.Software.HideIfNotFound)
                {
                    writer.WriteLine($"  {command,-16}: NOT FOUND");
                }

                continue;
            }

            var version = await GetVersion(location);
            writer.WriteLine($"  {command,-16}: {version}");
            writer.WriteLine($"  {"",-16}  ({location})");
        }

        if (profile.Software.DetectPostgresVersions)
        {
            await DetectPostgresVersionsAsync(writer, profile.Software.HideIfNotFound);
        }

        foreach (var pattern in profile.Software.Applications)
        {
            var result = FindInstalledApp(pattern);
            if (result is null)
            {
                if (!profile.Software.HideIfNotFound)
                {
                    writer.WriteLine($"  {pattern,-36}: NOT FOUND");
                }

                continue;
            }

            writer.WriteLine($"  {result.Value.DisplayName,-36}: {result.Value.Version}");
        }
    }

    private static async Task DetectPostgresVersionsAsync(TextWriter writer, bool hideIfNotFound)
    {
        var pgBase = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PostgreSQL");

        if (!Directory.Exists(pgBase))
        {
            if (!hideIfNotFound)
            {
                writer.WriteLine($"  postgresql         : NOT FOUND (no {pgBase})");
            }

            return;
        }

        var found = false;
        foreach (var versionDir in Directory.GetDirectories(pgBase).Order())
        {
            var psqlPath = Path.Combine(versionDir, "bin", "psql.exe");
            if (!File.Exists(psqlPath))
            {
                continue;
            }

            found = true;
            var ver = Path.GetFileName(versionDir);
            var label = $"postgresql-{ver}";
            var version = await GetVersion(psqlPath);
            writer.WriteLine($"  {label,-16}: {version}");
            writer.WriteLine($"  {"",-16}  ({psqlPath})");
        }

        if (!found && !hideIfNotFound)
        {
            writer.WriteLine($"  postgresql         : NOT FOUND (no versions in {pgBase})");
        }
    }

    private static (string DisplayName, string Version)? FindInstalledApp(string pattern)
    {
        var uninstallPaths = new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
        };

        foreach (var keyPath in uninstallPaths)
        {
            using var key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key is null)
            {
                continue;
            }

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                if (subKey?.GetValue("DisplayName") is not string displayName)
                {
                    continue;
                }

                if (!MatchesPattern(displayName, pattern))
                {
                    continue;
                }

                var version = subKey.GetValue("DisplayVersion") as string ?? "(unknown)";
                return (displayName, version);
            }
        }

        return null;
    }

    private static bool MatchesPattern(string name, string pattern)
    {
        if (pattern.EndsWith('*'))
        {
            return name.StartsWith(pattern[..^1], StringComparison.OrdinalIgnoreCase);
        }

        return string.Equals(name, pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static string? FindOnPath(string command)
    {
        var extensions = new[] { ".exe", ".cmd", ".bat", "" };
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;

        foreach (var dir in pathEnv.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var ext in extensions)
            {
                var candidate = Path.Combine(dir.Trim(), command + ext);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static async Task<string> GetVersion(string fileName)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await process.WaitForExitAsync(cts.Token);

            var result = string.IsNullOrWhiteSpace(output) ? error : output;
            var firstLine = result.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();
            return firstLine ?? "(unknown)";
        }
        catch
        {
            return "(failed to get version)";
        }
    }
}
