namespace Aloe.Apps.WindowsServiceMonitorLib.OtelViewer.Models;

public sealed class OtelMetricPoint
{
    public required string ServiceName { get; init; }

    public required string MetricName { get; init; }

    public string? Description { get; init; }

    public string? Unit { get; init; }

    public required OtelMetricType MetricType { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public double Value { get; init; }

    public IReadOnlyDictionary<string, string> Attributes { get; init; } = new Dictionary<string, string>();
}

public enum OtelMetricType
{
    Gauge = 0,
    Sum = 1,
    Histogram = 2,
    Summary = 3,
}
