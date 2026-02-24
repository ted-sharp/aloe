namespace Aloe.Apps.RazorReportServer.Services;

public class ZoomService
{
    private double _zoomLevel = 1.0;

    public event Action? OnZoomChanged;

    public double ZoomLevel => _zoomLevel;

    public double[] PredefinedZoomLevels => new[] { 0.5, 0.75, 1.0, 1.25, 1.5, 2.0 };

    public int ZoomPercentage => (int)(_zoomLevel * 100);

    public void ZoomIn()
    {
        var nextLevel = PredefinedZoomLevels.FirstOrDefault(z => z > _zoomLevel);
        if (nextLevel > 0)
        {
            SetZoom(nextLevel);
        }
    }

    public void ZoomOut()
    {
        var prevLevel = PredefinedZoomLevels.LastOrDefault(z => z < _zoomLevel);
        if (prevLevel > 0)
        {
            SetZoom(prevLevel);
        }
    }

    public void SetZoom(double zoomLevel)
    {
        if (zoomLevel > 0 && zoomLevel != _zoomLevel)
        {
            _zoomLevel = zoomLevel;
            NotifyZoomChanged();
        }
    }

    public void ResetZoom()
    {
        SetZoom(1.0);
    }

    public void FitToPage(double viewportWidth, double viewportHeight, double contentWidth, double contentHeight)
    {
        if (contentWidth <= 0 || contentHeight <= 0)
            return;

        double zoomX = viewportWidth / contentWidth;
        double zoomY = viewportHeight / contentHeight;
        double fitZoom = Math.Min(zoomX, zoomY);

        // Round to nearest predefined level or use calculated value
        var closestPredefined = PredefinedZoomLevels
            .OrderBy(z => Math.Abs(z - fitZoom))
            .First();

        SetZoom(closestPredefined);
    }

    private void NotifyZoomChanged()
    {
        OnZoomChanged?.Invoke();
    }
}
