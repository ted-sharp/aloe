using Aloe.Apps.DashboardLib.OtelViewer.Models;
using Aloe.Apps.DashboardLib.OtelViewer.Storage;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.DashboardLib.OtelViewer.Services;

public sealed class OtelIngestionService : IOtelIngestionService
{
    private readonly IOtelStore store;
    private readonly ILogger<OtelIngestionService> logger;

    public OtelIngestionService(IOtelStore store, ILogger<OtelIngestionService> logger)
    {
        this.store = store;
        this.logger = logger;
    }

    public void IngestTraces(OtlpExportTraceRequest request)
    {
        var spans = new List<OtelSpan>();

        foreach (var resourceSpans in request.ResourceSpans ?? [])
        {
            var serviceName = GetServiceName(resourceSpans.Resource);

            foreach (var scopeSpans in resourceSpans.ScopeSpans ?? [])
            {
                foreach (var span in scopeSpans.Spans ?? [])
                {
                    spans.Add(new OtelSpan
                    {
                        TraceId = span.TraceId,
                        SpanId = span.SpanId,
                        ParentSpanId = string.IsNullOrEmpty(span.ParentSpanId) ? null : span.ParentSpanId,
                        ServiceName = serviceName,
                        Name = span.Name,
                        StartTime = ParseNanoTimestamp(span.StartTimeUnixNano),
                        EndTime = ParseNanoTimestamp(span.EndTimeUnixNano),
                        Kind = (OtelSpanKind)span.Kind,
                        StatusCode = span.Status != null ? (OtelStatusCode)span.Status.Code : OtelStatusCode.Unset,
                        StatusMessage = span.Status?.Message,
                        Attributes = ToAttributes(span.Attributes),
                        Events = (span.Events ?? []).Select(e => new OtelSpanEvent
                        {
                            Name = e.Name,
                            Timestamp = ParseNanoTimestamp(e.TimeUnixNano),
                            Attributes = ToAttributes(e.Attributes),
                        }).ToList(),
                    });
                }
            }
        }

        if (spans.Count > 0)
        {
            this.store.AddSpans(spans);
            this.logger.LogDebug("Ingested {Count} spans", spans.Count);
        }
    }

    public void IngestMetrics(OtlpExportMetricsRequest request)
    {
        var points = new List<OtelMetricPoint>();

        foreach (var resourceMetrics in request.ResourceMetrics ?? [])
        {
            var serviceName = GetServiceName(resourceMetrics.Resource);

            foreach (var scopeMetrics in resourceMetrics.ScopeMetrics ?? [])
            {
                foreach (var metric in scopeMetrics.Metrics ?? [])
                {
                    ExtractDataPoints(points, serviceName, metric);
                }
            }
        }

        if (points.Count > 0)
        {
            this.store.AddMetricPoints(points);
            this.logger.LogDebug("Ingested {Count} metric points", points.Count);
        }
    }

    public void IngestLogs(OtlpExportLogsRequest request)
    {
        var records = new List<OtelLogRecord>();

        foreach (var resourceLogs in request.ResourceLogs ?? [])
        {
            var serviceName = GetServiceName(resourceLogs.Resource);

            foreach (var scopeLogs in resourceLogs.ScopeLogs ?? [])
            {
                foreach (var logRecord in scopeLogs.LogRecords ?? [])
                {
                    var timestamp = ParseNanoTimestamp(logRecord.TimeUnixNano);
                    if (timestamp == DateTimeOffset.MinValue)
                    {
                        timestamp = DateTimeOffset.UtcNow;
                    }

                    records.Add(new OtelLogRecord
                    {
                        ServiceName = serviceName,
                        Timestamp = timestamp,
                        Severity = logRecord.SeverityText ?? SeverityNumberToText(logRecord.SeverityNumber),
                        SeverityNumber = logRecord.SeverityNumber,
                        Body = logRecord.Body?.StringValue,
                        TraceId = string.IsNullOrEmpty(logRecord.TraceId) ? null : logRecord.TraceId,
                        SpanId = string.IsNullOrEmpty(logRecord.SpanId) ? null : logRecord.SpanId,
                        Attributes = ToAttributes(logRecord.Attributes),
                    });
                }
            }
        }

        if (records.Count > 0)
        {
            this.store.AddLogRecords(records);
            this.logger.LogDebug("Ingested {Count} log records", records.Count);
        }
    }

    private static void ExtractDataPoints(List<OtelMetricPoint> points, string serviceName, OtlpMetric metric)
    {
        if (metric.Gauge != null)
        {
            foreach (var dp in metric.Gauge.DataPoints ?? [])
            {
                points.Add(CreateNumberPoint(serviceName, metric, OtelMetricType.Gauge, dp));
            }
        }
        else if (metric.Sum != null)
        {
            foreach (var dp in metric.Sum.DataPoints ?? [])
            {
                points.Add(CreateNumberPoint(serviceName, metric, OtelMetricType.Sum, dp));
            }
        }
        else if (metric.Histogram != null)
        {
            foreach (var dp in metric.Histogram.DataPoints ?? [])
            {
                var count = ParseLong(dp.Count);
                points.Add(new OtelMetricPoint
                {
                    ServiceName = serviceName,
                    MetricName = metric.Name,
                    Description = metric.Description,
                    Unit = metric.Unit,
                    MetricType = OtelMetricType.Histogram,
                    Timestamp = ParseNanoTimestamp(dp.TimeUnixNano),
                    Value = count > 0 ? (dp.Sum ?? 0) / count : 0,
                    Attributes = ToAttributes(dp.Attributes),
                });
            }
        }
        else if (metric.Summary != null)
        {
            foreach (var dp in metric.Summary.DataPoints ?? [])
            {
                var count = ParseLong(dp.Count);
                points.Add(new OtelMetricPoint
                {
                    ServiceName = serviceName,
                    MetricName = metric.Name,
                    Description = metric.Description,
                    Unit = metric.Unit,
                    MetricType = OtelMetricType.Summary,
                    Timestamp = ParseNanoTimestamp(dp.TimeUnixNano),
                    Value = count > 0 ? (dp.Sum ?? 0) / count : 0,
                    Attributes = ToAttributes(dp.Attributes),
                });
            }
        }
    }

    private static OtelMetricPoint CreateNumberPoint(string serviceName, OtlpMetric metric, OtelMetricType type, OtlpNumberDataPoint dp)
    {
        double value = dp.AsDouble ?? (dp.AsInt != null && long.TryParse(dp.AsInt, out var intVal) ? intVal : 0);

        return new OtelMetricPoint
        {
            ServiceName = serviceName,
            MetricName = metric.Name,
            Description = metric.Description,
            Unit = metric.Unit,
            MetricType = type,
            Timestamp = ParseNanoTimestamp(dp.TimeUnixNano),
            Value = value,
            Attributes = ToAttributes(dp.Attributes),
        };
    }

    private static string GetServiceName(OtlpResource? resource)
    {
        if (resource?.Attributes != null)
        {
            foreach (var attr in resource.Attributes)
            {
                if (attr.Key == "service.name")
                {
                    return attr.Value?.StringValue ?? "unknown";
                }
            }
        }

        return "unknown";
    }

    private static Dictionary<string, string> ToAttributes(List<OtlpKeyValue>? attributes)
    {
        if (attributes == null || attributes.Count == 0)
        {
            return new Dictionary<string, string>();
        }

        var result = new Dictionary<string, string>(attributes.Count);
        foreach (var attr in attributes)
        {
            var val = attr.Value;
            result[attr.Key] = val?.StringValue
                ?? val?.IntValue
                ?? val?.DoubleValue?.ToString()
                ?? val?.BoolValue?.ToString()
                ?? string.Empty;
        }

        return result;
    }

    private static DateTimeOffset ParseNanoTimestamp(string? nanoString)
    {
        if (string.IsNullOrEmpty(nanoString) || !ulong.TryParse(nanoString, out var nanos) || nanos == 0)
        {
            return DateTimeOffset.MinValue;
        }

        return DateTimeOffset.FromUnixTimeMilliseconds((long)(nanos / 1_000_000));
    }

    private static long ParseLong(string? value)
    {
        if (string.IsNullOrEmpty(value) || !long.TryParse(value, out var result))
        {
            return 0;
        }

        return result;
    }

    private static string SeverityNumberToText(int severityNumber) => severityNumber switch
    {
        >= 1 and <= 4 => "TRACE",
        >= 5 and <= 8 => "DEBUG",
        >= 9 and <= 12 => "INFO",
        >= 13 and <= 16 => "WARN",
        >= 17 and <= 20 => "ERROR",
        >= 21 and <= 24 => "FATAL",
        _ => "UNSPECIFIED",
    };
}
