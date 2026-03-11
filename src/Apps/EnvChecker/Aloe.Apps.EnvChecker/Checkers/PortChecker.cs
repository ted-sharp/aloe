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

            if (!isListening && profile.Port.HideIfNotListening)
            {
                continue;
            }

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
        22 => "SSH",
        25 => "SMTP",
        53 => "DNS",
        80 => "HTTP",
        104 => "DICOM",
        110 => "POP3",
        123 => "NTP",
        143 => "IMAP",
        161 => "SNMP",
        162 => "SNMP Trap",
        443 => "HTTPS",
        445 => "SMB",
        500 => "IPsec/IKE",
        514 => "Syslog",
        587 => "SMTP (Submission)",
        902 => "VMware ESXi",
        903 => "VMware Console",
        993 => "IMAPS",
        995 => "POP3S",
        1194 => "OpenVPN",
        1433 => "SQL Server",
        1521 => "Oracle",
        1812 => "RADIUS",
        1813 => "RADIUS Accounting",
        2375 => "Docker",
        2376 => "Docker TLS",
        2575 => "HL7",
        2761 => "DICOM TLS",
        2762 => "DICOM TLS Alt",
        3000 => "Node.js Dev",
        3306 => "MySQL",
        3389 => "RDP",
        4006 => "DICOM Alt",
        4200 => "Angular Dev",
        4242 => "Orthanc DICOM",
        4500 => "IPsec NAT-T",
        5000 => ".NET (HTTP)",
        5001 => ".NET (HTTPS)",
        5432 => "PostgreSQL",
        5433 => "PostgreSQL Alt",
        5672 => "RabbitMQ",
        5985 => "WinRM (HTTP)",
        5986 => "WinRM (HTTPS)",
        6379 => "Redis",
        6443 => "Kubernetes API",
        8006 => "Proxmox",
        8042 => "Orthanc REST",
        8080 => "HTTP Alt",
        8443 => "HTTPS Alt",
        9000 => "SonarQube",
        9042 => "Cassandra",
        9092 => "Kafka",
        9200 => "Elasticsearch",
        10250 => "Kubelet",
        11112 => "DICOM Alt",
        15672 => "RabbitMQ Mgmt",
        27017 => "MongoDB",
        51820 => "WireGuard",
        61616 => "ActiveMQ",
        _ => string.Empty,
    };
}
