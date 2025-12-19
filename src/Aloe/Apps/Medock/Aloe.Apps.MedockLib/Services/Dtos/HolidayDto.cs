namespace Aloe.Apps.MedockLib.Services.Dtos;

/// <summary>
/// 祝日DTO
/// </summary>
public class HolidayDto
{
    public DateOnly Date { get; set; }
    public string Name { get; set; } = String.Empty;
}

