namespace Aloe.Apps.MedockLib.Services.Dtos.Appointments;
/// <summary>
/// 予約DTO
/// </summary>
public class AppointmentDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public int StartMin { get; set; }
    public string? PatientName { get; set; }
    public Guid? PatientId { get; set; }
    public string? OrganizationName { get; set; }
    public Guid? OrganizationId { get; set; }
    public string? FloorName { get; set; }
    public Guid? FloorId { get; set; }
    public int Status { get; set; }
    public string? Memo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<EquipmentResourceDto> EquipmentResources { get; set; } = new();
}

/// <summary>
/// 予約に関連する機器リソースDTO
/// </summary>
public class EquipmentResourceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = String.Empty;
}

