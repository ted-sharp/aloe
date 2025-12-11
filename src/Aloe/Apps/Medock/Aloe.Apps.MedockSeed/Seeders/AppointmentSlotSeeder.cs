using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentSlotSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid? floorId, IDateTimeProvider dateTimeProvider)
    {
        var existingApptSlots = await context.AppointmentSlots.AnyAsync();
        if (!existingApptSlots && floorId.HasValue)
        {
            Console.WriteLine("[INFO] Creating appointment slot seed data...");
            var slotsJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                slots = new[]
                {
                    // 早朝
                    new { time = "07:00", max = 2, duration = 60 },
                    new { time = "07:30", max = 2, duration = 60 },
                    new { time = "08:00", max = 5, duration = 60 },
                    new { time = "08:30", max = 5, duration = 60 },
                    // 午前中（始業後）
                    new { time = "09:00", max = 10, duration = 60 },
                    new { time = "09:15", max = 10, duration = 60 },
                    new { time = "09:30", max = 10, duration = 60 },
                    new { time = "09:45", max = 10, duration = 60 },
                    new { time = "10:00", max = 12, duration = 60 },
                    new { time = "10:15", max = 12, duration = 60 },
                    new { time = "10:30", max = 12, duration = 60 },
                    new { time = "10:45", max = 12, duration = 60 },
                    new { time = "11:00", max = 12, duration = 60 },
                    new { time = "11:15", max = 12, duration = 60 },
                    new { time = "11:30", max = 10, duration = 60 },
                    new { time = "11:45", max = 8, duration = 60 },
                    // 昼休み（イレギュラー）
                    new { time = "12:00", max = 3, duration = 30 },
                    new { time = "12:15", max = 3, duration = 30 },
                    new { time = "12:30", max = 3, duration = 30 },
                    new { time = "12:45", max = 3, duration = 30 },
                    // 午後（昼休み後）
                    new { time = "13:00", max = 12, duration = 60 },
                    new { time = "13:15", max = 12, duration = 60 },
                    new { time = "13:30", max = 12, duration = 60 },
                    new { time = "13:45", max = 12, duration = 60 },
                    new { time = "14:00", max = 12, duration = 60 },
                    new { time = "14:15", max = 12, duration = 60 },
                    new { time = "14:30", max = 12, duration = 60 },
                    new { time = "14:45", max = 12, duration = 60 },
                    new { time = "15:00", max = 12, duration = 60 },
                    new { time = "15:15", max = 12, duration = 60 },
                    new { time = "15:30", max = 10, duration = 60 },
                    new { time = "15:45", max = 10, duration = 60 },
                    new { time = "16:00", max = 8, duration = 60 },
                    new { time = "16:15", max = 8, duration = 60 },
                    new { time = "16:30", max = 5, duration = 60 },
                    new { time = "16:45", max = 5, duration = 60 },
                    new { time = "17:00", max = 3, duration = 60 },
                    // 夜間（イレギュラー）
                    new { time = "18:00", max = 2, duration = 60 },
                    new { time = "18:30", max = 2, duration = 60 },
                    new { time = "19:00", max = 1, duration = 60 },
                    new { time = "19:30", max = 1, duration = 60 },
                }
            });
            var apptSlot = new AppointmentSlot
            {
                ApptSlotId = Guid.NewGuid(),
                FloorId = floorId.Value,
                ApptSlots = slotsJson,
                IsActive = true,
                ActiveFrom = DateOnly.FromDateTime(dateTimeProvider.Today.AddYears(-1)),
                ActiveTo = new DateOnly(9999, 12, 31),
                IsDeleted = false,
                CreatedAt = dateTimeProvider.Now,
                UpdatedAt = dateTimeProvider.Now
            };
            context.AppointmentSlots.Add(apptSlot);
            Console.WriteLine($"  [+] AppointmentSlot: 8 time slots defined");
        }
        else if (existingApptSlots)
        {
            Console.WriteLine("[SKIP] AppointmentSlots already exist.");
        }
    }
}


