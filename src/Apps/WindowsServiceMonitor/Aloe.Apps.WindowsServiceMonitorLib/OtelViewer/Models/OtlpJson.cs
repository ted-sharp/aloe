using System.Text.Json.Serialization;

namespace Aloe.Apps.WindowsServiceMonitorLib.OtelViewer.Models;

// OTLP JSON DTOs (https://opentelemetry.io/docs/specs/otlp/#json-protobuf-encoding)

// === Traces ===

public sealed class OtlpExportTraceRequest
{
    [JsonPropertyName("resourceSpans")]
    public List<OtlpResourceSpans>? ResourceSpans { get; set; }
}

public sealed class OtlpResourceSpans
{
    [JsonPropertyName("resource")]
    public OtlpResource? Resource { get; set; }

    [JsonPropertyName("scopeSpans")]
    public List<OtlpScopeSpans>? ScopeSpans { get; set; }
}

public sealed class OtlpScopeSpans
{
    [JsonPropertyName("scope")]
    public OtlpScope? Scope { get; set; }

    [JsonPropertyName("spans")]
    public List<OtlpSpan>? Spans { get; set; }
}

public sealed class OtlpSpan
{
    [JsonPropertyName("traceId")]
    public string TraceId { get; set; } = "";

    [JsonPropertyName("spanId")]
    public string SpanId { get; set; } = "";

    [JsonPropertyName("parentSpanId")]
    public string? ParentSpanId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("kind")]
    public int Kind { get; set; }

    [JsonPropertyName("startTimeUnixNano")]
    public string? StartTimeUnixNano { get; set; }

    [JsonPropertyName("endTimeUnixNano")]
    public string? EndTimeUnixNano { get; set; }

    [JsonPropertyName("attributes")]
    public List<OtlpKeyValue>? Attributes { get; set; }

    [JsonPropertyName("events")]
    public List<OtlpSpanEvent>? Events { get; set; }

    [JsonPropertyName("status")]
    public OtlpStatus? Status { get; set; }
}

public sealed class OtlpSpanEvent
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("timeUnixNano")]
    public string? TimeUnixNano { get; set; }

    [JsonPropertyName("attributes")]
    public List<OtlpKeyValue>? Attributes { get; set; }
}

public sealed class OtlpStatus
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

// === Metrics ===

public sealed class OtlpExportMetricsRequest
{
    [JsonPropertyName("resourceMetrics")]
    public List<OtlpResourceMetrics>? ResourceMetrics { get; set; }
}

public sealed class OtlpResourceMetrics
{
    [JsonPropertyName("resource")]
    public OtlpResource? Resource { get; set; }

    [JsonPropertyName("scopeMetrics")]
    public List<OtlpScopeMetrics>? ScopeMetrics { get; set; }
}

public sealed class OtlpScopeMetrics
{
    [JsonPropertyName("scope")]
    public OtlpScope? Scope { get; set; }

    [JsonPropertyName("metrics")]
    public List<OtlpMetric>? Metrics { get; set; }
}

public sealed class OtlpMetric
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("gauge")]
    public OtlpGauge? Gauge { get; set; }

    [JsonPropertyName("sum")]
    public OtlpSum? Sum { get; set; }

    [JsonPropertyName("histogram")]
    public OtlpHistogram? Histogram { get; set; }

    [JsonPropertyName("summary")]
    public OtlpSummary? Summary { get; set; }
}

public sealed class OtlpGauge
{
    [JsonPropertyName("dataPoints")]
    public List<OtlpNumberDataPoint>? DataPoints { get; set; }
}

public sealed class OtlpSum
{
    [JsonPropertyName("dataPoints")]
    public List<OtlpNumberDataPoint>? DataPoints { get; set; }

    [JsonPropertyName("isMonotonic")]
    public bool IsMonotonic { get; set; }
}

public sealed class OtlpHistogram
{
    [JsonPropertyName("dataPoints")]
    public List<OtlpHistogramDataPoint>? DataPoints { get; set; }
}

public sealed class OtlpSummary
{
    [JsonPropertyName("dataPoints")]
    public List<OtlpSummaryDataPoint>? DataPoints { get; set; }
}

public sealed class OtlpNumberDataPoint
{
    [JsonPropertyName("timeUnixNano")]
    public string? TimeUnixNano { get; set; }

    [JsonPropertyName("asDouble")]
    public double? AsDouble { get; set; }

    [JsonPropertyName("asInt")]
    public string? AsInt { get; set; }

    [JsonPropertyName("attributes")]
    public List<OtlpKeyValue>? Attributes { get; set; }
}

public sealed class OtlpHistogramDataPoint
{
    [JsonPropertyName("timeUnixNano")]
    public string? TimeUnixNano { get; set; }

    [JsonPropertyName("count")]
    public string? Count { get; set; }

    [JsonPropertyName("sum")]
    public double? Sum { get; set; }

    [JsonPropertyName("attributes")]
    public List<OtlpKeyValue>? Attributes { get; set; }
}

public sealed class OtlpSummaryDataPoint
{
    [JsonPropertyName("timeUnixNano")]
    public string? TimeUnixNano { get; set; }

    [JsonPropertyName("count")]
    public string? Count { get; set; }

    [JsonPropertyName("sum")]
    public double? Sum { get; set; }

    [JsonPropertyName("attributes")]
    public List<OtlpKeyValue>? Attributes { get; set; }
}

// === Logs ===

public sealed class OtlpExportLogsRequest
{
    [JsonPropertyName("resourceLogs")]
    public List<OtlpResourceLogs>? ResourceLogs { get; set; }
}

public sealed class OtlpResourceLogs
{
    [JsonPropertyName("resource")]
    public OtlpResource? Resource { get; set; }

    [JsonPropertyName("scopeLogs")]
    public List<OtlpScopeLogs>? ScopeLogs { get; set; }
}

public sealed class OtlpScopeLogs
{
    [JsonPropertyName("scope")]
    public OtlpScope? Scope { get; set; }

    [JsonPropertyName("logRecords")]
    public List<OtlpLogRecord>? LogRecords { get; set; }
}

public sealed class OtlpLogRecord
{
    [JsonPropertyName("timeUnixNano")]
    public string? TimeUnixNano { get; set; }

    [JsonPropertyName("severityNumber")]
    public int SeverityNumber { get; set; }

    [JsonPropertyName("severityText")]
    public string? SeverityText { get; set; }

    [JsonPropertyName("body")]
    public OtlpAnyValue? Body { get; set; }

    [JsonPropertyName("attributes")]
    public List<OtlpKeyValue>? Attributes { get; set; }

    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }

    [JsonPropertyName("spanId")]
    public string? SpanId { get; set; }
}

// === Common ===

public sealed class OtlpResource
{
    [JsonPropertyName("attributes")]
    public List<OtlpKeyValue>? Attributes { get; set; }
}

public sealed class OtlpScope
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }
}

public sealed class OtlpKeyValue
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = "";

    [JsonPropertyName("value")]
    public OtlpAnyValue? Value { get; set; }
}

public sealed class OtlpAnyValue
{
    [JsonPropertyName("stringValue")]
    public string? StringValue { get; set; }

    [JsonPropertyName("intValue")]
    public string? IntValue { get; set; }

    [JsonPropertyName("doubleValue")]
    public double? DoubleValue { get; set; }

    [JsonPropertyName("boolValue")]
    public bool? BoolValue { get; set; }
}
