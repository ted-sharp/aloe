using System.ComponentModel;
using System.Diagnostics;
using Aloe.Common.AloeCoreLib.Util;
using Microsoft.Extensions.Logging;

// CLSに準拠してない例外を考慮のため除外
#pragma warning disable CS1058 // A previous catch clause already catches all exceptions

namespace Aloe.Common.AloeCoreLib.Win32;

/// <summary>
/// sc.exe (Service Controller) を実行します。
/// </summary>
public static class Sc
{
    public static bool CreateService(
        string serviceName,
        string servicePath,
        string? description = null,
        string startType = "auto",
        string account = "LocalSystem",
        string? dependencies = null,
        int failureResets = 0,
        string? failureActions = null,
        ILogger? logger = null)
    {
        var fullPath = PathHelper.FromBase(servicePath);
        if (String.IsNullOrWhiteSpace(servicePath) || !File.Exists(fullPath))
        {
            logger?.LogError($"Not found: {servicePath}");
            return false;
        }

        var args = new List<string>
        {
            "create",
            serviceName,
            $"binPath= \"{fullPath}\"",
            $"start= {startType}",
            $"obj= \"{account}\"",
        };

        if (!String.IsNullOrWhiteSpace(dependencies))
        {
            args.Add($"depend= \"{dependencies}\"");
        }

        var success = Sc.RunScCommand(String.Join(" ", args), logger);

        if (success && !String.IsNullOrWhiteSpace(description))
        {
            success = Sc.RunScCommand($"description {serviceName} \"{description}\"", logger);
        }

        if (success && failureResets > 0 && !String.IsNullOrWhiteSpace(failureActions))
        {
            Sc.RunScCommand($"failure {serviceName} reset= \"{failureResets}\" actions= \"{failureActions}\"", logger);
        }

        return success;
    }

    public static bool DeleteService(string serviceName, ILogger? logger = null)
    {
        return Sc.RunScCommand($"delete {serviceName}", logger);
    }

    private static bool RunScCommand(string arguments, ILogger? logger)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = arguments,
                UseShellExecute = true,
                Verb = "runas",
                CreateNoWindow = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
            };

            using var proc = Process.Start(psi);
            if (proc is not null)
            {
                proc.WaitForExit();
                if (proc.ExitCode == 0)
                {
                    logger?.LogInformation($"Success (sc.exe {arguments})");
                    return true;
                }
                else
                {
                    var win32Ex = new Win32Exception(proc.ExitCode);
                    logger?.LogInformation($"Failure (ExitCode {proc.ExitCode}: {win32Ex.Message})(sc.exe {arguments})");
                }
            }
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            logger?.LogWarning($"Canceled (ExitCode {ex.NativeErrorCode}: {ex.Message})");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, $"Exception (sc.exe {arguments})");
        }
        catch
        {
            logger?.LogError($"Exception (sc.exe {arguments})");
        }

        return false;
    }
}
