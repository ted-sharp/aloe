using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class PlanResourceRequirementSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingRequirements = await context.PlanResourceRequirements.AnyAsync();
        if (existingRequirements)
        {
            Console.WriteLine("[SKIP] PlanResourceRequirements already exist.");
            return;
        }

        var plans = await context.Plans
            .Where(p => !p.IsDeleted && p.FacilityId == facilityId)
            .ToListAsync();

        if (!plans.Any())
        {
            Console.WriteLine("[SKIP] PlanResourceRequirement: No plans found.");
            return;
        }

        var resources = await context.AppointmentResources
            .Where(r => !r.IsDeleted)
            .ToListAsync();

        if (!resources.Any())
        {
            Console.WriteLine("[SKIP] PlanResourceRequirement: No appointment resources found.");
            return;
        }

        Console.WriteLine("[INFO] Creating plan resource requirement seed data...");

        var requirements = new List<PlanResourceRequirement>();

        // リソース名でリソースを取得
        var endoscope = resources.FirstOrDefault(r => r.ApptResName == "内視鏡");
        var echo = resources.FirstOrDefault(r => r.ApptResName == "エコー");
        var ct = resources.FirstOrDefault(r => r.ApptResName == "CT");
        var mr = resources.FirstOrDefault(r => r.ApptResName == "MR");
        var mainResource = resources.FirstOrDefault(r => r.ApptResName == "メイン");

        foreach (var plan in plans)
        {
            // プランコードに応じてリソース要件を設定
            switch (plan.PlanCode)
            {
                case "BASIC":
                    // 基本健診：メインのみ
                    if (mainResource != null)
                    {
                        requirements.Add(CreateRequirement(plan.PlanId, mainResource.ApptResId, dateTimeProvider));
                    }
                    break;

                case "DOCK":
                    // 人間ドック：メイン、エコー
                    if (mainResource != null)
                    {
                        requirements.Add(CreateRequirement(plan.PlanId, mainResource.ApptResId, dateTimeProvider));
                    }
                    if (echo != null)
                    {
                        requirements.Add(CreateRequirement(plan.PlanId, echo.ApptResId, dateTimeProvider));
                    }
                    break;

                case "OPT_CT":
                    // CTオプション：CT、メイン
                    if (ct != null)
                    {
                        requirements.Add(CreateRequirement(plan.PlanId, ct.ApptResId, dateTimeProvider));
                    }
                    if (mainResource != null)
                    {
                        requirements.Add(CreateRequirement(plan.PlanId, mainResource.ApptResId, dateTimeProvider));
                    }
                    break;

                case "OPT_MR":
                    // MRオプション：MR、メイン
                    if (mr != null)
                    {
                        requirements.Add(CreateRequirement(plan.PlanId, mr.ApptResId, dateTimeProvider));
                    }
                    if (mainResource != null)
                    {
                        requirements.Add(CreateRequirement(plan.PlanId, mainResource.ApptResId, dateTimeProvider));
                    }
                    break;

                case "OPT_ENDOSCOPE":
                    // 内視鏡オプション：内視鏡、メイン
                    if (endoscope != null)
                    {
                        requirements.Add(CreateRequirement(plan.PlanId, endoscope.ApptResId, dateTimeProvider));
                    }
                    if (mainResource != null)
                    {
                        requirements.Add(CreateRequirement(plan.PlanId, mainResource.ApptResId, dateTimeProvider));
                    }
                    break;

                case "OPT_ECHO":
                    // エコーオプション：エコー、メイン
                    if (echo != null)
                    {
                        requirements.Add(CreateRequirement(plan.PlanId, echo.ApptResId, dateTimeProvider));
                    }
                    if (mainResource != null)
                    {
                        requirements.Add(CreateRequirement(plan.PlanId, mainResource.ApptResId, dateTimeProvider));
                    }
                    break;

                case "PREMIUM":
                    // プレミアム：すべてのリソース
                    foreach (var resource in resources)
                    {
                        requirements.Add(CreateRequirement(plan.PlanId, resource.ApptResId, dateTimeProvider));
                    }
                    break;

                default:
                    // その他のプラン：メインのみ
                    if (mainResource != null)
                    {
                        requirements.Add(CreateRequirement(plan.PlanId, mainResource.ApptResId, dateTimeProvider));
                    }
                    break;
            }
        }

        context.PlanResourceRequirements.AddRange(requirements);
        Console.WriteLine($"  [+] PlanResourceRequirements: {requirements.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }

    private static PlanResourceRequirement CreateRequirement(Guid planId, Guid resourceId, IDateTimeProvider dateTimeProvider)
    {
        var requirement = new PlanResourceRequirement
        {
            PlanResReqId = Guid.CreateVersion7(),
            PlanId = planId,
            ApptResId = resourceId,
            IsDeleted = false
        };

        SeederHelper.InitializeAuditFields(requirement, dateTimeProvider);
        return requirement;
    }
}

