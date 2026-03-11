using Microsoft.Win32;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class RegistryChecker : IChecker
{
    public string SectionKey => "registry";

    public string SectionTitle => "Registry Check";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        var config = profile.Registry;

        foreach (var entry in config.Entries)
        {
            var (found, value) = ReadRegistryEntry(entry);

            if (!found && config.HideIfNotFound)
            {
                continue;
            }

            var label = string.IsNullOrEmpty(entry.Label) ? entry.Path : entry.Label;
            var status = found ? "FOUND" : "NOT FOUND";

            if (found && value is not null)
            {
                writer.WriteLine($"  {label,-36} [{status}] = {value}");
            }
            else
            {
                writer.WriteLine($"  {label,-36} [{status}]");
            }
        }

        return Task.CompletedTask;
    }

    private static (bool Found, string? Value) ReadRegistryEntry(RegistryEntry entry)
    {
        try
        {
            var (hive, subKeyPath) = ParseRegistryPath(entry.Path);
            if (hive is null)
            {
                return (false, null);
            }

            using var key = hive.OpenSubKey(subKeyPath);
            if (key is null)
            {
                return (false, null);
            }

            if (entry.ValueName is null)
            {
                return (true, null);
            }

            var value = key.GetValue(entry.ValueName);
            return value is not null ? (true, value.ToString()) : (false, null);
        }
        catch
        {
            return (false, null);
        }
    }

    private static (RegistryKey? Hive, string SubKeyPath) ParseRegistryPath(string path)
    {
        var separatorIndex = path.IndexOf('\\');
        if (separatorIndex < 0)
        {
            return (null, string.Empty);
        }

        var hiveName = path[..separatorIndex].ToUpperInvariant();
        var subKeyPath = path[(separatorIndex + 1)..];

        var hive = hiveName switch
        {
            "HKLM" or "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
            "HKCU" or "HKEY_CURRENT_USER" => Registry.CurrentUser,
            "HKCR" or "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
            "HKU" or "HKEY_USERS" => Registry.Users,
            _ => null,
        };

        return (hive, subKeyPath);
    }
}
