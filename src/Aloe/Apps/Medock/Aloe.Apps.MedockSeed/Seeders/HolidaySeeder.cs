using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class HolidaySeeder
{
    public static async Task SeedAsync(MedockDbContext context)
    {
        var existingHolidays = await context.Holidays.AnyAsync();
        if (existingHolidays)
        {
            Console.WriteLine("[SKIP] Holidays already exist. Skipping holiday seed.");
        }
        else
        {
            Console.WriteLine("[INFO] Creating holiday seed data...");
            var holidays = new List<Holiday>
            {
                // 2025年
                new() { HolidayDate = new DateOnly(2025, 1, 1), HolidayName = "元日" },
                new() { HolidayDate = new DateOnly(2025, 1, 13), HolidayName = "成人の日" },
                new() { HolidayDate = new DateOnly(2025, 2, 11), HolidayName = "建国記念の日" },
                new() { HolidayDate = new DateOnly(2025, 2, 23), HolidayName = "天皇誕生日" },
                new() { HolidayDate = new DateOnly(2025, 2, 24), HolidayName = "振替休日" },
                new() { HolidayDate = new DateOnly(2025, 3, 20), HolidayName = "春分の日" },
                new() { HolidayDate = new DateOnly(2025, 4, 29), HolidayName = "昭和の日" },
                new() { HolidayDate = new DateOnly(2025, 5, 3), HolidayName = "憲法記念日" },
                new() { HolidayDate = new DateOnly(2025, 5, 4), HolidayName = "みどりの日" },
                new() { HolidayDate = new DateOnly(2025, 5, 5), HolidayName = "こどもの日" },
                new() { HolidayDate = new DateOnly(2025, 5, 6), HolidayName = "振替休日" },
                new() { HolidayDate = new DateOnly(2025, 7, 21), HolidayName = "海の日" },
                new() { HolidayDate = new DateOnly(2025, 8, 11), HolidayName = "山の日" },
                new() { HolidayDate = new DateOnly(2025, 9, 15), HolidayName = "敬老の日" },
                new() { HolidayDate = new DateOnly(2025, 9, 23), HolidayName = "秋分の日" },
                new() { HolidayDate = new DateOnly(2025, 10, 13), HolidayName = "スポーツの日" },
                new() { HolidayDate = new DateOnly(2025, 11, 3), HolidayName = "文化の日" },
                new() { HolidayDate = new DateOnly(2025, 11, 23), HolidayName = "勤労感謝の日" },
                new() { HolidayDate = new DateOnly(2025, 11, 24), HolidayName = "振替休日" },
                // 2026年
                new() { HolidayDate = new DateOnly(2026, 1, 1), HolidayName = "元日" },
                new() { HolidayDate = new DateOnly(2026, 1, 12), HolidayName = "成人の日" },
                new() { HolidayDate = new DateOnly(2026, 2, 11), HolidayName = "建国記念の日" },
                new() { HolidayDate = new DateOnly(2026, 2, 23), HolidayName = "天皇誕生日" },
                new() { HolidayDate = new DateOnly(2026, 3, 20), HolidayName = "春分の日" },
                new() { HolidayDate = new DateOnly(2026, 4, 29), HolidayName = "昭和の日" },
                new() { HolidayDate = new DateOnly(2026, 5, 3), HolidayName = "憲法記念日" },
                new() { HolidayDate = new DateOnly(2026, 5, 4), HolidayName = "みどりの日" },
                new() { HolidayDate = new DateOnly(2026, 5, 5), HolidayName = "こどもの日" },
                new() { HolidayDate = new DateOnly(2026, 5, 6), HolidayName = "振替休日" },
                new() { HolidayDate = new DateOnly(2026, 7, 20), HolidayName = "海の日" },
                new() { HolidayDate = new DateOnly(2026, 8, 11), HolidayName = "山の日" },
                new() { HolidayDate = new DateOnly(2026, 9, 21), HolidayName = "敬老の日" },
                new() { HolidayDate = new DateOnly(2026, 9, 22), HolidayName = "国民の休日" },
                new() { HolidayDate = new DateOnly(2026, 9, 23), HolidayName = "秋分の日" },
                new() { HolidayDate = new DateOnly(2026, 10, 12), HolidayName = "スポーツの日" },
                new() { HolidayDate = new DateOnly(2026, 11, 3), HolidayName = "文化の日" },
                new() { HolidayDate = new DateOnly(2026, 11, 23), HolidayName = "勤労感謝の日" },
            };
            foreach (var holiday in holidays)
            {
                holiday.IsDeleted = false;
            }
            context.Holidays.AddRange(holidays);
            Console.WriteLine($"  [+] Holidays: {holidays.Count} entries (2025-2026)");
        }
    }
}


