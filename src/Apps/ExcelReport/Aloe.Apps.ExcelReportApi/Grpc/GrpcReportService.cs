// <copyright file="GrpcReportService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportApi.Models;
using Aloe.Apps.ExcelReportApi.Options;
using Aloe.Apps.ExcelReportApi.Services;
using Aloe.Apps.ExcelReportLib.Abstractions;
using Aloe.Apps.ExcelReportLib.Contracts.Services;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.ExcelReportApi.Grpc;

/// <summary>
/// Excel帳票サービスの MagicOnion gRPC 実装。
/// </summary>
public class GrpcReportService : ServiceBase<IReportService>, IReportService
{
    private readonly ReportJobQueue _queue;
    private readonly JobStore _store;
    private readonly PrintJobQueue _printQueue;
    private readonly PrintJobStore _printStore;
    private readonly ISheetPrinter _printer;
    private readonly ExcelReportApiOptions _options;

    /// <summary>
    /// <see cref="GrpcReportService"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public GrpcReportService(
        ReportJobQueue queue,
        JobStore store,
        PrintJobQueue printQueue,
        PrintJobStore printStore,
        ISheetPrinter printer,
        IOptions<ExcelReportApiOptions> options)
    {
        this._queue = queue;
        this._store = store;
        this._printQueue = printQueue;
        this._printStore = printStore;
        this._printer = printer;
        this._options = options.Value;
    }

    /// <inheritdoc/>
    public UnaryResult<SubmitJobResponse> SubmitJobAsync(SubmitJobRequest request)
    {
        var jobId = Guid.NewGuid();
        this._store.Add(jobId);
        this._queue.Enqueue(new JobRequest(jobId, request.ExcelBytes, request.Variables, request.SheetIndex));
        return UnaryResult.FromResult(new SubmitJobResponse { JobId = jobId });
    }

    /// <inheritdoc/>
    public UnaryResult<JobStatusResponse> GetJobStatusAsync(GetJobStatusRequest request)
    {
        var info = this._store.Get(request.JobId);
        if (info == null)
        {
            throw new ReturnStatusException(StatusCode.NotFound, $"ジョブ '{request.JobId}' が見つかりません。");
        }

        return UnaryResult.FromResult(new JobStatusResponse
        {
            JobId = info.JobId,
            Status = info.Status.ToString(),
            ErrorMessage = info.ErrorMessage,
        });
    }

    /// <inheritdoc/>
    public UnaryResult<ListJobsResponse> ListJobsAsync(ListJobsRequest request)
    {
        IEnumerable<JobInfo> jobs = this._store.GetAll();
        if (!String.IsNullOrEmpty(request.StatusFilter) &&
            Enum.TryParse<JobStatus>(request.StatusFilter, ignoreCase: true, out var parsed))
        {
            jobs = jobs.Where(j => j.Status == parsed);
        }

        var summaries = jobs.Select(j => new JobSummary
        {
            JobId = j.JobId,
            Status = j.Status.ToString(),
            CreatedAt = j.CreatedAt,
        }).ToList();

        return UnaryResult.FromResult(new ListJobsResponse { Jobs = summaries });
    }

    /// <inheritdoc/>
    public async UnaryResult<UploadTemplateResponse> UploadTemplateAsync(UploadTemplateRequest request)
    {
        var fileName = Path.GetFileName(request.FileName);
        if (String.IsNullOrEmpty(fileName) || fileName.Contains('/') || fileName.Contains('\\'))
        {
            throw new ReturnStatusException(StatusCode.InvalidArgument, "ファイル名が無効です。");
        }

        var basePath = Path.IsPathRooted(this._options.TemplatePath)
            ? this._options.TemplatePath
            : Path.Combine(AppContext.BaseDirectory, this._options.TemplatePath);

        Directory.CreateDirectory(basePath);
        var destPath = Path.Combine(basePath, fileName);
        await File.WriteAllBytesAsync(destPath, request.FileBytes);

        return new UploadTemplateResponse { FileName = fileName };
    }

    /// <inheritdoc/>
    public UnaryResult<ListTemplatesResponse> ListTemplatesAsync()
    {
        var basePath = Path.IsPathRooted(this._options.TemplatePath)
            ? this._options.TemplatePath
            : Path.Combine(AppContext.BaseDirectory, this._options.TemplatePath);

        List<string> fileNames = [];
        if (Directory.Exists(basePath))
        {
            fileNames = [.. Directory.GetFiles(basePath).Select(f => Path.GetFileName(f)!)];
        }

        return UnaryResult.FromResult(new ListTemplatesResponse { FileNames = fileNames });
    }

    /// <inheritdoc/>
    public UnaryResult<ListPrintersResponse> ListPrintersAsync()
    {
        var printers = this._printer.GetInstalledPrinters();
        return UnaryResult.FromResult(new ListPrintersResponse { PrinterNames = [.. printers] });
    }

    /// <inheritdoc/>
    public UnaryResult<SubmitPrintJobResponse> SubmitPrintJobAsync(SubmitPrintJobRequest request)
    {
        var jobId = Guid.NewGuid();
        this._printStore.Add(jobId);
        this._printQueue.Enqueue(new PrintJobRequest(
            jobId,
            request.ExcelBytes,
            request.Variables,
            request.SheetIndex,
            request.PrinterName,
            request.Copies));
        return UnaryResult.FromResult(new SubmitPrintJobResponse { JobId = jobId });
    }

    /// <inheritdoc/>
    public UnaryResult<JobStatusResponse> GetPrintJobStatusAsync(GetPrintJobStatusRequest request)
    {
        var info = this._printStore.Get(request.JobId);
        if (info == null)
        {
            throw new ReturnStatusException(StatusCode.NotFound, $"印刷ジョブ '{request.JobId}' が見つかりません。");
        }

        return UnaryResult.FromResult(new JobStatusResponse
        {
            JobId = info.JobId,
            Status = info.Status.ToString(),
            ErrorMessage = info.ErrorMessage,
        });
    }
}
