using System.ServiceProcess;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class WindowsServiceChecker : IChecker
{
    public string SectionKey => "service";

    public string SectionTitle => "Windows Services";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        foreach (var serviceName in profile.Service.Services)
        {
            try
            {
                using var sc = new ServiceController(serviceName);
                var displayName = sc.DisplayName;
                var status = sc.Status;
                writer.WriteLine($"  {serviceName,-36} [{status}]  ({displayName})");
            }
            catch (InvalidOperationException)
            {
                writer.WriteLine($"  {serviceName,-36} [NOT INSTALLED]");
            }
        }

        return Task.CompletedTask;
    }
}
