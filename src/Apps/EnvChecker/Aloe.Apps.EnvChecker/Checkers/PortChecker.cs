using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class PortChecker : IChecker
{
    public string SectionKey => "port";

    public string SectionTitle => "Port Check";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        var activePorts = GetActiveListeningPorts();

        foreach (var port in profile.Port.Ports)
        {
            var isListening = activePorts.Contains(port);
            var status = isListening ? "LISTENING" : "NOT LISTENING";
            var label = GetPortLabel(port);
            writer.WriteLine($"  {port,5}  {label,-20} [{status}]");
        }

        return Task.CompletedTask;
    }

    private static HashSet<int> GetActiveListeningPorts()
    {
        var properties = IPGlobalProperties.GetIPGlobalProperties();
        var listeners = properties.GetActiveTcpListeners();
        return new HashSet<int>(listeners.Select(ep => ep.Port));
    }

    private static string GetPortLabel(int port) => port switch
    {
        80 => "HTTP",
        443 => "HTTPS",
        5432 => "PostgreSQL",
        3306 => "MySQL",
        1433 => "SQL Server",
        6379 => "Redis",
        5000 => ".NET (HTTP)",
        5001 => ".NET (HTTPS)",
        8080 => "HTTP Alt",
        _ => string.Empty,
    };
}
