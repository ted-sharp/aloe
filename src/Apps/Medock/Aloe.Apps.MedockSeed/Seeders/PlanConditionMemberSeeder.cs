using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class PlanConditionMemberSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingMembers = await context.PlanConditionMembers.AnyAsync();
        if (existingMembers)
        {
            Console.WriteLine("[SKIP] PlanConditionMembers already exist.");
            return;
        }

        var plans = await context.Plans
            .Where(p => !p.IsDeleted && p.FacilityId == facilityId)
            .ToListAsync();

        if (!plans.Any())
        {
            Console.WriteLine("[SKIP] PlanConditionMember: No plans found.");
            return;
        }

        var conditions = await context.PlanConditions
            .Where(c => !c.IsDeleted)
            .ToListAsync();

        if (!conditions.Any())
        {
            Console.WriteLine("[SKIP] PlanConditionMember: No plan conditions found.");
            return;
        }

        Console.WriteLine("[INFO] Creating plan condition member seed data...");

        var members = new List<PlanConditionMember>();

        // 条件名で条件を取得
        var age20s = conditions.FirstOrDefault(c => c.ConditionName == "年齢20代");
        var age30s = conditions.FirstOrDefault(c => c.ConditionName == "年齢30代");
        var age40s = conditions.FirstOrDefault(c => c.ConditionName == "年齢40代");
        var age50s = conditions.FirstOrDefault(c => c.ConditionName == "年齢50代");
        var age60s = conditions.FirstOrDefault(c => c.ConditionName == "年齢60代以上");
        var male = conditions.FirstOrDefault(c => c.ConditionName == "男性");
        var female = conditions.FirstOrDefault(c => c.ConditionName == "女性");
        var firstTime = conditions.FirstOrDefault(c => c.ConditionName == "初回健診");
        var regular = conditions.FirstOrDefault(c => c.ConditionName == "定期健診");

        foreach (var plan in plans)
        {
            // プランコードに応じて条件を関連付け
            switch (plan.PlanCode)
            {
                case "BASIC":
                    // 基本健診：すべての年齢、性別、初回・定期
                    if (age20s != null) members.Add(CreateMember(plan.PlanId, age20s.PlanCondId, dateTimeProvider));
                    if (age30s != null) members.Add(CreateMember(plan.PlanId, age30s.PlanCondId, dateTimeProvider));
                    if (age40s != null) members.Add(CreateMember(plan.PlanId, age40s.PlanCondId, dateTimeProvider));
                    if (age50s != null) members.Add(CreateMember(plan.PlanId, age50s.PlanCondId, dateTimeProvider));
                    if (age60s != null) members.Add(CreateMember(plan.PlanId, age60s.PlanCondId, dateTimeProvider));
                    if (male != null) members.Add(CreateMember(plan.PlanId, male.PlanCondId, dateTimeProvider));
                    if (female != null) members.Add(CreateMember(plan.PlanId, female.PlanCondId, dateTimeProvider));
                    if (firstTime != null) members.Add(CreateMember(plan.PlanId, firstTime.PlanCondId, dateTimeProvider));
                    if (regular != null) members.Add(CreateMember(plan.PlanId, regular.PlanCondId, dateTimeProvider));
                    break;

                case "DOCK":
                    // 人間ドック：30代以上、すべての性別、初回・定期
                    if (age30s != null) members.Add(CreateMember(plan.PlanId, age30s.PlanCondId, dateTimeProvider));
                    if (age40s != null) members.Add(CreateMember(plan.PlanId, age40s.PlanCondId, dateTimeProvider));
                    if (age50s != null) members.Add(CreateMember(plan.PlanId, age50s.PlanCondId, dateTimeProvider));
                    if (age60s != null) members.Add(CreateMember(plan.PlanId, age60s.PlanCondId, dateTimeProvider));
                    if (male != null) members.Add(CreateMember(plan.PlanId, male.PlanCondId, dateTimeProvider));
                    if (female != null) members.Add(CreateMember(plan.PlanId, female.PlanCondId, dateTimeProvider));
                    if (firstTime != null) members.Add(CreateMember(plan.PlanId, firstTime.PlanCondId, dateTimeProvider));
                    if (regular != null) members.Add(CreateMember(plan.PlanId, regular.PlanCondId, dateTimeProvider));
                    break;

                case "WOMAN":
                    // 女性健診：すべての年齢、女性のみ、初回・定期
                    if (age20s != null) members.Add(CreateMember(plan.PlanId, age20s.PlanCondId, dateTimeProvider));
                    if (age30s != null) members.Add(CreateMember(plan.PlanId, age30s.PlanCondId, dateTimeProvider));
                    if (age40s != null) members.Add(CreateMember(plan.PlanId, age40s.PlanCondId, dateTimeProvider));
                    if (age50s != null) members.Add(CreateMember(plan.PlanId, age50s.PlanCondId, dateTimeProvider));
                    if (age60s != null) members.Add(CreateMember(plan.PlanId, age60s.PlanCondId, dateTimeProvider));
                    if (female != null) members.Add(CreateMember(plan.PlanId, female.PlanCondId, dateTimeProvider));
                    if (firstTime != null) members.Add(CreateMember(plan.PlanId, firstTime.PlanCondId, dateTimeProvider));
                    if (regular != null) members.Add(CreateMember(plan.PlanId, regular.PlanCondId, dateTimeProvider));
                    break;

                case "SENIOR":
                    // 高齢者健診：50代以上、すべての性別、初回・定期
                    if (age50s != null) members.Add(CreateMember(plan.PlanId, age50s.PlanCondId, dateTimeProvider));
                    if (age60s != null) members.Add(CreateMember(plan.PlanId, age60s.PlanCondId, dateTimeProvider));
                    if (male != null) members.Add(CreateMember(plan.PlanId, male.PlanCondId, dateTimeProvider));
                    if (female != null) members.Add(CreateMember(plan.PlanId, female.PlanCondId, dateTimeProvider));
                    if (firstTime != null) members.Add(CreateMember(plan.PlanId, firstTime.PlanCondId, dateTimeProvider));
                    if (regular != null) members.Add(CreateMember(plan.PlanId, regular.PlanCondId, dateTimeProvider));
                    break;

                default:
                    // その他のプラン：すべての条件
                    foreach (var condition in conditions)
                    {
                        members.Add(CreateMember(plan.PlanId, condition.PlanCondId, dateTimeProvider));
                    }
                    break;
            }
        }

        context.PlanConditionMembers.AddRange(members);
        Console.WriteLine($"  [+] PlanConditionMembers: {members.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }

    private static PlanConditionMember CreateMember(Guid planId, Guid conditionId, IDateTimeProvider dateTimeProvider)
    {
        var member = new PlanConditionMember
        {
            PlanCondMemberId = Guid.CreateVersion7(),
            PlanId = planId,
            PlanCondId = conditionId,
            IsDeleted = false
        };

        SeederHelper.InitializeAuditFields(member, dateTimeProvider);
        return member;
    }
}

