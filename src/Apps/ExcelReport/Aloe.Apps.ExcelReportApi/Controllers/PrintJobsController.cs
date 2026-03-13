// <copyright file="PrintJobsController.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Text.Json;
using Aloe.Apps.ExcelReportApi.Models;
using Aloe.Apps.ExcelReportApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aloe.Apps.ExcelReportApi.Controllers;

/// <summary>
/// 印刷ジョブを管理するコントローラー。
/// </summary>
[ApiController]
[Route("api/printjobs")]
public class PrintJobsController : ControllerBase
{
    private readonly PrintJobQueue _queue;
    private readonly PrintJobStore _store;

    /// <summary>
    /// <see cref="PrintJobsController"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public PrintJobsController(PrintJobQueue queue, PrintJobStore store)
    {
        this._queue = queue;
        this._store = store;
    }

    /// <summary>
    /// 印刷ジョブをキューに投入する。
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> PostAsync(
        IFormFile template,
        [FromForm] string? variables,
        [FromForm] string printerName,
        [FromForm] int sheetIndex = 0,
        [FromForm] int copies = 1)
    {
        if (template == null)
        {
            return this.BadRequest("'template' が必要です。");
        }

        if (String.IsNullOrEmpty(printerName))
        {
            return this.BadRequest("'printerName' が必要です。");
        }

        using var ms = new MemoryStream();
        await template.CopyToAsync(ms);
        var excelBytes = ms.ToArray();

        IReadOnlyDictionary<string, string>? variablesDict = null;
        if (!String.IsNullOrEmpty(variables))
        {
            variablesDict = JsonSerializer.Deserialize<Dictionary<string, string>>(variables);
        }

        var jobId = Guid.NewGuid();
        this._store.Add(jobId);
        this._queue.Enqueue(new PrintJobRequest(jobId, excelBytes, variablesDict, sheetIndex, printerName, copies));

        return this.AcceptedAtAction(nameof(GetStatus), new { jobId }, new { jobId });
    }

    /// <summary>
    /// 指定印刷ジョブの処理状態を返す。
    /// </summary>
    [HttpGet("{jobId:guid}/status")]
    public IActionResult GetStatus(Guid jobId)
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
}
