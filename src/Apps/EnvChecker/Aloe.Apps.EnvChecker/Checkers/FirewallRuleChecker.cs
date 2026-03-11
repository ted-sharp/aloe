using Microsoft.Win32;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class FirewallRuleChecker : IChecker
{
    private const string FirewallRulesPath =
        @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\FirewallRules";

    public string SectionKey => "firewallrule";

    public string SectionTitle => "Firewall Rules";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        var config = profile.FirewallRule;
        var rules = LoadFirewallRules();

        if (config.CheckIcmp)
        {
            WriteIcmpRules(writer, config, rules);
        }

        if (config.Ports.Count > 0)
        {
            WritePortRules(writer, config, rules);
        }

        if (config.Programs.Count > 0)
        {
            WriteProgramRules(writer, config, rules);
        }

        return Task.CompletedTask;
    }

    private static void WriteIcmpRules(TextWriter writer, FirewallRuleSection config, List<FirewallRule> rules)
    {
        // ICMPv4 = Protocol 1, ICMPv6 = Protocol 58
        var icmpV4Allow = rules
            .Where(r => r.Direction == "In" && r.Action == "Allow" && r.Active && r.Protocol == "1")
            .ToList();
        var icmpV4Block = rules
            .Where(r => r.Direction == "In" && r.Action == "Block" && r.Active && r.Protocol == "1")
            .ToList();
        var icmpV6Allow = rules
            .Where(r => r.Direction == "In" && r.Action == "Allow" && r.Active && r.Protocol == "58")
            .ToList();
        var icmpV6Block = rules
            .Where(r => r.Direction == "In" && r.Action == "Block" && r.Active && r.Protocol == "58")
            .ToList();

        WriteIcmpProtocol(writer, "ICMPv4", icmpV4Allow, icmpV4Block, config.HideIfNotAllowed);
        WriteIcmpProtocol(writer, "ICMPv6", icmpV6Allow, icmpV6Block, config.HideIfNotAllowed);
    }

    private static void WriteIcmpProtocol(TextWriter writer, string label, List<FirewallRule> allow, List<FirewallRule> block, bool hideIfNotAllowed)
    {
        if (allow.Count == 0 && hideIfNotAllowed)
        {
            return;
        }

        if (allow.Count > 0)
        {
            var blockNote = block.Count > 0 ? $", {block.Count} block" : string.Empty;
            writer.WriteLine($"  {label,-28} [ALLOWED] ({allow.Count} allow{blockNote})");
            foreach (var rule in allow.Take(3))
            {
                writer.WriteLine($"    - {rule.Name}");
            }
        }
        else
        {
            writer.WriteLine($"  {label,-28} [NO RULE]");
        }
    }

    private static void WritePortRules(TextWriter writer, FirewallRuleSection config, List<FirewallRule> rules)
    {
        foreach (var port in config.Ports)
        {
            var matchingRules = rules
                .Where(r => r.Direction == "In"
                    && r.Action == "Allow"
                    && r.Active
                    && r.LocalPorts.Contains(port))
                .ToList();

            if (matchingRules.Count == 0 && config.HideIfNotAllowed)
            {
                continue;
            }

            var label = GetPortLabel(port);
            var portDisplay = string.IsNullOrEmpty(label) ? $"{port}" : $"{port} ({label})";

            if (matchingRules.Count > 0)
            {
                writer.WriteLine($"  {portDisplay,-28} [ALLOWED] ({matchingRules.Count} rule(s))");
                foreach (var rule in matchingRules.Take(3))
                {
                    writer.WriteLine($"    - {rule.Name}");
                }
            }
            else
            {
                writer.WriteLine($"  {portDisplay,-28} [NO RULE]");
            }
        }
    }

    private static void WriteProgramRules(TextWriter writer, FirewallRuleSection config, List<FirewallRule> rules)
    {
        foreach (var program in config.Programs)
        {
            var matchingRules = rules
                .Where(r => r.Direction == "In"
                    && r.Action == "Allow"
                    && r.Active
                    && r.AppPath is not null
                    && r.AppPath.Contains(program, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingRules.Count == 0 && config.HideIfNotAllowed)
            {
                continue;
            }

            var programName = Path.GetFileName(program);

            if (matchingRules.Count > 0)
            {
                writer.WriteLine($"  {programName,-28} [ALLOWED] ({matchingRules.Count} rule(s))");
                foreach (var rule in matchingRules.Take(3))
                {
                    writer.WriteLine($"    - {rule.Name}");
                }
            }
            else
            {
                writer.WriteLine($"  {programName,-28} [NO RULE]");
            }
        }
    }

    private static List<FirewallRule> LoadFirewallRules()
    {
        var result = new List<FirewallRule>();

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(FirewallRulesPath);
            if (key is null)
            {
                return result;
            }

            foreach (var valueName in key.GetValueNames())
            {
                var ruleString = key.GetValue(valueName)?.ToString();
                if (ruleString is null)
                {
                    continue;
                }

                var rule = ParseRule(ruleString);
                if (rule is not null)
                {
                    result.Add(rule);
                }
            }
        }
        catch
        {
            // Cannot read firewall rules from registry
        }

        return result;
    }

    private static FirewallRule? ParseRule(string ruleString)
    {
        var rule = new FirewallRule();
        var segments = ruleString.Split('|');

        foreach (var segment in segments)
        {
            var eqIndex = segment.IndexOf('=');
            if (eqIndex < 0)
            {
                continue;
            }

            var fieldName = segment[..eqIndex];
            var fieldValue = segment[(eqIndex + 1)..];

            switch (fieldName)
            {
                case "Action":
                    rule.Action = fieldValue;
                    break;
                case "Active":
                    rule.Active = string.Equals(fieldValue, "TRUE", StringComparison.OrdinalIgnoreCase);
                    break;
                case "Dir":
                    rule.Direction = fieldValue;
                    break;
                case "Protocol":
                    // 6 = TCP, 17 = UDP
                    rule.Protocol = fieldValue;
                    break;
                case "LPort":
                    rule.LocalPorts = ParsePorts(fieldValue);
                    break;
                case "App":
                    rule.AppPath = fieldValue;
                    break;
                case "Name":
                    rule.Name = fieldValue;
                    break;
            }
        }

        return rule.Action is not null ? rule : null;
    }

    private static HashSet<int> ParsePorts(string portString)
    {
        var ports = new HashSet<int>();
        foreach (var part in portString.Split(','))
        {
            if (int.TryParse(part.Trim(), out var port))
            {
                ports.Add(port);
            }
        }

        return ports;
    }

    private static string GetPortLabel(int port) => port switch
    {
        80 => "HTTP",
        443 => "HTTPS",
        104 => "DICOM",
        2575 => "HL7",
        2761 => "DICOM TLS",
        2762 => "DICOM TLS Alt",
        3389 => "RDP",
        5432 => "PostgreSQL",
        5433 => "PostgreSQL Alt",
        1433 => "SQL Server",
        1521 => "Oracle",
        3306 => "MySQL",
        6379 => "Redis",
        27017 => "MongoDB",
        5672 => "RabbitMQ",
        4242 => "Orthanc DICOM",
        11112 => "DICOM Alt",
        4006 => "DICOM Alt",
        8042 => "Orthanc REST",
        _ => string.Empty,
    };

    private sealed class FirewallRule
    {
        public string? Action { get; set; }
        public bool Active { get; set; }
        public string Direction { get; set; } = string.Empty;
        public string? Protocol { get; set; }
        public HashSet<int> LocalPorts { get; set; } = [];
        public string? AppPath { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
