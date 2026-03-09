using Aloe.Apps.WindowsServiceMonitorLib.OtelViewer.Models;

namespace Aloe.Apps.WindowsServiceMonitorLib.OtelViewer.Services;

public interface IOtelIngestionService
{
    void IngestTraces(OtlpExportTraceRequest request);

    void IngestMetrics(OtlpExportMetricsRequest request);

    void IngestLogs(OtlpExportLogsRequest request);
}
