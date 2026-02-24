using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class FacilityHolidaySeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingFacilityHolidays = await context.FacilityHolidays
            .AnyAsync(fh => fh.FacilityId == facilityId && !fh.IsDeleted);

        if (existingFacilityHolidays)
        {
            Console.WriteLine("[SKIP] FacilityHolidays already exist. Skipping facility holiday seed.");
            return;
        }

        Console.WriteLine("[INFO] Creating facility holiday seed data...");

        // ローカルファンクション: 施設祝日オブジェクト生成を簡潔化
        FacilityHoliday FH(int year, int month, int day, string name, string desc = "") =>
            new()
            {
                HolidayId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                HolidayDate = new DateOnly(year, month, day),
                HolidayName = name,
                HolidayDesc = desc,
                IsDeleted = false
            };

        // Collection expressionsとローカルファンクションを使用
        // 過去3年（2022, 2023, 2024）と未来1年（2026）のデータを作成
        // Holiday テーブルに登録されていない平日（祝日と祝日の間など）だけを追加
        FacilityHoliday[] holidays = [
            // 2022年
            // GW: 4/29(祝日)～5/1-2(平日)～5/3-5(祝日)
            FH(2022, 4, 30, "ゴールデンウィーク", "GW補間休"),
            FH(2022, 5, 1, "ゴールデンウィーク", "GW補間休"),
            FH(2022, 5, 2, "ゴールデンウィーク", "GW補間休"),
            // お盆: 8/11(祝)～8/12-15(平日)
            FH(2022, 8, 12, "夏季休業", "お盆補間休"),
            FH(2022, 8, 13, "夏季休業", "お盆補間休"),
            FH(2022, 8, 14, "夏季休業", "お盆補間休"),
            FH(2022, 8, 15, "夏季休業", "お盆補間休"),
            // SW: 9/19(祝)～9/20-22(平日)～9/23(祝)
            FH(2022, 9, 20, "シルバーウィーク", "SW補間休"),
            FH(2022, 9, 21, "シルバーウィーク", "SW補間休"),
            FH(2022, 9, 22, "シルバーウィーク", "SW補間休"),
            // 年末年始: 12/29-31(祝)とは別に施設休業を延長
            FH(2022, 12, 30, "年末年始休業", "年末年始補間休"),
            FH(2022, 12, 31, "年末年始休業", "年末年始補間休"),

            // 2023年
            FH(2023, 4, 30, "ゴールデンウィーク", "GW補間休"),
            FH(2023, 5, 1, "ゴールデンウィーク", "GW補間休"),
            FH(2023, 5, 2, "ゴールデンウィーク", "GW補間休"),
            FH(2023, 8, 12, "夏季休業", "お盆補間休"),
            FH(2023, 8, 13, "夏季休業", "お盆補間休"),
            FH(2023, 8, 14, "夏季休業", "お盆補間休"),
            FH(2023, 8, 15, "夏季休業", "お盆補間休"),
            // SW: 9/18(祝)～9/19-22(平日)～9/23(祝)
            FH(2023, 9, 19, "シルバーウィーク", "SW補間休"),
            FH(2023, 9, 20, "シルバーウィーク", "SW補間休"),
            FH(2023, 9, 21, "シルバーウィーク", "SW補間休"),
            FH(2023, 9, 22, "シルバーウィーク", "SW補間休"),
            FH(2023, 12, 30, "年末年始休業", "年末年始補間休"),
            FH(2023, 12, 31, "年末年始休業", "年末年始補間休"),

            // 2024年
            FH(2024, 4, 30, "ゴールデンウィーク", "GW補間休"),
            FH(2024, 5, 1, "ゴールデンウィーク", "GW補間休"),
            FH(2024, 5, 2, "ゴールデンウィーク", "GW補間休"),
            FH(2024, 8, 12, "夏季休業", "お盆補間休"),
            FH(2024, 8, 13, "夏季休業", "お盆補間休"),
            FH(2024, 8, 14, "夏季休業", "お盆補間休"),
            FH(2024, 8, 15, "夏季休業", "お盆補間休"),
            // SW: 9/16(祝)～9/17-22(平日)～9/23(祝)
            FH(2024, 9, 17, "シルバーウィーク", "SW補間休"),
            FH(2024, 9, 18, "シルバーウィーク", "SW補間休"),
            FH(2024, 9, 19, "シルバーウィーク", "SW補間休"),
            FH(2024, 9, 20, "シルバーウィーク", "SW補間休"),
            FH(2024, 9, 21, "シルバーウィーク", "SW補間休"),
            FH(2024, 9, 22, "シルバーウィーク", "SW補間休"),
            FH(2024, 12, 30, "年末年始休業", "年末年始補間休"),
            FH(2024, 12, 31, "年末年始休業", "年末年始補間休"),

            // 2025年
            FH(2025, 4, 30, "ゴールデンウィーク", "GW補間休"),
            FH(2025, 5, 1, "ゴールデンウィーク", "GW補間休"),
            FH(2025, 5, 2, "ゴールデンウィーク", "GW補間休"),
            FH(2025, 8, 12, "夏季休業", "お盆補間休"),
            FH(2025, 8, 13, "夏季休業", "お盆補間休"),
            FH(2025, 8, 14, "夏季休業", "お盆補間休"),
            FH(2025, 8, 15, "夏季休業", "お盆補間休"),
            // SW: 9/15(祝)～9/16-22(平日)～9/23(祝)
            FH(2025, 9, 16, "シルバーウィーク", "SW補間休"),
            FH(2025, 9, 17, "シルバーウィーク", "SW補間休"),
            FH(2025, 9, 18, "シルバーウィーク", "SW補間休"),
            FH(2025, 9, 19, "シルバーウィーク", "SW補間休"),
            FH(2025, 9, 20, "シルバーウィーク", "SW補間休"),
            FH(2025, 9, 21, "シルバーウィーク", "SW補間休"),
            FH(2025, 9, 22, "シルバーウィーク", "SW補間休"),

            // 2026年（未来1年）
            FH(2026, 4, 30, "ゴールデンウィーク", "GW補間休"),
            FH(2026, 5, 1, "ゴールデンウィーク", "GW補間休"),
            FH(2026, 5, 2, "ゴールデンウィーク", "GW補間休"),
            FH(2026, 8, 12, "夏季休業", "お盆補間休"),
            FH(2026, 8, 13, "夏季休業", "お盆補間休"),
            FH(2026, 8, 14, "夏季休業", "お盆補間休"),
            FH(2026, 8, 15, "夏季休業", "お盆補間休"),
            // SW: 9/21(祝)～9/22(平日)～9/23(祝)
            FH(2026, 9, 22, "シルバーウィーク", "SW補間休"),
            FH(2026, 12, 30, "年末年始休業", "年末年始補間休"),
            FH(2026, 12, 31, "年末年始休業", "年末年始補間休"),
        ];

        // 監査フィールドを初期化
        foreach (var holiday in holidays)
        {
            SeederHelper.InitializeAuditFields(holiday, dateTimeProvider);
        }

        context.FacilityHolidays.AddRange(holidays);
        Console.WriteLine($"  [+] FacilityHolidays: {holidays.Length} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}
