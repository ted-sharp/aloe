using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

/// <summary>
/// Seeder共通処理を提供する基底クラス
/// </summary>
internal static class SeederHelper
{
    /// <summary>
    /// 指定期間内にデータが既に存在するかチェック
    /// </summary>
    public static async Task<bool> HasDataInRangeAsync<T>(
        DbSet<T> dbSet,
        DateOnly startDate,
        DateOnly endDate,
        Func<T, DateOnly> dateSelector,
        Func<T, bool>? additionalFilter = null) where T : class
    {
        var query = dbSet.AsQueryable();

        // 残念ながらEF Coreの式木ではジェネリックなDateOnlyフィルタが難しいため、
        // 全件取得後にフィルタリング（データ量が少ないSeeder用途なので許容）
        var items = await dbSet.ToListAsync();

        return items.Any(item =>
        {
            var date = dateSelector(item);
            var inRange = date >= startDate && date <= endDate;
            var passesFilter = additionalFilter == null || additionalFilter(item);
            return inRange && passesFilter;
        });
    }

    /// <summary>
    /// 監査フィールドを初期化
    /// </summary>
    public static void InitializeAuditFields(IAuditableEntity entity, IDateTimeProvider dateTimeProvider)
    {
        var now = dateTimeProvider.Now;
        entity.CreatedAt = now;
        entity.CreatedUserId = Guid.Empty;
        entity.CreatedSessionId = Guid.Empty;
        entity.UpdatedAt = now;
        entity.UpdatedUserId = Guid.Empty;
        entity.UpdatedSessionId = Guid.Empty;
    }

    /// <summary>
    /// 祝日セットを読み込み
    /// </summary>
    public static async Task<HashSet<DateOnly>> LoadHolidaySetAsync(MedockDbContext context)
    {
        return await SeedBusinessCalendar.LoadHolidaySetAsync(context);
    }

    /// <summary>
    /// 今日を基準とした期間を取得（過去3年～未来1年）
    /// </summary>
    public static (DateOnly StartDate, DateOnly EndDate) GetDefaultDateRange(IDateTimeProvider dateTimeProvider)
    {
        var today = dateTimeProvider.TodayDateOnly;
        return (today.AddYears(-3), today.AddYears(1));
    }

    /// <summary>
    /// 週末かどうかを判定
    /// </summary>
    public static bool IsWeekend(DateOnly date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }

    /// <summary>
    /// 土曜日かどうかを判定
    /// </summary>
    public static bool IsSaturday(DateOnly date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday;
    }

    /// <summary>
    /// 日曜日かどうかを判定
    /// </summary>
    public static bool IsSunday(DateOnly date)
    {
        return date.DayOfWeek == DayOfWeek.Sunday;
    }

    /// <summary>
    /// 営業日かどうかを判定（祝日・日曜は休み）
    /// </summary>
    public static bool IsBusinessDay(DateOnly date, HashSet<DateOnly> holidays)
    {
        return !IsSunday(date) && !holidays.Contains(date);
    }

    /// <summary>
    /// 標準営業時間のスロット（15分単位）
    /// </summary>
    public static class TimeSlots
    {
        public static readonly string[] MorningSlots = ["09:00", "09:15", "09:30", "09:45", "10:00", "10:15", "10:30", "10:45", "11:00", "11:15", "11:30", "11:45"];
        public static readonly string[] AfternoonSlots = ["13:00", "13:15", "13:30", "13:45", "14:00", "14:15", "14:30", "14:45", "15:00", "15:15", "15:30", "15:45", "16:00", "16:15", "16:30", "16:45", "17:00"];
        public static readonly string[] SaturdayMorningSlots = ["09:00", "09:30", "10:00", "10:30", "11:00", "11:30"];
        public static readonly string[] EarlyMorningSlots = ["07:00", "07:30", "08:00", "08:30"];
        public static readonly string[] EveningSlots = ["18:00", "18:30", "19:00", "19:30"];
        public static readonly string[] LunchSlots = ["12:00", "12:15", "12:30", "12:45"];
    }
}
