using Aloe.Apps.DashboardLib.OtelViewer.Models;

namespace Aloe.Apps.DashboardLib.OtelViewer.Services;

public interface IOtelIngestionService
{
    void IngestTraces(OtlpExportTraceRequest request);

    void IngestMetrics(OtlpExportMetricsRequest request);

    void IngestLogs(OtlpExportLogsRequest request);
}
