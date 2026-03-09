namespace Aloe.Apps.WindowsServiceMonitorLib.OtelViewer.Models;

public sealed class OtelViewerOptions
{
    public const string SectionName = "OtelViewer";

    public bool Enabled { get; set; } = true;

    public int MaxTraces { get; set; } = 10000;

    public int MaxMetricPoints { get; set; } = 50000;

    public int MaxLogRecords { get; set; } = 50000;
}
