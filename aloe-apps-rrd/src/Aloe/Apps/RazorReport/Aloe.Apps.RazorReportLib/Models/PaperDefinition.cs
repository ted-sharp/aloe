namespace Aloe.Apps.RazorReportLib.Models;

public class PaperDefinition
{
    private const double DPI = 96.0;
    private const double MM_TO_INCH = 1.0 / 25.4;

    public string Name { get; set; } = String.Empty;
    public double WidthMm { get; set; }
    public double HeightMm { get; set; }
    public int PortraitColumns { get; set; }
    public int PortraitRows { get; set; }
    public int LandscapeColumns { get; set; }
    public int LandscapeRows { get; set; }

    public double GetWidthPx(Orientation orientation)
    {
        double mm = orientation == Orientation.Portrait ? this.WidthMm : this.HeightMm;
        return mm * MM_TO_INCH * DPI;
    }

    public double GetHeightPx(Orientation orientation)
    {
        double mm = orientation == Orientation.Portrait ? this.HeightMm : this.WidthMm;
        return mm * MM_TO_INCH * DPI;
    }

    public GridConfig GetGridConfig(Orientation orientation)
    {
        int columns = orientation == Orientation.Portrait ? this.PortraitColumns : this.LandscapeColumns;
        int rows = orientation == Orientation.Portrait ? this.PortraitRows : this.LandscapeRows;

        return new GridConfig(columns, rows);
    }
}
