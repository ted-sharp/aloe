using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class PlanConditionSeeder
{
    public static async Task SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
    {
        var existingConditions = await context.PlanConditions.AnyAsync();
        if (existingConditions)
        {
            Console.WriteLine("[SKIP] PlanConditions already exist.");
            return;
        }

        Console.WriteLine("[INFO] Creating plan condition seed data...");

        var conditions = new List<PlanCondition>
        {
            new()
            {
                PlanCondId = Guid.CreateVersion7(),
                ConditionName = "年齢20代",
                IsDeleted = false
            },
            new()
            {
                PlanCondId = Guid.CreateVersion7(),
                ConditionName = "年齢30代",
                IsDeleted = false
            },
            new()
            {
                PlanCondId = Guid.CreateVersion7(),
                ConditionName = "年齢40代",
                IsDeleted = false
            },
            new()
            {
                PlanCondId = Guid.CreateVersion7(),
                ConditionName = "年齢50代",
                IsDeleted = false
            },
            new()
            {
                PlanCondId = Guid.CreateVersion7(),
                ConditionName = "年齢60代以上",
                IsDeleted = false
            },
            new()
            {
                PlanCondId = Guid.CreateVersion7(),
                ConditionName = "男性",
                IsDeleted = false
            },
            new()
            {
                PlanCondId = Guid.CreateVersion7(),
                ConditionName = "女性",
                IsDeleted = false
            },
            new()
            {
                PlanCondId = Guid.CreateVersion7(),
                ConditionName = "初回健診",
                IsDeleted = false
            },
            new()
            {
                PlanCondId = Guid.CreateVersion7(),
                ConditionName = "定期健診",
                IsDeleted = false
            }
        };

        foreach (var condition in conditions)
        {
            SeederHelper.InitializeAuditFields(condition, dateTimeProvider);
        }

        context.PlanConditions.AddRange(conditions);
        Console.WriteLine($"  [+] PlanConditions: {conditions.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}

