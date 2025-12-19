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
        var examRoomGroup = groups.FirstOrDefault(g => g.ResGroupCode == "EXAM_ROOM");
        var echoRoomGroup = groups.FirstOrDefault(g => g.ResGroupCode == "ECHO_ROOM");
        var lockerClusterGroup = groups.FirstOrDefault(g => g.ResGroupCode == "LOCKER_CLUSTER");

        // リソース名でリソースを取得
        var endoscope = resources.FirstOrDefault(r => r.ApptResName == "内視鏡");
        var ct = resources.FirstOrDefault(r => r.ApptResName == "CT");
        var mr = resources.FirstOrDefault(r => r.ApptResName == "MR");
        var echo = resources.FirstOrDefault(r => r.ApptResName == "エコー");
        var locker = resources.FirstOrDefault(r => r.ApptResName == "ロッカー");

        // 検査室グループに内視鏡、CT、MRを追加
        if (examRoomGroup != null)
        {
            if (endoscope != null)
            {
                members.Add(CreateMember(examRoomGroup.ApptResGroupId, endoscope.ApptResId, dateTimeProvider));
            }
            if (ct != null)
            {
                members.Add(CreateMember(examRoomGroup.ApptResGroupId, ct.ApptResId, dateTimeProvider));
            }
            if (mr != null)
            {
                members.Add(CreateMember(examRoomGroup.ApptResGroupId, mr.ApptResId, dateTimeProvider));
            }
        }

        // エコー室グループにエコーを追加
        if (echoRoomGroup != null && echo != null)
        {
            members.Add(CreateMember(echoRoomGroup.ApptResGroupId, echo.ApptResId, dateTimeProvider));
        }

        // ロッカークラスターにロッカーを追加
        if (lockerClusterGroup != null && locker != null)
        {
            members.Add(CreateMember(lockerClusterGroup.ApptResGroupId, locker.ApptResId, dateTimeProvider));
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

