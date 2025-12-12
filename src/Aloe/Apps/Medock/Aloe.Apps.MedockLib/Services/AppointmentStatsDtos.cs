namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 日別統計DTO
/// </summary>
public class DayStatsDto
{
    public int AmCount { get; set; }
    public int PmCount { get; set; }
    public int AmMax { get; set; } = 10;
    public int PmMax { get; set; } = 10;
}
