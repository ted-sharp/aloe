using Aloe.Apps.MedockLib.Services.Dtos;

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

