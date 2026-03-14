using Aloe.Apps.DashboardLib.OtelViewer.Models;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.DashboardLib.OtelViewer.Storage;

public sealed class InMemoryOtelStore : IOtelStore
{
    private readonly CircularBuffer<OtelSpan> spans;
    private readonly CircularBuffer<OtelMetricPoint> metrics;
    private readonly CircularBuffer<OtelLogRecord> logs;

    public InMemoryOtelStore(IOptions<OtelViewerOptions> options)
    {
        var opts = options.Value;
        this.spans = new CircularBuffer<OtelSpan>(opts.MaxTraces);
        this.metrics = new CircularBuffer<OtelMetricPoint>(opts.MaxMetricPoints);
        this.logs = new CircularBuffer<OtelLogRecord>(opts.MaxLogRecords);
    }

    public void AddSpans(IEnumerable<OtelSpan> spans) => this.spans.AddRange(spans);

    public void AddMetricPoints(IEnumerable<OtelMetricPoint> points) => this.metrics.AddRange(points);

    public void AddLogRecords(IEnumerable<OtelLogRecord> records) => this.logs.AddRange(records);

    public IReadOnlyList<OtelSpan> QuerySpans(string? serviceName, string? traceId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take)
    {
        return this.spans.Query(
            s => MatchesSpan(s, serviceName, traceId, from, to),
            skip,
            take);
    }

    public int CountSpans(string? serviceName, string? traceId, DateTimeOffset? from, DateTimeOffset? to)
    {
        return this.spans.CountWhere(s => MatchesSpan(s, serviceName, traceId, from, to));
    }

    public IReadOnlyList<OtelMetricPoint> QueryMetrics(string? serviceName, string? metricName, DateTimeOffset? from, DateTimeOffset? to, int skip, int take)
    {
        return this.metrics.Query(
            m => MatchesMetric(m, serviceName, metricName, from, to),
            skip,
            take);
    }

    public int CountMetrics(string? serviceName, string? metricName, DateTimeOffset? from, DateTimeOffset? to)
    {
        return this.metrics.CountWhere(m => MatchesMetric(m, serviceName, metricName, from, to));
    }

    public IReadOnlyList<OtelLogRecord> QueryLogs(string? serviceName, string? severity, string? traceId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take)
    {
        return this.logs.Query(
            l => MatchesLog(l, serviceName, severity, traceId, from, to),
            skip,
            take);
    }

    public int CountLogs(string? serviceName, string? severity, string? traceId, DateTimeOffset? from, DateTimeOffset? to)
    {
        return this.logs.CountWhere(l => MatchesLog(l, serviceName, severity, traceId, from, to));
    }

    public IReadOnlyList<string> GetServiceNames()
    {
        var spanServices = this.spans.DistinctValues(s => s.ServiceName);
        var metricServices = this.metrics.DistinctValues(m => m.ServiceName);
        var logServices = this.logs.DistinctValues(l => l.ServiceName);

        var all = new SortedSet<string>(spanServices);
        all.UnionWith(metricServices);
        all.UnionWith(logServices);
        return [.. all];
    }

    public IReadOnlyList<string> GetMetricNames(string? serviceName)
    {
        if (string.IsNullOrEmpty(serviceName))
        {
            return [.. this.metrics.DistinctValues(m => m.MetricName).Order()];
        }

        var names = new SortedSet<string>();
        var all = this.metrics.Query(m => m.ServiceName == serviceName, 0, int.MaxValue);
        foreach (var m in all)
        {
            names.Add(m.MetricName);
        }

        return [.. names];
    }

    private static bool MatchesSpan(OtelSpan s, string? serviceName, string? traceId, DateTimeOffset? from, DateTimeOffset? to)
    {
        if (!string.IsNullOrEmpty(serviceName) && !s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
            return false;
        if (!string.IsNullOrEmpty(traceId) && !s.TraceId.Equals(traceId, StringComparison.OrdinalIgnoreCase))
            return false;
        if (from.HasValue && s.StartTime < from.Value)
            return false;
        if (to.HasValue && s.StartTime > to.Value)
            return false;
        return true;
    }

    private static bool MatchesMetric(OtelMetricPoint m, string? serviceName, string? metricName, DateTimeOffset? from, DateTimeOffset? to)
    {
        if (!string.IsNullOrEmpty(serviceName) && !m.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
            return false;
        if (!string.IsNullOrEmpty(metricName) && !m.MetricName.Equals(metricName, StringComparison.OrdinalIgnoreCase))
            return false;
        if (from.HasValue && m.Timestamp < from.Value)
            return false;
        if (to.HasValue && m.Timestamp > to.Value)
            return false;
        return true;
    }

    private static bool MatchesLog(OtelLogRecord l, string? serviceName, string? severity, string? traceId, DateTimeOffset? from, DateTimeOffset? to)
    {
        if (!string.IsNullOrEmpty(serviceName) && !l.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
            return false;
        if (!string.IsNullOrEmpty(severity) && !l.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase))
            return false;
        if (!string.IsNullOrEmpty(traceId) && !string.Equals(l.TraceId, traceId, StringComparison.OrdinalIgnoreCase))
            return false;
        if (from.HasValue && l.Timestamp < from.Value)
            return false;
        if (to.HasValue && l.Timestamp > to.Value)
            return false;
        return true;
    }
}
