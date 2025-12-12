using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentStatsSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid? floorId, IDateTimeProvider dateTimeProvider)
    {
        if (!floorId.HasValue)
        {
            Console.WriteLine("[SKIP] FloorId is not set. Skipping appointment stats seed data.");
            return;
        }

        var today = dateTimeProvider.TodayDateOnly;
        var startDate = today.AddYears(-3);
        var endDate = today.AddYears(1);

        // 過去3年〜未来1年の範囲に既存データが存在するかチェック
        var existingStatsInRange = await context.AppointmentStats
            .Where(s => !s.IsDeleted && s.ApptDate >= startDate && s.ApptDate <= endDate)
            .AnyAsync();

        if (existingStatsInRange)
        {
            Console.WriteLine("[SKIP] AppointmentStats data already exists in the range (past 3 years to future 1 year).");
            return;
        }

        Console.WriteLine("[INFO] Creating appointment stats seed data (past 3 years to future 1 year)...");
        var random = new Random(42);
        var statsList = new List<AppointmentStats>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dayOfWeek = date.DayOfWeek;
                var isWeekend = dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;

                var slots = new List<object>();
                var totalCount = 0;
                var totalMax = 0;

                var slotTimes = new[] { "08:00", "09:00", "10:00", "11:00", "13:00", "14:00", "15:00", "16:00" };
                var slotMaxes = new[] { 5, 8, 8, 8, 8, 8, 8, 5 };

                for (var i = 0; i < slotTimes.Length; i++)
                {
                    var max = isWeekend ? slotMaxes[i] / 2 : slotMaxes[i];
                    var count = random.Next(0, max + 1);
                    slots.Add(new { time = slotTimes[i], count, max });
                    totalCount += count;
                    totalMax += max;
                }

                var graphJson = System.Text.Json.JsonSerializer.Serialize(new { slots });

                statsList.Add(new AppointmentStats
                {
                    ApptStatId = Guid.NewGuid(),
                    FloorId = floorId.Value,
                    ApptDate = date,
                    ApptCount = totalCount,
                    ApptMax = totalMax,
                    ApptGraph = graphJson,
                    IsDeleted = false,
                    CreatedAt = dateTimeProvider.Now,
                    UpdatedAt = dateTimeProvider.Now
                });
            }

        context.AppointmentStats.AddRange(statsList);
        Console.WriteLine($"  [+] AppointmentStats: {statsList.Count} days with slot data");
    }
}


