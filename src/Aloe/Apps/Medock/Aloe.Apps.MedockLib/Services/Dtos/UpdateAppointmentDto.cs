namespace Aloe.Apps.MedockLib.Services.Dtos;

/// <summary>
/// 予約更新DTO
/// </summary>
public class UpdateAppointmentDto
{
    public DateOnly? Date { get; set; }

    // TODO: int StartMin に変更予定
    public TimeOnly? StartTime { get; set; }
    // TODO: 削除予定
    public TimeOnly? EndTime { get; set; }

    public Guid? PatientId { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid? FloorId { get; set; }
    public int? Status { get; set; }
    public string? Memo { get; set; }
    public List<Guid> EquipmentResourceIds { get; set; } = new();

    /// <summary>
    /// 楽観的ロック用：クライアント側で読み込んだ時点の最終更新日時
    /// 更新時にこの値とデータベースの値を比較して、他のユーザーによる更新を検出
    /// </summary>
    public DateTime? ExpectedUpdatedAt { get; set; }
}

