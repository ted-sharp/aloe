namespace Aloe.Apps.DashboardLib.OtelViewer.Models;

public sealed class OtelSpan
{
    public required string TraceId { get; init; }

    public required string SpanId { get; init; }

    public string? ParentSpanId { get; init; }

    public required string ServiceName { get; init; }

    public required string Name { get; init; }

    public required DateTimeOffset StartTime { get; init; }

    public required DateTimeOffset EndTime { get; init; }

    public TimeSpan Duration => this.EndTime - this.StartTime;

    public required OtelSpanKind Kind { get; init; }

    public required OtelStatusCode StatusCode { get; init; }

    public string? StatusMessage { get; init; }

    public IReadOnlyDictionary<string, string> Attributes { get; init; } = new Dictionary<string, string>();

    public IReadOnlyList<OtelSpanEvent> Events { get; init; } = [];
}

public sealed class OtelSpanEvent
{
    public required string Name { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public IReadOnlyDictionary<string, string> Attributes { get; init; } = new Dictionary<string, string>();
}

public enum OtelSpanKind
{
    Unspecified = 0,
    Internal = 1,
    Server = 2,
    Client = 3,
    Producer = 4,
    Consumer = 5,
}

public enum OtelStatusCode
{
    Unset = 0,
    Ok = 1,
    Error = 2,
}
