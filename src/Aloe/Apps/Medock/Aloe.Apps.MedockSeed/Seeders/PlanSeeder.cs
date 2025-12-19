using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class PlanSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingPlans = await context.Plans.AnyAsync();
        if (existingPlans)
        {
            Console.WriteLine("[SKIP] Plans already exist.");
            return;
        }

        Console.WriteLine("[INFO] Creating plan seed data...");

        var today = dateTimeProvider.TodayDateOnly;
        var plans = new List<Plan>
        {
            // 基本健診プラン
            new()
            {
                PlanId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                PlanCode = "BASIC",
                PlanName = "基本健診",
                PlanShortName = "基本健診",
                PlanAbbrName = "基本",
                PlanDesc = "基本的な健康診断プラン",
                PlanKindCode = 1,
                IsActive = true,
                ActiveFrom = today.AddYears(-1),
                ActiveTo = new DateOnly(9999, 12, 31),
                IsDeleted = false
            },
            // 人間ドックプラン
            new()
            {
                PlanId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                PlanCode = "DOCK",
                PlanName = "人間ドック",
                PlanShortName = "人間ドック",
                PlanAbbrName = "ドック",
                PlanDesc = "総合的な人間ドックプラン",
                PlanKindCode = 2,
                IsActive = true,
                ActiveFrom = today.AddYears(-1),
                ActiveTo = new DateOnly(9999, 12, 31),
                IsDeleted = false
            },
            // 特定健診プラン
            new()
            {
                PlanId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                PlanCode = "SPECIFIC",
                PlanName = "特定健診",
                PlanShortName = "特定健診",
                PlanAbbrName = "特定",
                PlanDesc = "特定健康診査プラン",
                PlanKindCode = 3,
                IsActive = true,
                ActiveFrom = today.AddYears(-1),
                ActiveTo = new DateOnly(9999, 12, 31),
                IsDeleted = false
            },
            // オプション検査プラン
            new()
            {
                PlanId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                PlanCode = "OPT_CT",
                PlanName = "CT検査オプション",
                PlanShortName = "CTオプション",
                PlanAbbrName = "CT",
                PlanDesc = "CT検査オプションプラン",
                PlanKindCode = 4,
                IsActive = true,
                ActiveFrom = today.AddYears(-1),
                ActiveTo = new DateOnly(9999, 12, 31),
                IsDeleted = false
            },
            new()
            {
                PlanId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                PlanCode = "OPT_MR",
                PlanName = "MR検査オプション",
                PlanShortName = "MRオプション",
                PlanAbbrName = "MR",
                PlanDesc = "MR検査オプションプラン",
                PlanKindCode = 4,
                IsActive = true,
                ActiveFrom = today.AddYears(-1),
                ActiveTo = new DateOnly(9999, 12, 31),
                IsDeleted = false
            },
            new()
            {
                PlanId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                PlanCode = "OPT_ENDOSCOPE",
                PlanName = "内視鏡検査オプション",
                PlanShortName = "内視鏡オプション",
                PlanAbbrName = "内視鏡",
                PlanDesc = "内視鏡検査オプションプラン",
                PlanKindCode = 4,
                IsActive = true,
                ActiveFrom = today.AddYears(-1),
                ActiveTo = new DateOnly(9999, 12, 31),
                IsDeleted = false
            },
            new()
            {
                PlanId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                PlanCode = "OPT_ECHO",
                PlanName = "エコー検査オプション",
                PlanShortName = "エコーオプション",
                PlanAbbrName = "エコー",
                PlanDesc = "エコー検査オプションプラン",
                PlanKindCode = 4,
                IsActive = true,
                ActiveFrom = today.AddYears(-1),
                ActiveTo = new DateOnly(9999, 12, 31),
                IsDeleted = false
            },
            // 女性向けプラン
            new()
            {
                PlanId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                PlanCode = "WOMAN",
                PlanName = "女性健診プラン",
                PlanShortName = "女性健診",
                PlanAbbrName = "女性",
                PlanDesc = "女性向け健診プラン",
                PlanKindCode = 5,
                IsActive = true,
                ActiveFrom = today.AddYears(-1),
                ActiveTo = new DateOnly(9999, 12, 31),
                IsDeleted = false
            },
            // 高齢者向けプラン
            new()
            {
                PlanId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                PlanCode = "SENIOR",
                PlanName = "高齢者健診プラン",
                PlanShortName = "高齢者健診",
                PlanAbbrName = "高齢者",
                PlanDesc = "高齢者向け健診プラン",
                PlanKindCode = 6,
                IsActive = true,
                ActiveFrom = today.AddYears(-1),
                ActiveTo = new DateOnly(9999, 12, 31),
                IsDeleted = false
            },
            // プレミアムプラン
            new()
            {
                PlanId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                PlanCode = "PREMIUM",
                PlanName = "プレミアム健診プラン",
                PlanShortName = "プレミアム",
                PlanAbbrName = "プレミアム",
                PlanDesc = "全検査を含むプレミアムプラン",
                PlanKindCode = 7,
                IsActive = true,
                ActiveFrom = today.AddYears(-1),
                ActiveTo = new DateOnly(9999, 12, 31),
                IsDeleted = false
            }
        };

        foreach (var plan in plans)
        {
            SeederHelper.InitializeAuditFields(plan, dateTimeProvider);
        }

        context.Plans.AddRange(plans);
        Console.WriteLine($"  [+] Plans: {plans.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}

