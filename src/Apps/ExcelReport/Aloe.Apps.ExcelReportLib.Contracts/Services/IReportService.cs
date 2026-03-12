// <copyright file="IReportService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using MagicOnion;
using MessagePack;

namespace Aloe.Apps.ExcelReportLib.Contracts.Services;

/// <summary>ジョブ投入リクエスト。</summary>
[MessagePackObject]
public class SubmitJobRequest
{
    /// <summary>Excelテンプレートのバイト列。</summary>
    [Key(0)]
    public byte[] ExcelBytes { get; set; } = [];

    /// <summary>変数辞書（省略可）。</summary>
    [Key(1)]
    public Dictionary<string, string>? Variables { get; set; }

    /// <summary>シートインデックス。</summary>
    [Key(2)]
    public int SheetIndex { get; set; }
}

/// <summary>ジョブ投入レスポンス。</summary>
[MessagePackObject]
public class SubmitJobResponse
{
    /// <summary>ジョブID。</summary>
    [Key(0)]
    public Guid JobId { get; set; }
}

/// <summary>ジョブ状態取得リクエスト。</summary>
[MessagePackObject]
public class GetJobStatusRequest
{
    /// <summary>ジョブID。</summary>
    [Key(0)]
    public Guid JobId { get; set; }
}

/// <summary>ジョブ状態レスポンス。</summary>
[MessagePackObject]
public class JobStatusResponse
{
    /// <summary>ジョブID。</summary>
    [Key(0)]
    public Guid JobId { get; set; }

    /// <summary>状態文字列。</summary>
    [Key(1)]
    public string Status { get; set; } = String.Empty;

    /// <summary>エラーメッセージ（失敗時のみ）。</summary>
    [Key(2)]
    public string? ErrorMessage { get; set; }
}

/// <summary>ジョブ一覧リクエスト。</summary>
[MessagePackObject]
public class ListJobsRequest
{
    /// <summary>状態フィルター（省略可）。</summary>
    [Key(0)]
    public string? StatusFilter { get; set; }
}

/// <summary>ジョブサマリー。</summary>
[MessagePackObject]
public class JobSummary
{
    /// <summary>ジョブID。</summary>
    [Key(0)]
    public Guid JobId { get; set; }

    /// <summary>状態文字列。</summary>
    [Key(1)]
    public string Status { get; set; } = String.Empty;

    /// <summary>作成日時（UTC）。</summary>
    [Key(2)]
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>ジョブ一覧レスポンス。</summary>
[MessagePackObject]
public class ListJobsResponse
{
    /// <summary>ジョブ一覧。</summary>
    [Key(0)]
    public List<JobSummary> Jobs { get; set; } = [];
}

/// <summary>テンプレートアップロードリクエスト。</summary>
[MessagePackObject]
public class UploadTemplateRequest
{
    /// <summary>ファイル名。</summary>
    [Key(0)]
    public string FileName { get; set; } = String.Empty;

    /// <summary>ファイルのバイト列。</summary>
    [Key(1)]
    public byte[] FileBytes { get; set; } = [];
}

/// <summary>テンプレートアップロードレスポンス。</summary>
[MessagePackObject]
public class UploadTemplateResponse
{
    /// <summary>保存されたファイル名。</summary>
    [Key(0)]
    public string FileName { get; set; } = String.Empty;
}

/// <summary>テンプレート一覧レスポンス。</summary>
[MessagePackObject]
public class ListTemplatesResponse
{
    /// <summary>ファイル名一覧。</summary>
    [Key(0)]
    public List<string> FileNames { get; set; } = [];
}

/// <summary>
/// Excel帳票サービスの gRPC インターフェース。
/// </summary>
public interface IReportService : IService<IReportService>
{
    /// <summary>PDF生成ジョブを投入する。</summary>
    UnaryResult<SubmitJobResponse> SubmitJobAsync(SubmitJobRequest request);

    /// <summary>ジョブの状態を取得する。</summary>
    UnaryResult<JobStatusResponse> GetJobStatusAsync(GetJobStatusRequest request);

    /// <summary>ジョブ一覧を取得する。</summary>
    UnaryResult<ListJobsResponse> ListJobsAsync(ListJobsRequest request);

    /// <summary>テンプレートをアップロードする。</summary>
    UnaryResult<UploadTemplateResponse> UploadTemplateAsync(UploadTemplateRequest request);

    /// <summary>テンプレート一覧を取得する。</summary>
    UnaryResult<ListTemplatesResponse> ListTemplatesAsync();
}
