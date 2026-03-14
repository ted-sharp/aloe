namespace Aloe.Apps.DashboardLib.OtelViewer.Models;

public sealed class OtelLogRecord
{
    public required string ServiceName { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required string Severity { get; init; }

    public int SeverityNumber { get; init; }

    public string? Body { get; init; }

    public string? TraceId { get; init; }

    public string? SpanId { get; init; }

    public IReadOnlyDictionary<string, string> Attributes { get; init; } = new Dictionary<string, string>();
}
