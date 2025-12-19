using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentResourceGroupMemberSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingMembers = await context.AppointmentResourceGroupMembers.AnyAsync();
        if (existingMembers)
        {
            Console.WriteLine("[SKIP] AppointmentResourceGroupMembers already exist.");
            return;
        }

        var resources = await context.AppointmentResources
            .Where(r => !r.IsDeleted)
            .ToListAsync();

        if (!resources.Any())
        {
            Console.WriteLine("[SKIP] AppointmentResourceGroupMember: No appointment resources found.");
            return;
        }

        var groups = await context.AppointmentResourceGroups
            .Where(g => !g.IsDeleted && g.FacilityId == facilityId)
            .ToListAsync();

        if (!groups.Any())
        {
            Console.WriteLine("[SKIP] AppointmentResourceGroupMember: No appointment resource groups found.");
            return;
        }

        Console.WriteLine("[INFO] Creating appointment resource group member seed data...");

        var members = new List<AppointmentResourceGroupMember>();

        // グループコードでグループを取得
        var echoRoomGroup = groups.FirstOrDefault(g => g.ResGroupCode == "ECHO_ROOM");
        var stomachExamBothGroup = groups.FirstOrDefault(g => g.ResGroupCode == "STOMACH_EXAM_BOTH");
        var stomachExamBothExtGroup = groups.FirstOrDefault(g => g.ResGroupCode == "STOMACH_EXAM_BOTH_EXT");
        var endoscopeOnlyGroup = groups.FirstOrDefault(g => g.ResGroupCode == "ENDOSCOPE_ONLY");
        var endoscopeOnlyExtGroup = groups.FirstOrDefault(g => g.ResGroupCode == "ENDOSCOPE_ONLY_EXT");

        // リソース名でリソースを取得
        var endoscope = resources.FirstOrDefault(r => r.ApptResName == "内視鏡");
        var endoscopeExt = resources.FirstOrDefault(r => r.ApptResName == "内視鏡(外部)");
        var abdomenEcho = resources.FirstOrDefault(r => r.ApptResName == "腹部エコー");
        var breastEcho = resources.FirstOrDefault(r => r.ApptResName == "乳腺エコー");
        var carotidEcho = resources.FirstOrDefault(r => r.ApptResName == "頸動脈エコー");
        var stomachBa = resources.FirstOrDefault(r => r.ApptResName == "胃Ba");

        // エコーグループにすべてのエコーリソースを追加
        if (echoRoomGroup != null)
        {
            if (abdomenEcho != null)
            {
                members.Add(CreateMember(echoRoomGroup.ApptResGroupId, abdomenEcho.ApptResId, dateTimeProvider));
            }
            if (breastEcho != null)
            {
                members.Add(CreateMember(echoRoomGroup.ApptResGroupId, breastEcho.ApptResId, dateTimeProvider));
            }
            if (carotidEcho != null)
            {
                members.Add(CreateMember(echoRoomGroup.ApptResGroupId, carotidEcho.ApptResId, dateTimeProvider));
            }
        }

        // 胃部検査(内視鏡または胃Ba)グループに内視鏡と胃Baを追加
        if (stomachExamBothGroup != null)
        {
            if (endoscope != null)
            {
                members.Add(CreateMember(stomachExamBothGroup.ApptResGroupId, endoscope.ApptResId, dateTimeProvider));
            }
            if (stomachBa != null)
            {
                members.Add(CreateMember(stomachExamBothGroup.ApptResGroupId, stomachBa.ApptResId, dateTimeProvider));
            }
        }

        // 胃部検査(内視鏡または胃Ba)-外部委託含むグループに内視鏡、内視鏡(外部)、胃Baを追加
        if (stomachExamBothExtGroup != null)
        {
            if (endoscope != null)
            {
                members.Add(CreateMember(stomachExamBothExtGroup.ApptResGroupId, endoscope.ApptResId, dateTimeProvider));
            }
            if (endoscopeExt != null)
            {
                members.Add(CreateMember(stomachExamBothExtGroup.ApptResGroupId, endoscopeExt.ApptResId, dateTimeProvider));
            }
            if (stomachBa != null)
            {
                members.Add(CreateMember(stomachExamBothExtGroup.ApptResGroupId, stomachBa.ApptResId, dateTimeProvider));
            }
        }

        // 内視鏡グループに内視鏡を追加
        if (endoscopeOnlyGroup != null && endoscope != null)
        {
            members.Add(CreateMember(endoscopeOnlyGroup.ApptResGroupId, endoscope.ApptResId, dateTimeProvider));
        }

        // 内視鏡-外部委託含むグループに内視鏡(外部)を追加
        if (endoscopeOnlyExtGroup != null && endoscopeExt != null)
        {
            members.Add(CreateMember(endoscopeOnlyExtGroup.ApptResGroupId, endoscopeExt.ApptResId, dateTimeProvider));
        }

        context.AppointmentResourceGroupMembers.AddRange(members);
        Console.WriteLine($"  [+] AppointmentResourceGroupMembers: {members.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }

    private static AppointmentResourceGroupMember CreateMember(Guid groupId, Guid resourceId, IDateTimeProvider dateTimeProvider)
    {
        var member = new AppointmentResourceGroupMember
        {
            ApptResGroupMemberId = Guid.CreateVersion7(),
            ApptResGroupId = groupId,
            ApptResId = resourceId,
            IsDeleted = false
        };

        SeederHelper.InitializeAuditFields(member, dateTimeProvider);
        return member;
    }
}

