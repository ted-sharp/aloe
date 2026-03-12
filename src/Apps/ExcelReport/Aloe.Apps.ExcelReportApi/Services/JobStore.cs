// <copyright file="JobStore.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using Aloe.Apps.ExcelReportApi.Models;
using Aloe.Apps.ExcelReportApi.Options;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.ExcelReportApi.Services;

/// <summary>
/// ジョブ情報をメモリ上で管理するシングルトンストア。
/// 期限切れジョブと一時PDFファイルを自動削除する。
/// </summary>
public class JobStore
{
    private readonly ConcurrentDictionary<Guid, JobInfo> _jobs = new();
    private readonly ExcelReportApiOptions _options;

    /// <summary>
    /// <see cref="JobStore"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    /// <param name="options">APIオプション。</param>
    public JobStore(IOptions<ExcelReportApiOptions> options)
    {
        this._options = options.Value;
    }

    /// <summary>
    /// 新しいジョブを追加し、Queued 状態で返す。
    /// </summary>
    public JobInfo Add(Guid jobId)
    {
        var info = new JobInfo
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
    public JobInfo? Get(Guid jobId)
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

    /// <summary>
    /// 全ジョブを返す。
    /// </summary>
    public IReadOnlyList<JobInfo> GetAll() => [.. this._jobs.Values];

    /// <summary>
    /// 期限切れジョブを削除し、関連する一時PDFファイルも削除する。
    /// </summary>
    public void PurgeExpired()
    {
        var cutoff = DateTimeOffset.UtcNow.AddMinutes(-this._options.MaxJobAgeMins);
        foreach (var kvp in this._jobs)
        {
            if (kvp.Value.CreatedAt < cutoff)
            {
                if (this._jobs.TryRemove(kvp.Key, out var removed) && removed.PdfFilePath != null)
                {
                    try
                    {
                        File.Delete(removed.PdfFilePath);
                    }
                    catch (IOException)
                    {
                        // 削除失敗は無視
                    }
                }
            }
        }
    }
}
