using Aloe.Apps.MedockLib.Data.Entities;

namespace Aloe.Apps.MedockLib.Repositories;

/// <summary>
/// 予約リポジトリインターフェース
/// </summary>
/// <remarks>
/// 命名規則：
/// - Get... : 読み取り用（AsNoTracking、変更追跡なし）
/// - FindForUpdate... : 更新用（Tracking、変更追跡あり）
/// </remarks>
public interface IAppointmentRepository
{
    /// <summary>
    /// IDで予約を取得します（読み取り用、変更追跡なし）。
    /// </summary>
    Task<Appointment?> GetByIdAsync(Guid apptId);

    /// <summary>
    /// 日付範囲で予約を取得します。
    /// </summary>
    Task<List<Appointment>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// フロアと日付で予約を取得します。
    /// </summary>
    Task<List<Appointment>> GetByFloorAndDateAsync(Guid floorId, DateOnly date);

    /// <summary>
    /// 患者IDで予約を取得します。
    /// </summary>
    Task<List<Appointment>> GetByPatientIdAsync(Guid ptId);

    /// <summary>
    /// 団体IDで予約を取得します。
    /// </summary>
    Task<List<Appointment>> GetByOrganizationIdAsync(Guid orgId);

    /// <summary>
    /// 予約を追加します。
    /// </summary>
    Task AddAsync(Appointment appointment);

    /// <summary>
    /// 予約を更新します。
    /// </summary>
    Task UpdateAsync(Appointment appointment);

    /// <summary>
    /// 予約を論理削除します。
    /// </summary>
    Task DeleteAsync(Guid apptId);



    /// <summary>
    /// IDで予約を取得します（更新用、変更追跡あり）。
    /// </summary>
    Task<Appointment?> FindForUpdateAsync(Guid apptId);
}

