namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 予約サービスインターフェース
/// </summary>
public interface IAppointmentService
{


    /// <summary>
    /// 指定期間の予約一覧を取得します
    /// </summary>
    /// <param name="startDate">開始日</param>
    /// <param name="endDate">終了日</param>
    /// <returns>予約DTOのリスト</returns>
    Task<List<AppointmentDto>> GetAppointmentsAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// 予約を取得します
    /// </summary>
    /// <param name="apptId">予約ID</param>
    /// <returns>予約DTO</returns>
    Task<AppointmentDto?> GetAppointmentAsync(Guid apptId);

    /// <summary>
    /// 予約を作成します
    /// </summary>
    /// <param name="dto">作成する予約データ</param>
    /// <returns>作成された予約DTO</returns>
    Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto dto);

    /// <summary>
    /// 予約を更新します
    /// </summary>
    /// <param name="apptId">予約ID</param>
    /// <param name="dto">更新する予約データ</param>
    /// <returns>更新された予約DTO</returns>
    Task<AppointmentDto?> UpdateAppointmentAsync(Guid apptId, UpdateAppointmentDto dto);

    /// <summary>
    /// 予約を削除します
    /// </summary>
    /// <param name="apptId">予約ID</param>
    /// <returns>削除成功したかどうか</returns>
    Task<bool> DeleteAppointmentAsync(Guid apptId);

    /// <summary>
    /// 指定期間の祝日を取得します
    /// </summary>
    /// <param name="startDate">開始日</param>
    /// <param name="endDate">終了日</param>
    /// <returns>祝日DTOのリスト</returns>
    Task<List<HolidayDto>> GetHolidaysAsync(DateOnly startDate, DateOnly endDate);
}



/// <summary>
/// 予約DTO
/// </summary>
public class AppointmentDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? PatientName { get; set; }
    public Guid? PatientId { get; set; }
    public string? OrganizationName { get; set; }
    public Guid? OrganizationId { get; set; }
    public string? FloorName { get; set; }
    public Guid? FloorId { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// 予約作成DTO
/// </summary>
public class CreateAppointmentDto
{
    public DateOnly Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public Guid PatientId { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid FloorId { get; set; }
    public int Status { get; set; } = 0;
}

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

/// <summary>
/// 祝日DTO
/// </summary>
public class HolidayDto
{
    public DateOnly Date { get; set; }
    public string Name { get; set; } = String.Empty;
}

