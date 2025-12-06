using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class FacilitySeeder
{
    public static async Task<(Guid? facilityId, Guid? floorId)> SeedAsync(MedockDbContext context, Guid tenantId)
    {
        var existingFacility = await context.Facilities.FirstOrDefaultAsync();
        Guid? facilityId = existingFacility?.FacilityId;
        Guid? floorId = null;

        if (existingFacility == null)
        {
            Console.WriteLine("[INFO] Creating facility and floor seed data...");

            facilityId = Guid.NewGuid();
            var facility = new Facility
            {
                FacilityId = facilityId.Value,
                TenantId = tenantId,
                MedicalInstitutionCode = "1234567890",
                FacilityName = "アロエ健診センター",
                FacilityNameDisplay = "アロエ健診センター",
                IsActive = true,
                ActiveFrom = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                CreatedUserId = Guid.Empty,
                CreatedSessionId = Guid.Empty,
                UpdatedUserId = Guid.Empty,
                UpdatedSessionId = Guid.Empty
            };
            context.Facilities.Add(facility);
            Console.WriteLine($"  [+] Facility: {facility.FacilityName} (TenantId: {tenantId})");

            floorId = Guid.NewGuid();
            var floor = new Floor
            {
                FloorId = floorId.Value,
                FacilityId = facilityId.Value,
                FloorCode = "1F",
                FloorName = "1階（健診フロア）",
                FloorDesc = "一般健診・人間ドック",
                FloorSeq = 1,
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                CreatedUserId = Guid.Empty,
                CreatedSessionId = Guid.Empty,
                UpdatedUserId = Guid.Empty,
                UpdatedSessionId = Guid.Empty
            };
            context.Floors.Add(floor);
            Console.WriteLine($"  [+] Floor: {floor.FloorName} (FacilityId: {facilityId})");
        }
        else
        {
            Console.WriteLine("[SKIP] Facility already exists.");
            facilityId = existingFacility.FacilityId;
            floorId = (await context.Floors.FirstOrDefaultAsync())?.FloorId;
        }

        return (facilityId, floorId);
    }
}

