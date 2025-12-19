using Aloe.Apps.MedockLib.Data;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal enum SeedDayType
{
    Closed = 0,
    WeekdayFull = 1,
    SaturdayMorning = 2,
    IrregularOpen = 3,
}

internal sealed record SeedDayContext(
    SeedDayType DayType,
    bool IsHoliday,
    bool IsWeekend,
    string? DayMemo);

/// <summary>
/// Seeder用の営業カレンダー判定（平日/土曜午前/日祝休み＋少数例外）。
/// 本番の業務ロジックではなく、データ生成の一貫性のための簡易ルール。
/// </summary>
internal static class SeedBusinessCalendar
{
    public static async Task<HashSet<DateOnly>> LoadHolidaySetAsync(MedockDbContext context)
    {
        var holidays = await context.Holidays
            .Where(h => !h.IsDeleted)
            .Select(h => h.HolidayDate)
            .ToListAsync();
        return holidays.ToHashSet();
    }

    public static SeedDayContext GetDayContext(DateOnly date, HashSet<DateOnly> holidays, Random random)
    {
        var dow = date.DayOfWeek;
        var isWeekend = dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;
        var isHoliday = holidays.Contains(date);

        // 例外営業（少数）
        // - 日祝でも 1% 程度は「臨時営業」扱いで午前のみ開ける
        var irregularOpen = (isHoliday || dow == DayOfWeek.Sunday) && random.Next(100) < 1;
        if (irregularOpen)
        {
            return new SeedDayContext(
                SeedDayType.IrregularOpen,
                IsHoliday: isHoliday,
                IsWeekend: true,
                DayMemo: isHoliday ? "祝日イレギュラー営業" : "日曜イレギュラー営業");
        }

        // 休診日（原則）
        if (dow == DayOfWeek.Sunday || isHoliday)
        {
            return new SeedDayContext(SeedDayType.Closed, isHoliday, isWeekend, null);
        }

        // 土曜午前
        if (dow == DayOfWeek.Saturday)
        {
            return new SeedDayContext(SeedDayType.SaturdayMorning, isHoliday, isWeekend, null);
        }

        // 平日フル（稀に臨時休診）
        var irregularClose = random.Next(1000) < 3; // 0.3% 程度
        if (irregularClose)
        {
            return new SeedDayContext(SeedDayType.Closed, isHoliday, isWeekend, "臨時休診");
        }

        return new SeedDayContext(SeedDayType.WeekdayFull, isHoliday, isWeekend, null);
    }
}




