namespace Aloe.Apps.RazorReportServer.Services;

public class ZoomService
{
    private double _zoomLevel = 1.0;

    public event Action? OnZoomChanged;

    public double ZoomLevel => this._zoomLevel;

    public double[] PredefinedZoomLevels => new[] { 0.5, 0.75, 1.0, 1.25, 1.5, 2.0 };

    public int ZoomPercentage => (int)(this._zoomLevel * 100);

    public void ZoomIn()
    {
        var nextLevel = this.PredefinedZoomLevels.FirstOrDefault(z => z > this._zoomLevel);
        if (nextLevel > 0)
        {
            this.SetZoom(nextLevel);
        }
    }

    public void ZoomOut()
    {
        var prevLevel = this.PredefinedZoomLevels.LastOrDefault(z => z < this._zoomLevel);
        if (prevLevel > 0)
        {
            this.SetZoom(prevLevel);
        }
    }

    public void SetZoom(double zoomLevel)
    {
        if (zoomLevel > 0 && zoomLevel != this._zoomLevel)
        {
            this._zoomLevel = zoomLevel;
            this.NotifyZoomChanged();
        }
    }

    public void ResetZoom()
    {
        this.SetZoom(1.0);
    }

    public void FitToPage(double viewportWidth, double viewportHeight, double contentWidth, double contentHeight)
    {
        if (contentWidth <= 0 || contentHeight <= 0)
            return;

        double zoomX = viewportWidth / contentWidth;
        double zoomY = viewportHeight / contentHeight;
        double fitZoom = Math.Min(zoomX, zoomY);

        // Round to nearest predefined level or use calculated value
        var closestPredefined = this.PredefinedZoomLevels
            .OrderBy(z => Math.Abs(z - fitZoom))
            .First();

        this.SetZoom(closestPredefined);
    }

    private void NotifyZoomChanged()
    {
        OnZoomChanged?.Invoke();
    }
}
