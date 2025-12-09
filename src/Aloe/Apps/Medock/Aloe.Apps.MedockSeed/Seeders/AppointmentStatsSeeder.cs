using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentStatsSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid? floorId)
    {
        var existingStats = await context.AppointmentStats.AnyAsync();
        if (!existingStats && floorId.HasValue)
        {
            Console.WriteLine("[INFO] Creating appointment stats seed data (1 year)...");
            var random = new Random(42);
            var startDate = new DateOnly(DateTime.Today.Year, 1, 1);
            var endDate = new DateOnly(DateTime.Today.Year, 12, 31);
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
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
            }

            context.AppointmentStats.AddRange(statsList);
            Console.WriteLine($"  [+] AppointmentStats: {statsList.Count} days with slot data");
        }
        else if (existingStats)
        {
            Console.WriteLine("[SKIP] AppointmentStats already exist.");
        }
    }
}


