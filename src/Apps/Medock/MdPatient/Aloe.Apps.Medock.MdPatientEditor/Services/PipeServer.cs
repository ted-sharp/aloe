// <copyright file="PipeServer.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace Aloe.Apps.Medock.MdPatientEditor.Services;

/// <summary>
/// NamedPipe サーバー。Finder からの患者 ID 受信を待ち受ける。
/// </summary>
public class PipeServer : IDisposable
{
    private readonly string _pipeName;
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;

    /// <summary>患者 ID を受信したときに発火するイベント。</summary>
    public event Action<Guid>? PatientIdReceived;

    /// <summary>
    /// <see cref="PipeServer"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public PipeServer(string pipeName)
    {
        _pipeName = pipeName;
    }

    /// <summary>パイプサーバーの待ち受けを開始する。</summary>
    public void Start()
    {
        _ = ListenAsync(_cts.Token);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    private async Task ListenAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var server = new NamedPipeServerStream(_pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await server.WaitForConnectionAsync(cancellationToken);

                using var reader = new StreamReader(server, Encoding.UTF8);
                var json = await reader.ReadToEndAsync(cancellationToken);

                if (!string.IsNullOrEmpty(json))
                {
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("patientId", out var idProp) &&
                        Guid.TryParse(idProp.GetString(), out var patientId))
                    {
                        PatientIdReceived?.Invoke(patientId);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (IOException)
            {
                // パイプ接続エラーは無視してリトライ
            }
        }
    }
}
