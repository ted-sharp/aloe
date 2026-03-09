using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aloe.Apps.EnvChecker;

internal sealed class CheckProfile
{
    public SectionToggle System { get; set; } = new() { Enabled = true };
    public SectionToggle Cpu { get; set; } = new() { Enabled = true };
    public SectionToggle Memory { get; set; } = new() { Enabled = true };
    public DiskSection Disk { get; set; } = new();
    public SectionToggle Dotnet { get; set; } = new() { Enabled = true };
    public SectionToggle Vcruntime { get; set; } = new() { Enabled = true };
    public NetworkSection Network { get; set; } = new();
    public PortSection Port { get; set; } = new();
    public EnvSection Env { get; set; } = new();
    public SectionToggle Firewall { get; set; } = new() { Enabled = true };
    public ServiceSection Service { get; set; } = new();
    public SoftwareSection Software { get; set; } = new();
    public EventLogSection EventLog { get; set; } = new();
    public CertSection Cert { get; set; } = new();

    public bool IsSectionEnabled(string sectionKey) => sectionKey switch
    {
        "system" => System.Enabled,
        "cpu" => Cpu.Enabled,
        "memory" => Memory.Enabled,
        "disk" => Disk.Enabled,
        "dotnet" => Dotnet.Enabled,
        "vcruntime" => Vcruntime.Enabled,
        "network" => Network.Enabled,
        "port" => Port.Enabled,
        "env" => Env.Enabled,
        "firewall" => Firewall.Enabled,
        "service" => Service.Enabled,
        "software" => Software.Enabled,
        "eventlog" => EventLog.Enabled,
        "cert" => Cert.Enabled,
        _ => false,
    };

    public void EnableOnly(IReadOnlyList<string> sections)
    {
        DisableAll();
        foreach (var s in sections)
        {
            SetSection(s, true);
        }
    }

    public void Exclude(IReadOnlyList<string> sections)
    {
        foreach (var s in sections)
        {
            SetSection(s, false);
        }
    }

    public static CheckProfile CreateAllEnabled() => new();

    public static CheckProfile LoadFromFile(string path)
    {
        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };
        return JsonSerializer.Deserialize<CheckProfile>(json, options) ?? new CheckProfile();
    }

    public static string GenerateSampleJson()
    {
        var sample = new CheckProfile
        {
            System = new() { Enabled = true },
            Cpu = new() { Enabled = true },
            Memory = new() { Enabled = true },
            Disk = new() { Enabled = true, WarningThresholdPercent = 90 },
            Dotnet = new() { Enabled = true },
            Vcruntime = new() { Enabled = true },
            Network = new() { Enabled = true, DnsTestHost = "www.google.com", PingTestHost = "8.8.8.8" },
            Port = new() { Enabled = true, Ports = [80, 443, 5432, 5000, 5001] },
            Env = new()
            {
                Enabled = true,
                Variables = ["PATH", "DOTNET_ROOT", "ASPNETCORE_ENVIRONMENT", "PGDATA", "PGHOST", "PGPORT", "PGUSER"],
                ShowPathEntries = true,
                HideIfNotSet = true,
            },
            Firewall = new() { Enabled = true },
            Service = new()
            {
                Enabled = true,
                Services =
                [
                    "MSSQLSERVER", "MSSQL$*",
                    "postgresql-x64-*",
                    "OracleService*",
                    "MySQL*",
                    "W3SVC", "nginx",
                    "wuauserv", "W32Time",
                ],
                HideIfNotInstalled = true,
            },
            Software = new() { Enabled = true, Commands = ["dotnet", "git", "node", "npm", "psql", "docker", "python"] },
            EventLog = new() { Enabled = true, LogNames = ["Application", "System"], Hours = 24, MaxEntries = 5 },
            Cert = new() { Enabled = true, WarningDays = 30 },
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        };
        return JsonSerializer.Serialize(sample, options);
    }

    private void DisableAll()
    {
        System.Enabled = false;
        Cpu.Enabled = false;
        Memory.Enabled = false;
        Disk.Enabled = false;
        Dotnet.Enabled = false;
        Vcruntime.Enabled = false;
        Network.Enabled = false;
        Port.Enabled = false;
        Env.Enabled = false;
        Firewall.Enabled = false;
        Service.Enabled = false;
        Software.Enabled = false;
        EventLog.Enabled = false;
        Cert.Enabled = false;
    }

    private void SetSection(string key, bool enabled)
    {
        switch (key)
        {
            case "system": System.Enabled = enabled; break;
            case "cpu": Cpu.Enabled = enabled; break;
            case "memory": Memory.Enabled = enabled; break;
            case "disk": Disk.Enabled = enabled; break;
            case "dotnet": Dotnet.Enabled = enabled; break;
            case "vcruntime": Vcruntime.Enabled = enabled; break;
            case "network": Network.Enabled = enabled; break;
            case "port": Port.Enabled = enabled; break;
            case "env": Env.Enabled = enabled; break;
            case "firewall": Firewall.Enabled = enabled; break;
            case "service": Service.Enabled = enabled; break;
            case "software": Software.Enabled = enabled; break;
            case "eventlog": EventLog.Enabled = enabled; break;
            case "cert": Cert.Enabled = enabled; break;
        }
    }
}

internal class SectionToggle
{
    public bool Enabled { get; set; } = true;
}

internal sealed class DiskSection : SectionToggle
{
    public int WarningThresholdPercent { get; set; } = 90;
}

internal sealed class NetworkSection : SectionToggle
{
    public string DnsTestHost { get; set; } = "www.google.com";
    public string PingTestHost { get; set; } = "8.8.8.8";
}

internal sealed class PortSection : SectionToggle
{
    public List<int> Ports { get; set; } = [80, 443, 5432, 5000, 5001];
}

internal sealed class EnvSection : SectionToggle
{
    public List<string> Variables { get; set; } =
        ["PATH", "DOTNET_ROOT", "ASPNETCORE_ENVIRONMENT", "PGDATA", "PGHOST", "PGPORT", "PGUSER"];

    public bool ShowPathEntries { get; set; } = true;
    public bool HideIfNotSet { get; set; } = true;
}

internal sealed class ServiceSection : SectionToggle
{
    public List<string> Services { get; set; } =
    [
        "MSSQLSERVER", "MSSQL$*",
        "postgresql-x64-*",
        "OracleService*",
        "MySQL*",
        "W3SVC", "nginx",
        "wuauserv", "W32Time",
    ];

    public bool HideIfNotInstalled { get; set; } = true;
}

internal sealed class SoftwareSection : SectionToggle
{
    public List<string> Commands { get; set; } = ["dotnet", "git", "node", "npm", "psql", "docker", "python"];
}

internal sealed class EventLogSection : SectionToggle
{
    public List<string> LogNames { get; set; } = ["Application", "System"];
    public int Hours { get; set; } = 24;
    public int MaxEntries { get; set; } = 5;
}

internal sealed class CertSection : SectionToggle
{
    public int WarningDays { get; set; } = 30;
}
