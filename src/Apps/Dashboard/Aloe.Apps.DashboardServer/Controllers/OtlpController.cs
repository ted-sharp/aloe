using Aloe.Apps.DashboardLib.OtelViewer.Models;
using Aloe.Apps.DashboardLib.OtelViewer.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aloe.Apps.DashboardServer.Controllers;

[ApiController]
public class OtlpController : ControllerBase
{
    private readonly IOtelIngestionService ingestionService;

    public OtlpController(IOtelIngestionService ingestionService)
    {
        this.ingestionService = ingestionService;
    }

    [HttpPost("/v1/traces")]
    public IActionResult IngestTraces([FromBody] OtlpExportTraceRequest request)
    {
        this.ingestionService.IngestTraces(request);
        return this.Ok(new { });
    }

    [HttpPost("/v1/metrics")]
    public IActionResult IngestMetrics([FromBody] OtlpExportMetricsRequest request)
    {
        this.ingestionService.IngestMetrics(request);
        return this.Ok(new { });
    }

    [HttpPost("/v1/logs")]
    public IActionResult IngestLogs([FromBody] OtlpExportLogsRequest request)
    {
        this.ingestionService.IngestLogs(request);
        return this.Ok(new { });
    }
}
