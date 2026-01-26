using Aloe.Apps.RazorReportLib.Models;
using Microsoft.Extensions.Configuration;

namespace Aloe.Apps.RazorReportServer.Services;

public class PageConfigService
{
    private readonly PapersConfiguration _papersConfig;
    private string _currentPaperName;
    private Orientation _currentOrientation;
    private int _gridDivision;

    public event Action? OnPageConfigChanged;

    public string CurrentPaperName => _currentPaperName;
    public Orientation CurrentOrientation => _currentOrientation;
    public int GridDivision => _gridDivision;

    public PaperDefinition? CurrentPaper =>
        _papersConfig.GetPaper(_currentPaperName);

    public GridConfig? CurrentGridConfig =>
        CurrentPaper?.GetGridConfig(_currentOrientation);

    public int CurrentColumns => CurrentGridConfig?.Columns ?? 36;
    public int CurrentRows => CurrentGridConfig?.Rows ?? 51;

    public double CurrentWidthPx => CurrentPaper?.GetWidthPx(_currentOrientation) ?? 793.7;
    public double CurrentHeightPx => CurrentPaper?.GetHeightPx(_currentOrientation) ?? 1122.52;

    public IEnumerable<string> AvailablePaperNames =>
        _papersConfig.Papers.Select(p => p.Name);

    public PageConfigService(IConfiguration configuration)
    {
        _papersConfig = new PapersConfiguration();
        configuration.GetSection("PapersConfiguration").Bind(_papersConfig);

        _currentPaperName = _papersConfig.DefaultPaper;
        _currentOrientation = _papersConfig.GetDefaultOrientation();
        _gridDivision = 36;
    }

    public void ChangePageSize(string paperName)
    {
        if (_papersConfig.GetPaper(paperName) != null && _currentPaperName != paperName)
        {
            _currentPaperName = paperName;
            NotifyConfigChanged();
        }
    }

    public void ToggleOrientation()
    {
        _currentOrientation = _currentOrientation == Orientation.Portrait
            ? Orientation.Landscape
            : Orientation.Portrait;
        NotifyConfigChanged();
    }

    public void SetGridDivision(int division)
    {
        if (division != _gridDivision && (division == 36 || division == 30 || division == 24 || division == 18))
        {
            _gridDivision = division;
            NotifyConfigChanged();
        }
    }

    private void NotifyConfigChanged()
    {
        OnPageConfigChanged?.Invoke();
    }
}
