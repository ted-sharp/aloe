namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class EnvironmentVariableChecker : IChecker
{
    public string SectionKey => "env";

    public string SectionTitle => "Environment Variables";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        var envConfig = profile.Env;
        var pathValue = Environment.GetEnvironmentVariable("PATH");

        foreach (var varName in envConfig.Variables)
        {
            var value = Environment.GetEnvironmentVariable(varName);

            if (value is null && envConfig.HideIfNotSet)
            {
                continue;
            }

            if (string.Equals(varName, "PATH", StringComparison.OrdinalIgnoreCase) && value is not null)
            {
                var showEntries = envConfig.ShowPathEntries
                    && !(envConfig.RequiredPaths.Count > 0 && envConfig.HidePathEntriesIfRequired);
                WritePath(writer, value, showEntries, envConfig.HidePathNotExist);
            }
            else
            {
                writer.WriteLine($"  {varName,-28}: {value ?? "(not set)"}");
            }
        }

        if (envConfig.RequiredPaths.Count > 0 && pathValue is not null)
        {
            WriteRequiredPaths(writer, pathValue, envConfig.RequiredPaths, envConfig.HideRequiredPathIfNotFound);
        }

        return Task.CompletedTask;
    }

    private static void WritePath(TextWriter writer, string pathValue, bool showEntries, bool hideNotExist)
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

            if (!exists && hideNotExist)
            {
                continue;
            }

            var status = exists ? "OK" : "NOT FOUND";
            writer.WriteLine($"    {trimmed,-52} [{status}]");
        }
    }

    private static void WriteRequiredPaths(TextWriter writer, string pathValue, List<string> requiredPaths, bool hideIfNotFound)
    {
        var entries = pathValue
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => Path.GetFullPath(e.Trim()).TrimEnd('\\'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        writer.WriteLine($"  {"PATH Required Entries",-28}:");

        foreach (var required in requiredPaths)
        {
            var expanded = Environment.ExpandEnvironmentVariables(required);
            var normalized = Path.GetFullPath(expanded).TrimEnd('\\');
            var found = entries.Contains(normalized);

            if (!found && hideIfNotFound)
            {
                continue;
            }

            var status = found ? "IN PATH" : "NOT IN PATH";
            writer.WriteLine($"    {expanded,-52} [{status}]");
        }
    }
}
