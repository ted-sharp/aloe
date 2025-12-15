using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class FacilitySeeder
{
    public static async Task<(Guid? facilityId, Guid? floorId)> SeedAsync(MedockDbContext context, Guid tenantId, IDateTimeProvider dateTimeProvider)
    {
        var existingFacility = await context.Facilities.FirstOrDefaultAsync();
        if (existingFacility != null)
        {
            Console.WriteLine("[SKIP] Facility already exists.");
            var firstFacilityId = existingFacility.FacilityId;
            var firstFloorId = (await context.Floors.FirstOrDefaultAsync())?.FloorId;
            return (firstFacilityId, firstFloorId);
        }

        Console.WriteLine("[INFO] Creating facility and floor seed data...");

        var facilityId = Guid.CreateVersion7();
        var facility = new Facility
        {
            FacilityId = facilityId,
            TenantId = tenantId,
            MedicalInstitutionCode = "1234567890",
            FacilityName = "DEMO-健診センター",
            FacilityNameDisplay = "DEMO-健診センター",
            IsActive = true,
            ActiveFrom = dateTimeProvider.TodayDateOnly.AddYears(-1),
            IsDeleted = false
        };
        SeederHelper.InitializeAuditFields(facility, dateTimeProvider);
        context.Facilities.Add(facility);
        Console.WriteLine($"  [+] Facility: {facility.FacilityName} (TenantId: {tenantId})");

        var floorId = Guid.CreateVersion7();
        var floor = new Floor
        {
            FloorId = floorId,
            FacilityId = facilityId,
            FloorCode = "1F",
            FloorName = "1階（DEMO-健診フロア）",
            FloorDesc = "一般健診・人間ドック",
            FloorSeq = 1,
            IsDeleted = false
        };
        SeederHelper.InitializeAuditFields(floor, dateTimeProvider);
        context.Floors.Add(floor);
        Console.WriteLine($"  [+] Floor: {floor.FloorName} (FacilityId: {facilityId})");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }

        return (facilityId, floorId);
    }
}


