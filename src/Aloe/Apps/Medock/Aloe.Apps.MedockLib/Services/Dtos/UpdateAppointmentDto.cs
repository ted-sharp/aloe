namespace Aloe.Apps.MedockLib.Services.Dtos;

/// <summary>
/// 予約更新DTO
/// </summary>
public class UpdateAppointmentDto
{
    public DateOnly? Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid? FloorId { get; set; }
    public int? Status { get; set; }
}

