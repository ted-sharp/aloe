// <copyright file="AppLauncher.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Aloe.Apps.Medock.MdLauncherLib.Contracts.Services;

namespace Aloe.Apps.Medock.MdLauncher.Services;

/// <summary>
/// アプリケーションの起動を行う。NamedPipe でフォーカス要求を試行し、失敗時は exe を起動する。
/// </summary>
public class AppLauncher
{
    /// <summary>
    /// アプリを起動する。
    /// </summary>
    public async Task LaunchAsync(LauncherAppItem app)
    {
        if (!string.IsNullOrEmpty(app.PipeName))
        {
            var message = JsonSerializer.Serialize(new { action = "focus" });
            if (await TrySendViaPipeAsync(app.PipeName, message))
            {
                return;
            }
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = app.ExePath,
            Arguments = app.Arguments,
            UseShellExecute = false,
        };

        if (!string.IsNullOrEmpty(app.WorkingDirectory))
        {
            startInfo.WorkingDirectory = app.WorkingDirectory;
        }

        Process.Start(startInfo);
    }

    private static async Task<bool> TrySendViaPipeAsync(string pipeName, string message)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            await client.ConnectAsync(500);

            var bytes = Encoding.UTF8.GetBytes(message);
            await client.WriteAsync(bytes);
            await client.FlushAsync();

            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }
}
