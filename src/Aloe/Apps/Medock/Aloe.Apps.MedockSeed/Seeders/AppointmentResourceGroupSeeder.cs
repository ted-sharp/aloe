using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentResourceGroupSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingGroups = await context.AppointmentResourceGroups.AnyAsync();
        if (existingGroups)
        {
            Console.WriteLine("[SKIP] AppointmentResourceGroups already exist.");
            return;
        }

        Console.WriteLine("[INFO] Creating appointment resource group seed data...");

        var groups = new List<AppointmentResourceGroup>
        {
            new()
            {
                ApptResGroupId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                ResGroupCode = "EXAM_ROOM",
                ResGroupName = "検査室グループ",
                ResGroupDesc = "内視鏡、CT、MRなどの検査室をグループ化",
                ResGroupSeq = 1,
                IsDeleted = false
            },
            new()
            {
                ApptResGroupId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                ResGroupCode = "ECHO_ROOM",
                ResGroupName = "エコー室グループ",
                ResGroupDesc = "エコー検査室をグループ化",
                ResGroupSeq = 2,
                IsDeleted = false
            },
            new()
            {
                ApptResGroupId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                ResGroupCode = "LOCKER_CLUSTER",
                ResGroupName = "ロッカークラスター",
                ResGroupDesc = "ロッカーをクラスター化",
                ResGroupSeq = 3,
                IsDeleted = false
            }
        };

        foreach (var group in groups)
        {
            SeederHelper.InitializeAuditFields(group, dateTimeProvider);
        }

        context.AppointmentResourceGroups.AddRange(groups);
        Console.WriteLine($"  [+] AppointmentResourceGroups: {groups.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}

