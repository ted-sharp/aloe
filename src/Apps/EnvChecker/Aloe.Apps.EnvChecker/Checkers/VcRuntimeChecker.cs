using Microsoft.Win32;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class VcRuntimeChecker : IChecker
{
    public string SectionKey => "vcruntime";

    public string SectionTitle => "Visual C++ Redistributable";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        var found = false;

        foreach (var arch in new[] { "x64", "x86", "ARM64" })
        {
            var keyPath = $@"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\{arch}";
            using var key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key is null)
            {
                continue;
            }

            var major = key.GetValue("Major");
            var minor = key.GetValue("Minor");
            var bld = key.GetValue("Bld");
            var version = $"{major}.{minor}.{bld}";

            writer.WriteLine($"  VC++ 2015-2022 Redistributable ({arch}) : {version}");
            found = true;
        }

        SearchUninstallKeys(writer, ref found);

        if (!found)
        {
            writer.WriteLine("  No Visual C++ Redistributable found");
        }

        return Task.CompletedTask;
    }

    private static void SearchUninstallKeys(TextWriter writer, ref bool found)
    {
        var uninstallPaths = new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
        };

        var reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in uninstallPaths)
        {
            using var baseKey = Registry.LocalMachine.OpenSubKey(path);
            if (baseKey is null)
            {
                continue;
            }

            foreach (var subKeyName in baseKey.GetSubKeyNames())
            {
                using var subKey = baseKey.OpenSubKey(subKeyName);
                var displayName = subKey?.GetValue("DisplayName") as string;
                if (displayName is null ||
                    !displayName.Contains("Visual C++", StringComparison.OrdinalIgnoreCase) ||
                    !displayName.Contains("Redistributable", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!reported.Add(displayName))
                {
                    continue;
                }

                var displayVersion = subKey?.GetValue("DisplayVersion") as string ?? "?";
                writer.WriteLine($"  {displayName} : {displayVersion}");
                found = true;
            }
        }
    }
}
