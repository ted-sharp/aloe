namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class EnvironmentVariableChecker : IChecker
{
    public string SectionKey => "env";

    public string SectionTitle => "Environment Variables";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        var envConfig = profile.Env;

        foreach (var varName in envConfig.Variables)
        {
            var value = Environment.GetEnvironmentVariable(varName);

            if (value is null && envConfig.HideIfNotSet)
            {
                continue;
            }

            if (string.Equals(varName, "PATH", StringComparison.OrdinalIgnoreCase) && value is not null)
            {
                WritePath(writer, value, envConfig.ShowPathEntries);
            }
            else
            {
                writer.WriteLine($"  {varName,-28}: {value ?? "(not set)"}");
            }
        }

        return Task.CompletedTask;
    }

    private static void WritePath(TextWriter writer, string pathValue, bool showEntries)
    {
        var entries = pathValue.Split(';', StringSplitOptions.RemoveEmptyEntries);
        writer.WriteLine($"  {"PATH",-28}: ({entries.Length} entries)");

        if (!showEntries)
        {
            return;
        }

        foreach (var entry in entries)
        {
            var trimmed = entry.Trim();
            var exists = Directory.Exists(trimmed);
            var status = exists ? "OK" : "NOT FOUND";
            writer.WriteLine($"    {trimmed,-52} [{status}]");
        }
    }
}
