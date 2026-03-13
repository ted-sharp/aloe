// <copyright file="PrintJobStore.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using Aloe.Apps.ExcelReportApi.Models;

namespace Aloe.Apps.ExcelReportApi.Services;

/// <summary>
/// 印刷ジョブ情報をメモリ上で管理するシングルトンストア。
/// </summary>
public class PrintJobStore
{
    private readonly ConcurrentDictionary<Guid, PrintJobInfo> _jobs = new();

    /// <summary>
    /// 新しいジョブを追加し、Queued 状態で返す。
    /// </summary>
    public PrintJobInfo Add(Guid jobId)
    {
        var info = new PrintJobInfo
        {
            JobId = jobId,
            Status = JobStatus.Queued,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        this._jobs[jobId] = info;
        return info;
    }

    /// <summary>
    /// 指定ジョブの状態を取得する。存在しない場合は null。
    /// </summary>
    public PrintJobInfo? Get(Guid jobId)
    {
        this._jobs.TryGetValue(jobId, out var info);
        return info;
    }

    /// <summary>
    /// 指定ジョブを Processing 状態に更新する。
    /// </summary>
    public void SetProcessing(Guid jobId)
    {
        if (this._jobs.TryGetValue(jobId, out var info))
        {
            info.Status = JobStatus.Processing;
        }
    }

    /// <summary>
    /// 指定ジョブを Completed 状態に更新し、PDFパスを設定する。
    /// </summary>
    public void SetCompleted(Guid jobId, string pdfFilePath)
    {
        if (this._jobs.TryGetValue(jobId, out var info))
        {
            info.Status = JobStatus.Completed;
            info.PdfFilePath = pdfFilePath;
        }
    }

    /// <summary>
    /// 指定ジョブを Failed 状態に更新し、エラーメッセージを設定する。
    /// </summary>
    public void SetFailed(Guid jobId, string errorMessage)
    {
        if (this._jobs.TryGetValue(jobId, out var info))
        {
            info.Status = JobStatus.Failed;
            info.ErrorMessage = errorMessage;
        }
    }
}
