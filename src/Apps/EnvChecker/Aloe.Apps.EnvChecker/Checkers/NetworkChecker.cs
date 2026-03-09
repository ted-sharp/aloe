using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class NetworkChecker : IChecker
{
    public string SectionKey => "network";

    public string SectionTitle => "Network";

    public async Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        var networkConfig = profile.Network;

        WriteAdapters(writer);
        writer.WriteLine();
        await WriteDnsTest(writer, networkConfig.DnsTestHost);
        await WritePingTest(writer, networkConfig.PingTestHost);
    }

    private static void WriteAdapters(TextWriter writer)
    {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                         ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Where(HasIpv4Address)
            .ToArray();

        if (interfaces.Length == 0)
        {
            writer.WriteLine("  No active network adapters found");
            return;
        }

        foreach (var ni in interfaces)
        {
            var ipProps = ni.GetIPProperties();
            var ipv4 = ipProps.UnicastAddresses
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.Address.ToString())
                .ToArray();

            writer.WriteLine($"  {ni.Name} ({ni.NetworkInterfaceType})");
            writer.WriteLine($"    MAC        : {FormatMac(ni.GetPhysicalAddress().ToString())}");
            writer.WriteLine($"    IPv4       : {string.Join(", ", ipv4)}");

            var gateways = ipProps.GatewayAddresses
                .Select(g => g.Address.ToString())
                .Where(g => !g.StartsWith("fe80", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (gateways.Length > 0)
            {
                writer.WriteLine($"    Gateway    : {string.Join(", ", gateways)}");
            }

            var dns = ipProps.DnsAddresses
                .Where(d => d.AddressFamily == AddressFamily.InterNetwork)
                .Select(d => d.ToString())
                .ToArray();
            if (dns.Length > 0)
            {
                writer.WriteLine($"    DNS        : {string.Join(", ", dns)}");
            }

            writer.WriteLine();
        }
    }

    private static async Task WriteDnsTest(TextWriter writer, string host)
    {
        writer.WriteLine($"  DNS Resolution Test ({host}):");
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(host);
            var ips = string.Join(", ", addresses.Select(a => a.ToString()));
            writer.WriteLine($"    Result     : OK ({ips})");
        }
        catch (Exception ex)
        {
            writer.WriteLine($"    Result     : FAILED ({ex.Message})");
        }
    }

    private static async Task WritePingTest(TextWriter writer, string host)
    {
        writer.WriteLine($"  Ping Test ({host}):");
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(host, 3000);
            if (reply.Status == IPStatus.Success)
            {
                writer.WriteLine($"    Result     : OK ({reply.RoundtripTime} ms)");
            }
            else
            {
                writer.WriteLine($"    Result     : {reply.Status}");
            }
        }
        catch (Exception ex)
        {
            writer.WriteLine($"    Result     : FAILED ({ex.Message})");
        }
    }

    private static bool HasIpv4Address(NetworkInterface ni)
    {
        return ni.GetIPProperties().UnicastAddresses
            .Any(a => a.Address.AddressFamily == AddressFamily.InterNetwork);
    }

    private static string FormatMac(string raw)
    {
        if (string.IsNullOrEmpty(raw) || raw.Length < 12)
        {
            return raw;
        }

        return string.Join(":", Enumerable.Range(0, 6).Select(i => raw.Substring(i * 2, 2)));
    }
}
