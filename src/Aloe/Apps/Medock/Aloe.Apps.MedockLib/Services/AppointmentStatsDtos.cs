namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// Mainリソース統計DTO
/// </summary>
public class MainStatsDto
{
    public int AmCount { get; set; }
    public int PmCount { get; set; }
    public int AmMax { get; set; } = 10;
    public int PmMax { get; set; } = 10;
}
