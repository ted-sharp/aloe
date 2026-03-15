// <copyright file="ViewerLauncher.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace Aloe.Apps.Medock.MdPatientFinder.Services;

/// <summary>
/// Viewer/Editor の起動または NamedPipe での患者 ID 送信を行う。
/// </summary>
public class ViewerLauncher
{
    /// <summary>
    /// 指定されたパイプ名で接続を試行し、失敗した場合は対応する exe を起動する。
    /// </summary>
    public async Task LaunchAsync(string pipeName, Guid patientId)
    {
        var message = JsonSerializer.Serialize(new { action = "open", patientId = patientId.ToString() });

        if (await TrySendViaPipeAsync(pipeName, message))
        {
            return;
        }

        var exeName = pipeName switch
        {
            "MdPatient_Viewer" => "Aloe.Apps.Medock.MdPatientViewer.exe",
            "MdPatient_Editor" => "Aloe.Apps.Medock.MdPatientEditor.exe",
            _ => throw new ArgumentException($"不明なパイプ名: {pipeName}"),
        };

        var exePath = Path.Combine(AppContext.BaseDirectory, exeName);
        Process.Start(new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = $"--patient-id {patientId}",
            UseShellExecute = false,
        });
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
