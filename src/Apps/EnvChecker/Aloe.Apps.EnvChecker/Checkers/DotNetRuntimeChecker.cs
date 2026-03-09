using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class DotNetRuntimeChecker : IChecker
{
    public string SectionKey => "dotnet";

    public string SectionTitle => ".NET Runtime";

    public async Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        writer.WriteLine($"  {"Current Runtime",-20}: {RuntimeInformation.FrameworkDescription}");

        var dotnetPath = FindDotnet();
        if (dotnetPath is null)
        {
            writer.WriteLine("  dotnet command not found on PATH");
            return;
        }

        writer.WriteLine($"  {"dotnet path",-20}: {dotnetPath}");
        writer.WriteLine();

        writer.WriteLine("  Installed Runtimes:");
        var runtimes = await RunDotnetCommand("--list-runtimes");
        foreach (var line in runtimes)
        {
            writer.WriteLine($"    {line}");
        }

        writer.WriteLine();
        writer.WriteLine("  Installed SDKs:");
        var sdks = await RunDotnetCommand("--list-sdks");
        foreach (var line in sdks)
        {
            writer.WriteLine($"    {line}");
        }
    }

    private static string? FindDotnet()
    {
        var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (dotnetRoot is not null)
        {
            var path = Path.Combine(dotnetRoot, "dotnet.exe");
            if (File.Exists(path))
            {
                return path;
            }
        }

        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var dir in pathEnv.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(dir.Trim(), "dotnet.exe");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static async Task<string[]> RunDotnetCommand(string arguments)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        catch
        {
            return ["(failed to execute dotnet command)"];
        }
    }
}
