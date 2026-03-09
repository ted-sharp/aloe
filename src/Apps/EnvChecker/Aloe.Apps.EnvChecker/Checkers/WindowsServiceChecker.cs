using System.ServiceProcess;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class WindowsServiceChecker : IChecker
{
    public string SectionKey => "service";

    public string SectionTitle => "Windows Services";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        foreach (var pattern in profile.Service.Services)
        {
            var resolved = ResolveServiceNames(pattern);

            if (resolved.Length == 0)
            {
                if (!profile.Service.HideIfNotInstalled)
                {
                    writer.WriteLine($"  {pattern,-36} [NOT INSTALLED]");
                }
                continue;
            }

            foreach (var serviceName in resolved)
            {
                WriteServiceStatus(writer, serviceName);
            }
        }

        return Task.CompletedTask;
    }

    private static string[] ResolveServiceNames(string pattern)
    {
        if (!pattern.EndsWith('*'))
        {
            try
            {
                using var sc = new ServiceController(pattern);
                _ = sc.Status;
                return [pattern];
            }
            catch (InvalidOperationException)
            {
                return [];
            }
        }

        var prefix = pattern[..^1];
        return ServiceController.GetServices()
            .Where(s => s.ServiceName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.ServiceName, StringComparer.OrdinalIgnoreCase)
            .Select(s => s.ServiceName)
            .ToArray();
    }

    private static void WriteServiceStatus(TextWriter writer, string serviceName)
    {
        try
        {
            using var sc = new ServiceController(serviceName);
            writer.WriteLine($"  {serviceName,-36} [{sc.Status}]  ({sc.DisplayName})");
        }
        catch (InvalidOperationException)
        {
            writer.WriteLine($"  {serviceName,-36} [NOT INSTALLED]");
        }
    }
}
