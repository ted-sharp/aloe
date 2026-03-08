using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class PlanOptionSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingOptions = await context.PlanOptions.AnyAsync();
        if (existingOptions)
        {
            Console.WriteLine("[SKIP] PlanOptions already exist.");
            return;
        }

        var plans = await context.Plans
            .Where(p => !p.IsDeleted && p.FacilityId == facilityId)
            .ToListAsync();

        if (!plans.Any())
        {
            Console.WriteLine("[SKIP] PlanOption: No plans found.");
            return;
        }

        Console.WriteLine("[INFO] Creating plan option seed data...");

        var options = new List<PlanOption>();

        // 基本プランとオプションプランの関連付け
        var basicPlan = plans.FirstOrDefault(p => p.PlanCode == "BASIC");
        var dockPlan = plans.FirstOrDefault(p => p.PlanCode == "DOCK");
        var specificPlan = plans.FirstOrDefault(p => p.PlanCode == "SPECIFIC");
        var premiumPlan = plans.FirstOrDefault(p => p.PlanCode == "PREMIUM");

        var optCtPlan = plans.FirstOrDefault(p => p.PlanCode == "OPT_CT");
        var optMrPlan = plans.FirstOrDefault(p => p.PlanCode == "OPT_MR");
        var optEndoscopePlan = plans.FirstOrDefault(p => p.PlanCode == "OPT_ENDOSCOPE");
        var optEchoPlan = plans.FirstOrDefault(p => p.PlanCode == "OPT_ECHO");

        // 基本健診にオプションを追加
        if (basicPlan != null)
        {
            if (optCtPlan != null)
            {
                options.Add(CreatePlanOption(basicPlan.PlanId, optCtPlan.PlanId, dateTimeProvider));
            }
            if (optMrPlan != null)
            {
                options.Add(CreatePlanOption(basicPlan.PlanId, optMrPlan.PlanId, dateTimeProvider));
            }
            if (optEndoscopePlan != null)
            {
                options.Add(CreatePlanOption(basicPlan.PlanId, optEndoscopePlan.PlanId, dateTimeProvider));
            }
            if (optEchoPlan != null)
            {
                options.Add(CreatePlanOption(basicPlan.PlanId, optEchoPlan.PlanId, dateTimeProvider));
            }
        }

        // 人間ドックにオプションを追加
        if (dockPlan != null)
        {
            if (optCtPlan != null)
            {
                options.Add(CreatePlanOption(dockPlan.PlanId, optCtPlan.PlanId, dateTimeProvider));
            }
            if (optMrPlan != null)
            {
                options.Add(CreatePlanOption(dockPlan.PlanId, optMrPlan.PlanId, dateTimeProvider));
            }
            if (optEndoscopePlan != null)
            {
                options.Add(CreatePlanOption(dockPlan.PlanId, optEndoscopePlan.PlanId, dateTimeProvider));
            }
        }

        // 特定健診にオプションを追加
        if (specificPlan != null)
        {
            if (optEchoPlan != null)
            {
                options.Add(CreatePlanOption(specificPlan.PlanId, optEchoPlan.PlanId, dateTimeProvider));
            }
        }

        // プレミアムプランにはすべてのオプションを含める
        if (premiumPlan != null)
        {
            if (optCtPlan != null)
            {
                options.Add(CreatePlanOption(premiumPlan.PlanId, optCtPlan.PlanId, dateTimeProvider));
            }
            if (optMrPlan != null)
            {
                options.Add(CreatePlanOption(premiumPlan.PlanId, optMrPlan.PlanId, dateTimeProvider));
            }
            if (optEndoscopePlan != null)
            {
                options.Add(CreatePlanOption(premiumPlan.PlanId, optEndoscopePlan.PlanId, dateTimeProvider));
            }
            if (optEchoPlan != null)
            {
                options.Add(CreatePlanOption(premiumPlan.PlanId, optEchoPlan.PlanId, dateTimeProvider));
            }
        }

        context.PlanOptions.AddRange(options);
        Console.WriteLine($"  [+] PlanOptions: {options.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }

    private static PlanOption CreatePlanOption(Guid planId, Guid optionPlanId, IDateTimeProvider dateTimeProvider)
    {
        var option = new PlanOption
        {
            PlanOptionId = Guid.CreateVersion7(),
            PlanId = planId,
            OptionPlanId = optionPlanId,
            IsDeleted = false
        };

        SeederHelper.InitializeAuditFields(option, dateTimeProvider);
        return option;
    }
}

