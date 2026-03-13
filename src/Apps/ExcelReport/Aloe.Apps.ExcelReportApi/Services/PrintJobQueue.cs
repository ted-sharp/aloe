// <copyright file="PrintJobQueue.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Threading.Channels;

namespace Aloe.Apps.ExcelReportApi.Services;

/// <summary>
/// 印刷ジョブのリクエスト情報。
/// </summary>
public record PrintJobRequest(
    Guid JobId,
    byte[] ExcelBytes,
    IReadOnlyDictionary<string, string>? Variables,
    int SheetIndex,
    string PrinterName,
    int Copies);

/// <summary>
/// 印刷ジョブを非同期に受け渡す Channel ラッパー。
/// </summary>
public class PrintJobQueue
{
    private readonly Channel<PrintJobRequest> _channel = Channel.CreateUnbounded<PrintJobRequest>();

    /// <summary>
    /// ジョブをキューに追加する。
    /// </summary>
    public void Enqueue(PrintJobRequest request) => this._channel.Writer.TryWrite(request);

    /// <summary>
    /// キューからジョブを取り出す（非同期）。
    /// </summary>
    public ValueTask<PrintJobRequest> DequeueAsync(CancellationToken ct) =>
        this._channel.Reader.ReadAsync(ct);
}
