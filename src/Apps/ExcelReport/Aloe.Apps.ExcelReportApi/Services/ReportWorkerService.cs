// <copyright file="ReportWorkerService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportLib.Models;
using Aloe.Apps.ExcelReportLib.Services;

namespace Aloe.Apps.ExcelReportApi.Services;

/// <summary>
/// キューからジョブを取り出してPDF生成を実行する BackgroundService。
/// </summary>
public class ReportWorkerService : BackgroundService
{
    private readonly ReportJobQueue _queue;
    private readonly JobStore _store;
    private readonly ExcelReportService _reportService;
    private readonly ILogger<ReportWorkerService> _logger;

    /// <summary>
    /// <see cref="ReportWorkerService"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public ReportWorkerService(
        ReportJobQueue queue,
        JobStore store,
        ExcelReportService reportService,
        ILogger<ReportWorkerService> logger)
    {
        this._queue = queue;
        this._store = store;
        this._reportService = reportService;
        this._logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var purgeTimer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        var purgeTask = this.RunPurgeLoopAsync(purgeTimer, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            JobRequest request;
            try
            {
                request = await this._queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            this._store.SetProcessing(request.JobId);

            var pdfPath = Path.Combine(Path.GetTempPath(), $"{request.JobId}.pdf");
            try
            {
                using var excelStream = new MemoryStream(request.ExcelBytes);
                using var pdfStream = File.Create(pdfPath);

                var options = new ReportOptions
                {
                    SheetIndex = request.SheetIndex,
                };

                bool clearKeywords = request.Variables == null;
                this._reportService.GeneratePdf(excelStream, pdfStream, request.Variables, options, clearKeywords);
                this._store.SetCompleted(request.JobId, pdfPath);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "ジョブ {JobId} の処理中にエラーが発生しました。", request.JobId);
                this._store.SetFailed(request.JobId, ex.Message);

                try { File.Delete(pdfPath); } catch (IOException) { }
            }
        }

        purgeTimer.Dispose();
        await purgeTask;
    }

    private async Task RunPurgeLoopAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                this._store.PurgeExpired();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
