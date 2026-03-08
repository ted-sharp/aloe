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

    public string CurrentPaperName => this._currentPaperName;
    public Orientation CurrentOrientation => this._currentOrientation;
    public int GridDivision => this._gridDivision;

    public PaperDefinition? CurrentPaper =>
        this._papersConfig.GetPaper(this._currentPaperName);

    public GridConfig? CurrentGridConfig =>
        this.CurrentPaper?.GetGridConfig(this._currentOrientation);

    public int CurrentColumns => this.CurrentGridConfig?.Columns ?? 36;
    public int CurrentRows => this.CurrentGridConfig?.Rows ?? 51;

    public double CurrentWidthPx => this.CurrentPaper?.GetWidthPx(this._currentOrientation) ?? 793.7;
    public double CurrentHeightPx => this.CurrentPaper?.GetHeightPx(this._currentOrientation) ?? 1122.52;

    public IEnumerable<string> AvailablePaperNames =>
        this._papersConfig.Papers.Select(p => p.Name);

    public PageConfigService(IConfiguration configuration)
    {
        this._papersConfig = new PapersConfiguration();
        configuration.GetSection("PapersConfiguration").Bind(this._papersConfig);

        this._currentPaperName = this._papersConfig.DefaultPaper;
        this._currentOrientation = this._papersConfig.GetDefaultOrientation();
        this._gridDivision = 36;
    }

    public void ChangePageSize(string paperName)
    {
        if (this._papersConfig.GetPaper(paperName) != null && this._currentPaperName != paperName)
        {
            this._currentPaperName = paperName;
            this.NotifyConfigChanged();
        }
    }

    public void ToggleOrientation()
    {
        this._currentOrientation = this._currentOrientation == Orientation.Portrait
            ? Orientation.Landscape
            : Orientation.Portrait;
        this.NotifyConfigChanged();
    }

    public void SetGridDivision(int division)
    {
        if (division != this._gridDivision && (division == 36 || division == 30 || division == 24 || division == 18))
        {
            this._gridDivision = division;
            this.NotifyConfigChanged();
        }
    }

    private void NotifyConfigChanged()
    {
        OnPageConfigChanged?.Invoke();
    }
}
