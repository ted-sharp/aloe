using Aloe.Apps.MedockLib.Data.Entities;

namespace Aloe.Apps.MedockLib.Repositories;

/// <summary>
/// 予約リポジトリインターフェース
/// </summary>
public interface IAppointmentRepository
{
    /// <summary>
    /// IDで予約を取得します。
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
    /// 指定日の予約件数を取得します。
    /// </summary>
    Task<int> GetCountByDateAsync(DateOnly date);

    /// <summary>
    /// 指定フロア・日付のステータス別予約件数を取得します。
    /// </summary>
    Task<Dictionary<int, int>> GetStatusCountByFloorAndDateAsync(Guid floorId, DateOnly date);

    /// <summary>
    /// 日別統計用に予約の日付と開始時刻を取得します。
    /// </summary>
    Task<List<(DateOnly? ApptDate, DateTime? ApptStartAt)>> GetForDayStatsAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// 予約をIDで検索します（更新用）。
    /// </summary>
    Task<Appointment?> FindByIdAsync(Guid apptId);

    /// <summary>
    /// 監査情報を設定します。
    /// </summary>
    void SetAuditInfo(Guid userId, Guid sessionId);
}

