using Aloe.Apps.MedockLib.Data.Entities;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 予約統計サービスインターフェース
/// </summary>
public interface IAppointmentStatisticsService
{
    /// <summary>
    /// 指定期間のMainリソース統計を取得します
    /// </summary>
    /// <param name="startDate">開始日</param>
    /// <param name="endDate">終了日</param>
    /// <returns>日付文字列をキーとした統計Dictionary</returns>
    Task<Dictionary<string, MainStatsDto>> GetMainStatsAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// 指定フロア・日付のステータス別予約件数を取得します
    /// </summary>
    /// <param name="floorId">フロアID</param>
    /// <param name="date">日付</param>
    /// <returns>ステータスコードをキーとした件数Dictionary</returns>
    Task<Dictionary<int, int>> GetStatusCountByFloorAndDateAsync(Guid floorId, DateOnly date);
}
