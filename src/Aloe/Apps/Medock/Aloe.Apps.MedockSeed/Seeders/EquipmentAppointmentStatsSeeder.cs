using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class EquipmentAppointmentStatsSeeder
{
    public static async Task SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
    {
        var existingEquipmentStats = await context.EquipmentAppointmentStats.AnyAsync();
        if (!existingEquipmentStats)
        {
            Console.WriteLine("[INFO] Creating equipment appointment stats seed data...");
            var equipments = await context.Equipments.Where(e => !e.IsDeleted).ToListAsync();
            if (equipments.Any())
            {
                var random = new Random(42);
                var startDate = DateOnly.FromDateTime(dateTimeProvider.Today.AddMonths(-1));
                var endDate = DateOnly.FromDateTime(dateTimeProvider.Today.AddMonths(2));
                var equipmentStats = new List<EquipmentAppointmentStats>();

                foreach (var equipment in equipments)
                {
                    for (var date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                            continue;

                        var slots = new List<object>();
                        var totalCount = 0;
                        var totalMax = 0;

                        var slotTimes = new[] { "08:00", "09:00", "10:00", "11:00", "13:00", "14:00", "15:00", "16:00" };
                        var slotMaxes = new[] { 2, 3, 3, 3, 3, 3, 3, 2 };

                        for (var i = 0; i < slotTimes.Length; i++)
                        {
                            var max = slotMaxes[i];
                            var count = random.Next(0, max + 1);
                            slots.Add(new { time = slotTimes[i], count, max });
                            totalCount += count;
                            totalMax += max;
                        }

                        var graphJson = System.Text.Json.JsonSerializer.Serialize(new { slots });

                        equipmentStats.Add(new EquipmentAppointmentStats
                        {
                            ApptStatId = Guid.NewGuid(),
                            EquipId = equipment.EquipId,
                            ApptDate = date,
                            ApptCount = totalCount,
                            ApptMax = totalMax,
                            ApptGraph = graphJson,
                            IsDeleted = false,
                            CreatedAt = dateTimeProvider.Now,
                            UpdatedAt = dateTimeProvider.Now,
                            CreatedUserId = Guid.Empty,
                            CreatedSessionId = Guid.Empty,
                            UpdatedUserId = Guid.Empty,
                            UpdatedSessionId = Guid.Empty
                        });
                    }
                }

                context.EquipmentAppointmentStats.AddRange(equipmentStats);
                Console.WriteLine($"  [+] Equipment Appointment Stats: {equipmentStats.Count} entries ({equipments.Count} equipments × ~90 days)");
            }
            else
            {
                Console.WriteLine("[WARN] No equipments found. Skipping equipment appointment stats seed.");
            }
        }
        else
        {
            Console.WriteLine("[SKIP] Equipment appointment stats already exist.");
        }
    }
}


