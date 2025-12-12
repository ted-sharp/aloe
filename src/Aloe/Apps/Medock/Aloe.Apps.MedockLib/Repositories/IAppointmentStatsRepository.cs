namespace Aloe.Apps.MedockLib.Repositories;

/// <summary>
/// 予約統計リポジトリインターフェース
/// </summary>
public interface IAppointmentStatsRepository
{
    /// <summary>
    /// 指定日の予約件数を取得します。
    /// </summary>
    Task<int> GetCountByDateAsync(DateOnly date);

    /// <summary>
    /// 指定フロア・日付のステータス別予約件数を取得します。
    /// </summary>
    Task<Dictionary<int, int>> GetStatusCountByFloorAndDateAsync(Guid floorId, DateOnly date);

    /// <summary>
    /// 日別統計用の予約データ（日付と開始時間のみ）を取得します。
    /// </summary>
    Task<List<(DateOnly? ApptDate, DateTime? ApptStartAt)>> GetForDayStatsAsync(DateOnly startDate, DateOnly endDate);
}
