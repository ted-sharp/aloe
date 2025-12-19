using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentResourceSeeder
{
    public static async Task<Dictionary<string, Guid>> SeedAsync(MedockDbContext context, Guid floorId, IDateTimeProvider dateTimeProvider)
    {
        var resourceIds = new Dictionary<string, Guid>();

        var existingResources = await context.AppointmentResources.AnyAsync();
        if (existingResources)
        {
            Console.WriteLine("[SKIP] AppointmentResources already exist.");
            // 既存のリソースを読み込んで返す
            var existingResourcesList = await context.AppointmentResources
                .Where(r => !r.IsDeleted)
                .ToListAsync();
            foreach (var resource in existingResourcesList)
            {
                var key = resource.ApptResName;
                if (!resourceIds.ContainsKey(key))
                {
                    resourceIds[key] = resource.ApptResId;
                }
            }
            return resourceIds;
        }

        Console.WriteLine("[INFO] Creating appointment resource seed data...");

        var resources = new List<AppointmentResource>
        {
            // メインリソースを最初に追加
            new()
            {
                ApptResId = Guid.CreateVersion7(),
                FloorId = floorId,
                AppointmentResourceType = Aloe.Apps.MedockLib.Constants.AppointmentResourceType.Main, // ロッカー → Main
                ApptResName = "ロッカー",
                ApptResDesc = "ロッカー（AM/PM各80個制限、30分区切りで業務時間を分割）",
                ApptResSeq = 1,
                IsDeleted = false
            },
            // その他のリソース
            new()
            {
                ApptResId = Guid.CreateVersion7(),
                FloorId = floorId,
                AppointmentResourceType = Aloe.Apps.MedockLib.Constants.AppointmentResourceType.Equipment, // 内視鏡 → Equipment
                ApptResName = "内視鏡",
                ApptResDesc = "内視鏡検査用リソース",
                ApptResSeq = 2,
                IsDeleted = false
            },
            new()
            {
                ApptResId = Guid.CreateVersion7(),
                FloorId = floorId,
                AppointmentResourceType = Aloe.Apps.MedockLib.Constants.AppointmentResourceType.Equipment, // エコー → Equipment
                ApptResName = "腹部エコー",
                ApptResDesc = "腹部エコー検査用リソース",
                ApptResSeq = 3,
                IsDeleted = false
            },
            new()
            {
                ApptResId = Guid.CreateVersion7(),
                FloorId = floorId,
                AppointmentResourceType = Aloe.Apps.MedockLib.Constants.AppointmentResourceType.Equipment, // エコー → Equipment
                ApptResName = "乳腺エコー",
                ApptResDesc = "乳腺エコー検査用リソース",
                ApptResSeq = 4,
                IsDeleted = false
            },
            new()
            {
                ApptResId = Guid.CreateVersion7(),
                FloorId = floorId,
                AppointmentResourceType = Aloe.Apps.MedockLib.Constants.AppointmentResourceType.Equipment, // エコー → Equipment
                ApptResName = "頸動脈エコー",
                ApptResDesc = "頸動脈エコー検査用リソース",
                ApptResSeq = 5,
                IsDeleted = false
            },
            new()
            {
                ApptResId = Guid.CreateVersion7(),
                FloorId = floorId,
                AppointmentResourceType = Aloe.Apps.MedockLib.Constants.AppointmentResourceType.Equipment, // CT → Equipment
                ApptResName = "CT",
                ApptResDesc = "CT検査用リソース",
                ApptResSeq = 6,
                IsDeleted = false
            },
            new()
            {
                ApptResId = Guid.CreateVersion7(),
                FloorId = floorId,
                AppointmentResourceType = Aloe.Apps.MedockLib.Constants.AppointmentResourceType.Equipment, // MR → Equipment
                ApptResName = "MR",
                ApptResDesc = "MR検査用リソース",
                ApptResSeq = 7,
                IsDeleted = false
            },
            new()
            {
                ApptResId = Guid.CreateVersion7(),
                FloorId = floorId,
                AppointmentResourceType = Aloe.Apps.MedockLib.Constants.AppointmentResourceType.Equipment, // 胃Ba → Equipment
                ApptResName = "胃Ba",
                ApptResDesc = "胃部X線検査用リソース",
                ApptResSeq = 8,
                IsDeleted = false
            },
            new()
            {
                ApptResId = Guid.CreateVersion7(),
                FloorId = floorId,
                AppointmentResourceType = Aloe.Apps.MedockLib.Constants.AppointmentResourceType.Equipment, // 内視鏡(外部) → Equipment
                ApptResName = "内視鏡(外部)",
                ApptResDesc = "内視鏡検査用リソース（外部委託）",
                ApptResSeq = 9,
                IsDeleted = false
            }
        };

        foreach (var resource in resources)
        {
            SeederHelper.InitializeAuditFields(resource, dateTimeProvider);
            resourceIds[resource.ApptResName] = resource.ApptResId;
        }

        context.AppointmentResources.AddRange(resources);
        Console.WriteLine($"  [+] AppointmentResources: {resources.Count} entries");
        foreach (var resource in resources)
        {
            Console.WriteLine($"    - {resource.ApptResName} (Type: {resource.ApptResTypeCode})");
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }

        return resourceIds;
    }
}

