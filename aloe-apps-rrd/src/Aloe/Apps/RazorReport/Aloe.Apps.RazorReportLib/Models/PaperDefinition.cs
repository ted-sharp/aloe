namespace Aloe.Apps.RazorReportLib.Models;

public class PaperDefinition
{
    private const double DPI = 96.0;
    private const double MM_TO_INCH = 1.0 / 25.4;

    public string Name { get; set; } = string.Empty;
    public double WidthMm { get; set; }
    public double HeightMm { get; set; }
    public int PortraitColumns { get; set; }
    public int PortraitRows { get; set; }
    public int LandscapeColumns { get; set; }
    public int LandscapeRows { get; set; }

    public double GetWidthPx(Orientation orientation)
    {
        double mm = orientation == Orientation.Portrait ? WidthMm : HeightMm;
        return mm * MM_TO_INCH * DPI;
    }

    public double GetHeightPx(Orientation orientation)
    {
        double mm = orientation == Orientation.Portrait ? HeightMm : WidthMm;
        return mm * MM_TO_INCH * DPI;
    }

    public GridConfig GetGridConfig(Orientation orientation)
    {
        int columns = orientation == Orientation.Portrait ? PortraitColumns : LandscapeColumns;
        int rows = orientation == Orientation.Portrait ? PortraitRows : LandscapeRows;

        return new GridConfig(columns, rows);
    }
}
