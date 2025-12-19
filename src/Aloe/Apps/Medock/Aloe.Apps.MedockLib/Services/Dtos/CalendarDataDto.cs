namespace Aloe.Apps.MedockLib.Services.Dtos;

/// <summary>
/// カレンダー表示用データDTO（JavaScript用）
/// </summary>
public class CalendarDataDto
{
    public List<AppointmentDataDto> Appointments { get; set; } = new();
    public Dictionary<string, MainStatsDataDto> MainStats { get; set; } = new();
    public Dictionary<string, string> Holidays { get; set; } = new();
}

/// <summary>
/// 予約データDTO（JavaScript用）
/// </summary>
public class AppointmentDataDto
{
    public string Id { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string? PatientName { get; set; }
    public Guid? PatientId { get; set; }
    public string? OrganizationName { get; set; }
    public Guid? OrganizationId { get; set; }
    public string? FloorName { get; set; }
    public Guid? FloorId { get; set; }
    public int Status { get; set; }
}

/// <summary>
/// Mainリソース統計データDTO（JavaScript用）
/// </summary>
public class MainStatsDataDto
{
    public List<SlotDataDto> Slots { get; set; } = new();
    public bool IsGrayedOut { get; set; }
}

/// <summary>
/// スロットデータDTO（JavaScript用）
/// </summary>
public class SlotDataDto
{
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Cap { get; set; }
    public int Available { get; set; }
    public bool IsGrayedOut { get; set; }
    public int FilteredCount { get; set; }
}

