using System.Diagnostics;

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
                writer.WriteLine($"  {command,-16}: NOT FOUND");
                continue;
            }

            var version = await GetVersion(command);
            writer.WriteLine($"  {command,-16}: {version}");
            writer.WriteLine($"  {"",-16}  ({location})");
        }
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

    private static async Task<string> GetVersion(string command)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = command,
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
