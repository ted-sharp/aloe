using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Logging;

/// <summary>
/// バッファとタイマーがあるカスタムシンクです。
/// 内部ロガーを経由して他のSinkへ出力します。
/// </summary>
public class BufferingSink : ILogEventSink, IDisposable
{
    private readonly BufferingSinkOptions _options;
    private readonly Serilog.ILogger _innerLogger;

    private readonly ConcurrentQueue<LogEvent> _logBuffer = new();
    private readonly Timer? _flushTimer;
    private int _isFlushing;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public BufferingSink(
        Serilog.ILogger innerLogger,
        BufferingSinkOptions options)
    {
        ArgumentNullException.ThrowIfNull(innerLogger, nameof(innerLogger));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentOutOfRangeException.ThrowIfLessThan(options.BatchSize, 100, nameof(options.BatchSize));

        this._innerLogger = innerLogger;
        this._options = options;

        if (options.FlushInterval > TimeSpan.Zero)
        {
            // タイマーで定期的にバッファをフラッシュ
            this._flushTimer = new Timer(
                _ => this.TriggerFlush(),
                null,
                options.FlushInterval,
                options.FlushInterval);
        }
    }

    private void TriggerFlush()
    {
        if (Interlocked.Exchange(ref this._isFlushing, 1) == 1)
        {
            // フラッシュ中の場合は新しいタスクをスケジュールしない
            return;
        }

        _ = this.FlushBufferAsync()
            .ContinueWith(_ => Interlocked.Exchange(ref this._isFlushing, 0));
    }

    private async Task FlushBufferAsync()
    {
        try
        {
            await this._semaphore.WaitAsync();

            if (this._logBuffer.IsEmpty)
            {
                return;
            }

            var count = Math.Min(this._logBuffer.Count, this._options.BatchSize);
            var batch = new List<LogEvent>(count);

            var max = this._options.BatchSize;
            while (batch.Count < max && this._logBuffer.TryDequeue(out var logEvent))
            {
                batch.Add(logEvent);
            }

            if (batch.Count == 1)
            {
                this._innerLogger.Write(batch[0]);
            }
            else if (batch.Count > 0)
            {
                await Task.Run(() =>
                {
                    foreach (var item in batch)
                    {
                        this._innerLogger.Write(item);
                    }
                });
            }
        }
        finally
        {
            this._semaphore.Release();

        }
    }

    #region ILogEventSink

    public void Emit(LogEvent logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent, nameof(logEvent));

        this._logBuffer.Enqueue(logEvent);

        // バッファサイズが一定数を超えたらフラッシュ
        if (this._logBuffer.Count >= this._options.BatchSize)
        {
            this.TriggerFlush();
        }

        // 最初のイベントを即座に送信する設定
        if (this._options.EagerlyEmitFirstEvent && this._logBuffer.Count == 1)
        {
            this.TriggerFlush();
        }
    }

    #endregion ILogEventSink

    #region IDisposable

    public void Dispose()
    {
        this._flushTimer?.Dispose();

        // Dispose時に残りのログを出力
        Task.Run(this.FlushBufferAsync).GetAwaiter().GetResult();

        // 実際のロガーも後始末する
        if (this._innerLogger is IDisposable disposableLogger)
        {
            disposableLogger.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    #endregion IDisposable
}
