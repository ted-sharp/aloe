namespace Aloe.Apps.MedockLib.Services.Dtos;

/// <summary>
/// 予約作成DTO
/// </summary>
public class CreateAppointmentDto
{
    public DateOnly Date { get; set; }

    // TODO: int StartMin に変更予定
    public TimeOnly? StartTime { get; set; }
    // TODO: 削除予定
    public TimeOnly? EndTime { get; set; }

    public Guid? PatientId { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid FloorId { get; set; }
    public int Status { get; set; } = 0;
    public string? Memo { get; set; }
    public List<Guid> EquipmentResourceIds { get; set; } = new();
}

