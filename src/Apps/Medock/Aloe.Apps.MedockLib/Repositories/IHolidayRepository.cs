using Aloe.Apps.MedockLib.Data.Entities;

namespace Aloe.Apps.MedockLib.Repositories;

/// <summary>
/// 祝日リポジトリインターフェース
/// </summary>
public interface IHolidayRepository
{
    /// <summary>
    /// 日付範囲で祝日を取得します。
    /// </summary>
    Task<List<Holiday>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);
}

