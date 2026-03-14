using Aloe.Apps.DashboardLib.OtelViewer.Models;

namespace Aloe.Apps.DashboardLib.OtelViewer.Storage;

public interface IOtelStore
{
    void AddSpans(IEnumerable<OtelSpan> spans);

    void AddMetricPoints(IEnumerable<OtelMetricPoint> points);

    void AddLogRecords(IEnumerable<OtelLogRecord> records);

    IReadOnlyList<OtelSpan> QuerySpans(string? serviceName, string? traceId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take);

    int CountSpans(string? serviceName, string? traceId, DateTimeOffset? from, DateTimeOffset? to);

    IReadOnlyList<OtelMetricPoint> QueryMetrics(string? serviceName, string? metricName, DateTimeOffset? from, DateTimeOffset? to, int skip, int take);

    int CountMetrics(string? serviceName, string? metricName, DateTimeOffset? from, DateTimeOffset? to);

    IReadOnlyList<OtelLogRecord> QueryLogs(string? serviceName, string? severity, string? traceId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take);

    int CountLogs(string? serviceName, string? severity, string? traceId, DateTimeOffset? from, DateTimeOffset? to);

    IReadOnlyList<string> GetServiceNames();

    IReadOnlyList<string> GetMetricNames(string? serviceName);
}
