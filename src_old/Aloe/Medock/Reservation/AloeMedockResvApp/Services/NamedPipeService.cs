using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Services;

public class NamedPipeService : IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// 既存インスタンスからの引数受信イベント
    /// </summary>
    public event Action<string[]?>? ArgumentsReceived;

    /// <summary>
    /// 受信サーバーを開始します（最初のインスタンスのみ呼ぶ）
    /// </summary>
    public void Listen(string pipeName)
    {
        _ = Task.Run(async () =>
        {
            while (!this._cts.IsCancellationRequested)
            {
                await using var server = new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous);

                // 接続を待ち受け
                await server
                    .WaitForConnectionAsync(this._cts.Token)
                    .ConfigureAwait(false);

                // 内容を受信
                using var reader = new StreamReader(server);
                var raw = await reader
                    .ReadToEndAsync()
                    .ConfigureAwait(false);
                var args = raw.Split('|', StringSplitOptions.RemoveEmptyEntries);

                // UI スレッドで通知
                Application.Current.Dispatcher.Invoke(() =>
                    this.ArgumentsReceived?.Invoke(args));
            }
        });
    }

    /// <summary>
    /// 既存インスタンスへ引数を送信します（2回目以降の起動プロセスで呼ぶ）
    /// </summary>
    public static void Send(string pipeName, string[] args)
    {
        using var client = new NamedPipeClientStream(
            ".",
            pipeName,
            PipeDirection.Out);
        client.Connect(1_000);
        using var writer = new StreamWriter(client);
        writer.AutoFlush = true;
        writer.Write(String.Join("|", args));
    }

    public void Dispose()
    {
        this._cts.Cancel();
        this._cts.Dispose();
        this.ArgumentsReceived = null;
    }
}
