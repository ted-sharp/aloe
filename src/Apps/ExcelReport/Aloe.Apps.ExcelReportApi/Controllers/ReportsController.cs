// <copyright file="ReportsController.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Text.Json;
using Aloe.Apps.ExcelReportApi.Models;
using Aloe.Apps.ExcelReportApi.Options;
using Aloe.Apps.ExcelReportApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.ExcelReportApi.Controllers;

/// <summary>
/// PDF生成ジョブを管理するコントローラー。
/// </summary>
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly ReportJobQueue _queue;
    private readonly JobStore _store;
    private readonly ExcelReportApiOptions _options;

    /// <summary>
    /// <see cref="ReportsController"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public ReportsController(
        ReportJobQueue queue,
        JobStore store,
        IOptions<ExcelReportApiOptions> options)
    {
        this._queue = queue;
        this._store = store;
        this._options = options.Value;
    }

    /// <summary>
    /// PDF生成ジョブをキューに投入する。
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> PostAsync(
        IFormFile? template,
        [FromForm] string? templateName,
        [FromForm] string? variables,
        [FromForm] int sheetIndex = 0)
    {
        if (template == null && String.IsNullOrEmpty(templateName))
        {
            return this.BadRequest("'template' または 'templateName' のいずれかが必要です。");
        }

        byte[] excelBytes;

        if (template != null)
        {
            using var ms = new MemoryStream();
            await template.CopyToAsync(ms);
            excelBytes = ms.ToArray();
        }
        else
        {
            var basePath = Path.IsPathRooted(this._options.TemplatePath)
                ? this._options.TemplatePath
                : Path.Combine(AppContext.BaseDirectory, this._options.TemplatePath);

            var filePath = Path.Combine(basePath, templateName!);
            if (!System.IO.File.Exists(filePath))
            {
                return this.NotFound($"テンプレート '{templateName}' が見つかりません。");
            }

            excelBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        }

        IReadOnlyDictionary<string, string>? variablesDict = null;
        if (!String.IsNullOrEmpty(variables))
        {
            variablesDict = JsonSerializer.Deserialize<Dictionary<string, string>>(variables);
        }

        var jobId = Guid.NewGuid();
        this._store.Add(jobId);
        this._queue.Enqueue(new JobRequest(jobId, excelBytes, variablesDict, sheetIndex));

        return this.AcceptedAtAction(nameof(GetStatusAsync), new { jobId }, new { jobId });
    }

    /// <summary>
    /// ジョブ一覧を返す。status クエリで絞り込み可能。
    /// </summary>
    [HttpGet]
    public IActionResult List([FromQuery] string? status)
    {
        var all = this._store.GetAll();
        IEnumerable<JobInfo> result = all;
        if (!String.IsNullOrEmpty(status) &&
            Enum.TryParse<JobStatus>(status, ignoreCase: true, out var parsed))
        {
            result = result.Where(j => j.Status == parsed);
        }

        return this.Ok(result.Select(j => new
        {
            jobId = j.JobId,
            status = j.Status.ToString(),
            createdAt = j.CreatedAt,
            error = j.ErrorMessage,
        }));
    }

    /// <summary>
    /// 指定ジョブの処理状態を返す。
    /// </summary>
    [HttpGet("{jobId:guid}/status")]
    public IActionResult GetStatusAsync(Guid jobId)
    {
        var info = this._store.Get(jobId);
        if (info == null)
        {
            return this.NotFound();
        }

        return this.Ok(new
        {
            jobId = info.JobId,
            status = info.Status.ToString(),
            error = info.ErrorMessage,
        });
    }

    /// <summary>
    /// 生成済みPDFをダウンロードする。
    /// </summary>
    [HttpGet("{jobId:guid}/download")]
    public IActionResult Download(Guid jobId)
    {
        var info = this._store.Get(jobId);
        if (info == null)
        {
            return this.NotFound();
        }

        if (info.Status == JobStatus.Completed && info.PdfFilePath != null)
        {
            var stream = System.IO.File.OpenRead(info.PdfFilePath);
            return this.File(stream, "application/pdf", $"{jobId}.pdf");
        }

        return this.Accepted(new { jobId = info.JobId, status = info.Status.ToString() });
    }
}
