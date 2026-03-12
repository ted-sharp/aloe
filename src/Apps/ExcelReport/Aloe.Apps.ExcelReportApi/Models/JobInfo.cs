// <copyright file="JobInfo.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.ExcelReportApi.Models;

/// <summary>
/// ジョブの状態を表す列挙型。
/// </summary>
public enum JobStatus
{
    /// <summary>キュー待機中。</summary>
    Queued,

    /// <summary>処理中。</summary>
    Processing,

    /// <summary>完了。</summary>
    Completed,

    /// <summary>失敗。</summary>
    Failed,
}

/// <summary>
/// PDF生成ジョブの情報。
/// </summary>
public class JobInfo
{
    /// <summary>ジョブID。</summary>
    public Guid JobId { get; set; }

    /// <summary>ジョブの現在状態。</summary>
    public JobStatus Status { get; set; }

    /// <summary>生成済みPDFファイルのパス。Completed のときのみ設定される。</summary>
    public string? PdfFilePath { get; set; }

    /// <summary>エラーメッセージ。Failed のときのみ設定される。</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>ジョブ作成日時（UTC）。</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
