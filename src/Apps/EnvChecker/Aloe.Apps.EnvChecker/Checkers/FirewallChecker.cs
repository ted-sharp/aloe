using Microsoft.Win32;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class FirewallChecker : IChecker
{
    private const string FirewallPolicyPath =
        @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy";

    private static readonly (string SubKey, string Label)[] Profiles =
    [
        ("DomainProfile", "Domain"),
        ("StandardProfile", "Private"),
        ("PublicProfile", "Public"),
    ];

    public string SectionKey => "firewall";

    public string SectionTitle => "Windows Firewall";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        foreach (var (subKey, label) in Profiles)
        {
            var state = GetFirewallEnabled(subKey);
            writer.WriteLine($"  {label,-20}: {state}");
        }

        return Task.CompletedTask;
    }

    private static string GetFirewallEnabled(string profileSubKey)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                $@"{FirewallPolicyPath}\{profileSubKey}");

            if (key is null)
                return "Unknown (registry key not found)";

            var value = key.GetValue("EnableFirewall");
            return value switch
            {
                1 => "ON",
                0 => "OFF",
                _ => $"Unknown ({value})",
            };
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
