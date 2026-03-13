// <copyright file="PrintJobInfo.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.ExcelReportApi.Models;

/// <summary>
/// 印刷ジョブの情報。
/// </summary>
public class PrintJobInfo
{
    /// <summary>ジョブID。</summary>
    public Guid JobId { get; set; }

    /// <summary>ジョブの現在状態。</summary>
    public JobStatus Status { get; set; }

    /// <summary>保存済みPDFファイルのパス。Completed のときのみ設定される。</summary>
    public string? PdfFilePath { get; set; }

    /// <summary>エラーメッセージ。Failed のときのみ設定される。</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>ジョブ作成日時（UTC）。</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
