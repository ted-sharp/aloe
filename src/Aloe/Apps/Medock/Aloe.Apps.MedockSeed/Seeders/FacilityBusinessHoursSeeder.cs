using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class FacilityBusinessHoursSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingBusinessHours = await context.FacilityBusinessHours
            .AnyAsync(fbh => fbh.FacilityId == facilityId && !fbh.IsDeleted);

        if (existingBusinessHours)
        {
            Console.WriteLine("[SKIP] FacilityBusinessHours already exist.");
            return;
        }

        Console.WriteLine("[INFO] Creating facility business hours seed data...");

        var facilityBusinessHours = new FacilityBusinessHours
        {
            FacilityBusinessHoursId = Guid.CreateVersion7(),
            FacilityId = facilityId,
            WorkStartMin = 540,   // 09:00
            WorkEndMin = 1080,    // 18:00
            LunchStartMin = 720,  // 12:00
            LunchEndMin = 780,    // 13:00
            IsActive = true,
            ActiveFrom = DateOnly.FromDateTime(dateTimeProvider.Today),
            ActiveTo = new DateOnly(9999, 12, 31),
            IsDeleted = false,
            CreatedAt = dateTimeProvider.Now,
            UpdatedAt = dateTimeProvider.Now,
            CreatedUserId = Guid.Empty,
            CreatedSessionId = Guid.Empty,
            UpdatedUserId = Guid.Empty,
            UpdatedSessionId = Guid.Empty
        };

        context.FacilityBusinessHours.Add(facilityBusinessHours);
        Console.WriteLine($"  [+] FacilityBusinessHours: Start 09:00, End 18:00, Lunch 12:00-13:00");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}
