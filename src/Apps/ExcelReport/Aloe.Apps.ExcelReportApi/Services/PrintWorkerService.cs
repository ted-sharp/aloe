// <copyright file="PrintWorkerService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportApi.Options;
using Aloe.Apps.ExcelReportLib.Models;
using Aloe.Apps.ExcelReportLib.Services;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.ExcelReportApi.Services;

/// <summary>
/// キューから印刷ジョブを取り出して印刷を実行する BackgroundService。
/// </summary>
public class PrintWorkerService : BackgroundService
{
    private readonly PrintJobQueue _queue;
    private readonly PrintJobStore _store;
    private readonly ExcelPrintService _printService;
    private readonly ExcelReportApiOptions _options;
    private readonly ILogger<PrintWorkerService> _logger;

    /// <summary>
    /// <see cref="PrintWorkerService"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public PrintWorkerService(
        PrintJobQueue queue,
        PrintJobStore store,
        ExcelPrintService printService,
        IOptions<ExcelReportApiOptions> options,
        ILogger<PrintWorkerService> logger)
    {
        this._queue = queue;
        this._store = store;
        this._printService = printService;
        this._options = options.Value;
        this._logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            PrintJobRequest request;
            try
            {
                request = await this._queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            this._store.SetProcessing(request.JobId);

            var outputPath = this.GetOutputPath();
            var pdfPath = Path.Combine(outputPath, $"{request.JobId}.pdf");

            try
            {
                using var excelStream = new MemoryStream(request.ExcelBytes);

                var reportOptions = new ReportOptions
                {
                    SheetIndex = request.SheetIndex,
                };

                var printOptions = new PrintOptions
                {
                    PrinterName = request.PrinterName,
                    Copies = request.Copies,
                };

                this._printService.Print(excelStream, request.Variables, reportOptions, pdfPath, printOptions);
                this._store.SetCompleted(request.JobId, pdfPath);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "印刷ジョブ {JobId} の処理中にエラーが発生しました。", request.JobId);
                this._store.SetFailed(request.JobId, ex.Message);

                try { File.Delete(pdfPath); } catch (IOException) { }
            }

            this.PurgeOldPdfs(outputPath);
        }
    }

    private string GetOutputPath()
    {
        return Path.IsPathRooted(this._options.PrintOutputPath)
            ? this._options.PrintOutputPath
            : Path.Combine(AppContext.BaseDirectory, this._options.PrintOutputPath);
    }

    private void PurgeOldPdfs(string outputPath)
    {
        if (!Directory.Exists(outputPath))
        {
            return;
        }

        var files = Directory.GetFiles(outputPath, "*.pdf")
            .Select(f => new FileInfo(f))
            .OrderBy(f => f.CreationTimeUtc)
            .ToList();

        int toDelete = files.Count - this._options.MaxPrintedPdfCount;
        for (int i = 0; i < toDelete; i++)
        {
            try
            {
                files[i].Delete();
            }
            catch (IOException)
            {
                // 削除失敗は無視
            }
        }
    }
}
