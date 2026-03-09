using System.Management;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class HardwareChecker : IChecker
{
    public string SectionKey => "cpu";

    public string SectionTitle => "CPU";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        using var searcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed FROM Win32_Processor");
        foreach (var obj in searcher.Get())
        {
            writer.WriteLine($"  {"Model",-20}: {obj["Name"]}");
            writer.WriteLine($"  {"Cores",-20}: {obj["NumberOfCores"]} ({obj["NumberOfLogicalProcessors"]} logical)");
            writer.WriteLine($"  {"Max Clock",-20}: {obj["MaxClockSpeed"]} MHz");
        }

        return Task.CompletedTask;
    }
}
