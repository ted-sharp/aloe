// <copyright file="ReportJobQueue.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Threading.Channels;

namespace Aloe.Apps.ExcelReportApi.Services;

/// <summary>
/// PDF生成ジョブのリクエスト情報。
/// </summary>
public record JobRequest(
    Guid JobId,
    byte[] ExcelBytes,
    IReadOnlyDictionary<string, string>? Variables,
    int SheetIndex);

/// <summary>
/// PDF生成ジョブを非同期に受け渡す Channel ラッパー。
/// </summary>
public class ReportJobQueue
{
    private readonly Channel<JobRequest> _channel = Channel.CreateUnbounded<JobRequest>();

    /// <summary>
    /// ジョブをキューに追加する。
    /// </summary>
    public void Enqueue(JobRequest request) => this._channel.Writer.TryWrite(request);

    /// <summary>
    /// キューからジョブを取り出す（非同期）。
    /// </summary>
    public ValueTask<JobRequest> DequeueAsync(CancellationToken ct) =>
        this._channel.Reader.ReadAsync(ct);
}
