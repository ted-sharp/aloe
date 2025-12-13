using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentResourceSeeder
{
    public static async Task<Dictionary<string, Guid>> SeedAsync(MedockDbContext context, Guid? floorId, IDateTimeProvider dateTimeProvider)
    {
        var resourceIds = new Dictionary<string, Guid>();
        
        if (!floorId.HasValue)
        {
            Console.WriteLine("[SKIP] AppointmentResource: No floor ID provided.");
            return resourceIds;
        }

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
            new()
            {
                ApptResId = Guid.NewGuid(),
                FloorId = floorId.Value,
                ApptResTypeCode = 1, // 内視鏡
                ApptResName = "内視鏡",
                ApptResDesc = "内視鏡検査用リソース",
                ApptResSeq = 1,
                IsDeleted = false
            },
            new()
            {
                ApptResId = Guid.NewGuid(),
                FloorId = floorId.Value,
                ApptResTypeCode = 2, // エコー
                ApptResName = "エコー",
                ApptResDesc = "エコー検査用リソース（AM/PM制限）",
                ApptResSeq = 2,
                IsDeleted = false
            },
            new()
            {
                ApptResId = Guid.NewGuid(),
                FloorId = floorId.Value,
                ApptResTypeCode = 3, // CT
                ApptResName = "CT",
                ApptResDesc = "CT検査用リソース",
                ApptResSeq = 3,
                IsDeleted = false
            },
            new()
            {
                ApptResId = Guid.NewGuid(),
                FloorId = floorId.Value,
                ApptResTypeCode = 4, // MR
                ApptResName = "MR",
                ApptResDesc = "MR検査用リソース",
                ApptResSeq = 4,
                IsDeleted = false
            },
            new()
            {
                ApptResId = Guid.NewGuid(),
                FloorId = floorId.Value,
                ApptResTypeCode = 5, // ロッカー
                ApptResName = "ロッカー",
                ApptResDesc = "ロッカー（AM/PM各80個制限）",
                ApptResSeq = 5,
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

        return resourceIds;
    }
}

