namespace Aloe.Apps.MedockLib.Services.Dtos.Appointments;

/// <summary>
/// 予約作成DTO
/// </summary>
public class CreateAppointmentDto
{
    public DateOnly Date { get; set; }
    public int? StartMin { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid FloorId { get; set; }
    public int Status { get; set; } = 0;
    public string? Memo { get; set; }
    public List<Guid> EquipmentResourceIds { get; set; } = new();
}

