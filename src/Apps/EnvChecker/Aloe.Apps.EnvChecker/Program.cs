using System.CommandLine;
using System.Reflection;
using Aloe.Apps.EnvChecker;
using Aloe.Apps.EnvChecker.Checkers;

Option<FileInfo?> profileOption = new("--profile")
{
    Description = "JSON profile file for detailed check configuration",
};

Option<string?> onlyOption = new("--only")
{
    Description = "Comma-separated section keys to run (e.g. system,disk,port)",
};

Option<string?> excludeOption = new("--exclude")
{
    Description = "Comma-separated section keys to exclude (e.g. eventlog,cert)",
};

Option<bool> showAllOption = new("--show-all")
{
    Description = "Show all items including missing/not found (default: hide missing)",
};

Option<bool> initOption = new("--init")
{
    Description = "Generate a sample profile JSON to stdout",
};

RootCommand rootCommand = new("Aloe Environment Checker - diagnose system environment for troubleshooting")
{
    profileOption,
    onlyOption,
    excludeOption,
    showAllOption,
    initOption,
};

rootCommand.SetAction(async parseResult =>
{
    var profileFile = parseResult.GetValue(profileOption);
    var only = parseResult.GetValue(onlyOption);
    var exclude = parseResult.GetValue(excludeOption);
    var showAll = parseResult.GetValue(showAllOption);
    var init = parseResult.GetValue(initOption);

    if (init)
    {
        Console.WriteLine(CheckProfile.GenerateSampleJson());
        return 0;
    }

    CheckProfile profile;
    if (profileFile is not null)
    {
        if (!profileFile.Exists)
        {
            Console.Error.WriteLine($"Profile file not found: {profileFile.FullName}");
            return 1;
        }

        profile = CheckProfile.LoadFromFile(profileFile.FullName);
    }
    else
    {
        profile = CheckProfile.CreateAllEnabled();
    }

    if (only is not null)
    {
        var sections = only.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        profile.EnableOnly(sections);
    }
    else if (exclude is not null)
    {
        var sections = exclude.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        profile.Exclude(sections);
    }

    if (showAll)
    {
        profile.ShowAll();
    }

    var checkers = CreateCheckers();
    var writer = Console.Out;

    var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
    var now = DateTimeOffset.Now;

    await writer.WriteLineAsync("========================================");
    await writer.WriteLineAsync($"  Aloe Environment Checker v{version}");
    await writer.WriteLineAsync($"  Date: {now:yyyy-MM-dd HH:mm:ss zzz}");
    await writer.WriteLineAsync("========================================");

    foreach (var checker in checkers)
    {
        if (!profile.IsSectionEnabled(checker.SectionKey))
        {
            continue;
        }

        await writer.WriteLineAsync();
        await writer.WriteLineAsync($"[{checker.SectionTitle}]");

        try
        {
            await checker.RunAsync(writer, profile);
        }
        catch (Exception ex)
        {
            await writer.WriteLineAsync($"  [ERROR] {ex.GetType().Name}: {ex.Message}");
        }
    }

    await writer.WriteLineAsync();
    return 0;
});

return rootCommand.Parse(args).Invoke();

static IChecker[] CreateCheckers() =>
[
    new SystemInfoChecker(),
    new HardwareChecker(),
    new GpuChecker(),
    new MemoryChecker(),
    new DiskChecker(),
    new DiskHealthChecker(),
    new StorageChecker(),
    new DotNetRuntimeChecker(),
    new VcRuntimeChecker(),
    new NetworkChecker(),
    new PortChecker(),
    new EnvironmentVariableChecker(),
    new FirewallChecker(),
    new FirewallRuleChecker(),
    new RegistryChecker(),
    new DefenderExclusionChecker(),
    new WindowsServiceChecker(),
    new InstalledSoftwareChecker(),
    new EventLogChecker(),
    new CertificateChecker(),
];
