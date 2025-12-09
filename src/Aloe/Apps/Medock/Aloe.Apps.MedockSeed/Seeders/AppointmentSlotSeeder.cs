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
                    new { time = "08:00", max = 5, duration = 60 },
                    new { time = "09:00", max = 8, duration = 60 },
                    new { time = "10:00", max = 8, duration = 60 },
                    new { time = "11:00", max = 8, duration = 60 },
                    new { time = "13:00", max = 8, duration = 60 },
                    new { time = "14:00", max = 8, duration = 60 },
                    new { time = "15:00", max = 8, duration = 60 },
                    new { time = "16:00", max = 5, duration = 60 },
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


