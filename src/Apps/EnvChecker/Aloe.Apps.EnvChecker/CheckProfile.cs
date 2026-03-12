using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aloe.Apps.EnvChecker;

internal sealed class CheckProfile
{
    public SectionToggle System { get; set; } = new() { Enabled = true };
    public SectionToggle Cpu { get; set; } = new() { Enabled = true };
    public SectionToggle Gpu { get; set; } = new() { Enabled = true };
    public SectionToggle Memory { get; set; } = new() { Enabled = true };
    public DiskSection Disk { get; set; } = new();
    public DiskHealthSection DiskHealth { get; set; } = new();
    public SectionToggle Dotnet { get; set; } = new() { Enabled = true };
    public SectionToggle Vcruntime { get; set; } = new() { Enabled = true };
    public NetworkSection Network { get; set; } = new();
    public PortSection Port { get; set; } = new();
    public EnvSection Env { get; set; } = new();
    public SectionToggle Storage { get; set; } = new() { Enabled = true };
    public SectionToggle Firewall { get; set; } = new() { Enabled = true };
    public FirewallRuleSection FirewallRule { get; set; } = new();
    public RegistrySection Registry { get; set; } = new();
    public DefenderExclusionSection DefenderExclusion { get; set; } = new();
    public ServiceSection Service { get; set; } = new();
    public SoftwareSection Software { get; set; } = new();
    public EventLogSection EventLog { get; set; } = new();
    public CertSection Cert { get; set; } = new();

    public bool IsSectionEnabled(string sectionKey) => sectionKey switch
    {
        "system" => System.Enabled,
        "cpu" => Cpu.Enabled,
        "gpu" => Gpu.Enabled,
        "memory" => Memory.Enabled,
        "disk" => Disk.Enabled,
        "diskhealth" => DiskHealth.Enabled,
        "storage" => Storage.Enabled,
        "dotnet" => Dotnet.Enabled,
        "vcruntime" => Vcruntime.Enabled,
        "network" => Network.Enabled,
        "port" => Port.Enabled,
        "env" => Env.Enabled,
        "firewall" => Firewall.Enabled,
        "firewallrule" => FirewallRule.Enabled,
        "registry" => Registry.Enabled,
        "defender" => DefenderExclusion.Enabled,
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

    public void ShowAll()
    {
        Port.HideIfNotListening = false;
        Env.HideIfNotSet = false;
        Env.HidePathNotExist = false;
        Env.HideRequiredPathIfNotFound = false;
        Env.HidePathEntriesIfRequired = false;
        Service.HideIfNotInstalled = false;
        Software.HideIfNotFound = false;
        FirewallRule.HideIfNotAllowed = false;
        Registry.HideIfNotFound = false;
        DefenderExclusion.HideIfNotExcluded = false;
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
            Gpu = new() { Enabled = true },
            Memory = new() { Enabled = true },
            Disk = new() { Enabled = true, WarningThresholdPercent = 90 },
            DiskHealth = new() { Enabled = true, EventLogHours = 168, MaxEventEntries = 5 },
            Storage = new() { Enabled = true },
            Dotnet = new() { Enabled = true },
            Vcruntime = new() { Enabled = true },
            Network = new() { Enabled = true, DnsTestHost = "www.google.com", PingTestHost = "8.8.8.8", ShowStaticRoutes = true },
            Port = new()
            {
                Enabled = true,
                Ports =
                [
                    80, 443, 104, 11112, 4242, 4006, 8042, 2575, 2761, 2762,
                    5432, 5000, 5001,
                    6379, 5672, 15672, 27017, 9042, 9092, 61616,
                    6443, 10250, 2375,
                    5985, 5986, 9000, 8006,
                ],
                HideIfNotListening = true,
            },
            Env = new()
            {
                Enabled = true,
                Variables = ["PATH", "DOTNET_ROOT", "ASPNETCORE_ENVIRONMENT", "PGDATA", "PGHOST", "PGPORT", "PGUSER"],
                ShowPathEntries = true,
                HideIfNotSet = true,
                HidePathNotExist = true,
                RequiredPaths = [@"C:\my\tools", @"%USERPROFILE%\scoop\shims"],
                HideRequiredPathIfNotFound = true,
                HidePathEntriesIfRequired = true,
            },
            Firewall = new() { Enabled = true },
            FirewallRule = new()
            {
                Enabled = true,
                Ports =
                [
                    80, 443, 3389,
                    5432, 5433, 1433, 3306, 1521, 6379, 27017, 5672,
                    104, 11112, 2575, 4242, 8042, 4006, 2761, 2762,
                ],
                Programs = [],
                CheckIcmp = true,
                HideIfNotAllowed = true,
            },
            Registry = new()
            {
                Enabled = true,
                Entries =
                [
                    new RegistryEntry
                    {
                        Path = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Orthanc",
                        Label = "Orthanc",
                    },
                ],
                HideIfNotFound = true,
            },
            DefenderExclusion = new()
            {
                Enabled = true,
                ShowRegistered = true,
                Paths = [@"C:\Orthanc", @"C:\PostgreSQL"],
                HideIfNotExcluded = true,
            },
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
            Software = new()
            {
                Enabled = true,
                Commands = ["dotnet", "git", "code", "node", "npm", "docker", "python"],
                Applications = ["Microsoft Office*", "Microsoft 365*"],
                DetectPostgresVersions = true,
                HideIfNotFound = true,
            },
            EventLog = new()
            {
                Enabled = true,
                LogNames = ["Application", "System"],
                Hours = 24,
                MaxEntries = 5,
                IgnoreSources = ["DCOM", "DistributedCOM", "Microsoft-Windows-DistributedCOM", "TPM-WMI", "ACPI"],
            },
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
        Gpu.Enabled = false;
        Memory.Enabled = false;
        Disk.Enabled = false;
        DiskHealth.Enabled = false;
        Storage.Enabled = false;
        Dotnet.Enabled = false;
        Vcruntime.Enabled = false;
        Network.Enabled = false;
        Port.Enabled = false;
        Env.Enabled = false;
        Firewall.Enabled = false;
        FirewallRule.Enabled = false;
        Registry.Enabled = false;
        DefenderExclusion.Enabled = false;
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
            case "gpu": Gpu.Enabled = enabled; break;
            case "memory": Memory.Enabled = enabled; break;
            case "disk": Disk.Enabled = enabled; break;
            case "diskhealth": DiskHealth.Enabled = enabled; break;
            case "storage": Storage.Enabled = enabled; break;
            case "dotnet": Dotnet.Enabled = enabled; break;
            case "vcruntime": Vcruntime.Enabled = enabled; break;
            case "network": Network.Enabled = enabled; break;
            case "port": Port.Enabled = enabled; break;
            case "env": Env.Enabled = enabled; break;
            case "firewall": Firewall.Enabled = enabled; break;
            case "firewallrule": FirewallRule.Enabled = enabled; break;
            case "registry": Registry.Enabled = enabled; break;
            case "defender": DefenderExclusion.Enabled = enabled; break;
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

internal sealed class DiskHealthSection : SectionToggle
{
    public int EventLogHours { get; set; } = 168;
    public int MaxEventEntries { get; set; } = 5;
}

internal sealed class NetworkSection : SectionToggle
{
    public string DnsTestHost { get; set; } = "www.google.com";
    public string PingTestHost { get; set; } = "8.8.8.8";
    public bool ShowStaticRoutes { get; set; } = true;
}

internal sealed class PortSection : SectionToggle
{
    public List<int> Ports { get; set; } =
    [
        22, 25, 53, 80, 104, 123, 143, 161, 162, 443, 445, 500, 514,
        587, 902, 903, 993, 995, 1194, 1433, 1521, 1812, 1813,
        2375, 2376, 2575, 2761, 2762, 3000, 3306, 3389,
        4006, 4200, 4242, 4500,
        5000, 5001, 5432, 5433, 5672, 5985, 5986,
        6379, 6443,
        8006, 8042, 8080, 8443,
        9000, 9042, 9092, 9200, 10250,
        11112, 15672, 27017, 51820, 61616,
    ];

    public bool HideIfNotListening { get; set; } = true;
}

internal sealed class EnvSection : SectionToggle
{
    public List<string> Variables { get; set; } =
        ["PATH", "DOTNET_ROOT", "ASPNETCORE_ENVIRONMENT", "PGDATA", "PGHOST", "PGPORT", "PGUSER"];

    public bool ShowPathEntries { get; set; } = true;
    public bool HideIfNotSet { get; set; } = true;
    public bool HidePathNotExist { get; set; } = true;

    public List<string> RequiredPaths { get; set; } = [@"C:\my\tools", @"%USERPROFILE%\scoop\shims"];
    public bool HideRequiredPathIfNotFound { get; set; } = true;
    public bool HidePathEntriesIfRequired { get; set; } = true;
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
    public List<string> Commands { get; set; } = ["dotnet", "git", "code", "node", "npm", "docker", "python"];
    public List<string> Applications { get; set; } = [];
    public bool DetectPostgresVersions { get; set; } = false;
    public bool HideIfNotFound { get; set; } = true;
}

internal sealed class EventLogSection : SectionToggle
{
    public List<string> LogNames { get; set; } = ["Application", "System"];
    public int Hours { get; set; } = 24;
    public int MaxEntries { get; set; } = 5;
    public List<string> IgnoreSources { get; set; } =
    [
        "DCOM",
        "DistributedCOM",
        "Microsoft-Windows-DistributedCOM",
        "TPM-WMI",
        "Microsoft-Windows-TPM-WMI",
        "ACPI",
        "Microsoft-Windows-ACPI",
        "Microsoft-Windows-ACPI-StorFilter",
    ];
}

internal sealed class CertSection : SectionToggle
{
    public int WarningDays { get; set; } = 30;
}

internal sealed class FirewallRuleSection : SectionToggle
{
    public List<int> Ports { get; set; } =
    [
        80, 443, 3389,
        5432, 5433, 1433, 3306, 1521, 6379, 27017, 5672,
        104, 11112, 2575, 4242, 8042, 4006, 2761, 2762,
    ];
    public List<string> Programs { get; set; } = [];
    public bool CheckIcmp { get; set; } = true;
    public bool HideIfNotAllowed { get; set; } = true;
}

internal sealed class RegistrySection : SectionToggle
{
    public List<RegistryEntry> Entries { get; set; } = [];
    public bool HideIfNotFound { get; set; } = true;
}

internal sealed class RegistryEntry
{
    public string Path { get; set; } = string.Empty;
    public string? ValueName { get; set; }
    public string Label { get; set; } = string.Empty;
}

internal sealed class DefenderExclusionSection : SectionToggle
{
    public bool ShowRegistered { get; set; } = true;
    public List<string> Paths { get; set; } = [];
    public bool HideIfNotExcluded { get; set; } = true;
}
