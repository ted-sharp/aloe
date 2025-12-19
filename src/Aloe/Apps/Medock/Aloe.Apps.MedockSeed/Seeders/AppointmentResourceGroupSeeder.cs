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
                ResGroupCode = "ECHO_ROOM",
                ResGroupName = "エコー",
                ResGroupDesc = "エコー検査室をグループ化",
                ResGroupSeq = 1,
                IsDeleted = false
            },
            new()
            {
                ApptResGroupId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                ResGroupCode = "STOMACH_EXAM_BOTH",
                ResGroupName = "胃部検査(内視鏡または胃Ba)",
                ResGroupDesc = "胃部検査（内視鏡または胃Ba）用リソースグループ",
                ResGroupSeq = 2,
                IsDeleted = false
            },
            new()
            {
                ApptResGroupId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                ResGroupCode = "STOMACH_EXAM_BOTH_X",
                ResGroupName = "胃部検査(内視鏡または胃Ba)-外部委託含む",
                ResGroupDesc = "胃部検査（内視鏡または胃Ba）-外部委託含む用リソースグループ",
                ResGroupSeq = 3,
                IsDeleted = false
            },
            new()
            {
                ApptResGroupId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                ResGroupCode = "ENDOSCOPE_ONLY",
                ResGroupName = "内視鏡",
                ResGroupDesc = "内視鏡検査用リソースグループ",
                ResGroupSeq = 4,
                IsDeleted = false
            },
            new()
            {
                ApptResGroupId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                ResGroupCode = "ENDOSCOPE_ONLY_EXT",
                ResGroupName = "内視鏡-外部委託含む",
                ResGroupDesc = "内視鏡検査-外部委託含む用リソースグループ",
                ResGroupSeq = 5,
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

