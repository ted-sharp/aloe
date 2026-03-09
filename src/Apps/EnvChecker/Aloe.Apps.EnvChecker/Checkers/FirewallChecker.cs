using System.Diagnostics;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class FirewallChecker : IChecker
{
    public string SectionKey => "firewall";

    public string SectionTitle => "Windows Firewall";

    public async Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        foreach (var profileName in new[] { "domainprofile", "privateprofile", "publicprofile" })
        {
            var label = profileName switch
            {
                "domainprofile" => "Domain",
                "privateprofile" => "Private",
                "publicprofile" => "Public",
                _ => profileName,
            };

            var state = await GetFirewallState(profileName);
            writer.WriteLine($"  {label,-20}: {state}");
        }
    }

    private static async Task<string> GetFirewallState(string profileName)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"advfirewall show {profileName} state",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            foreach (var line in output.Split('\n', StringSplitOptions.TrimEntries))
            {
                if (line.Contains("State", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("状態", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    return parts.Length >= 2 ? parts[^1] : line.Trim();
                }
            }

            return "Unknown";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
