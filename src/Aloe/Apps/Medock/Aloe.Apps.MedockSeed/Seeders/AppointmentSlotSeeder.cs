using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentSlotSeeder
{
    public static async Task SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
    {
        var existingSlots = await context.AppointmentSlots.AnyAsync();
        if (existingSlots)
        {
            Console.WriteLine("[SKIP] AppointmentSlots already exist.");
            return;
        }

        var resources = await context.AppointmentResources
            .Where(r => !r.IsDeleted)
            .Include(r => r.Floor)
            .ToListAsync();

        if (!resources.Any())
        {
            Console.WriteLine("[SKIP] AppointmentSlot: No appointment resources found.");
            return;
        }

        // 施設のビジネスアワーを取得
        var facilityIds = resources.Select(r => r.Floor.FacilityId).Distinct().ToList();
        var businessHoursDict = await context.FacilityBusinessHours
            .Where(fbh => facilityIds.Contains(fbh.FacilityId) && fbh.IsActive && !fbh.IsDeleted)
            .ToDictionaryAsync(fbh => fbh.FacilityId, fbh => fbh.BusinessHoursData);

        Console.WriteLine("[INFO] Creating appointment slot seed data...");

        var slots = new List<AppointmentSlot>();
        // スロット定義は過去3年～未来2年程度
        var slotStartDate = dateTimeProvider.TodayDateOnly.AddYears(-3);
        var slotEndDate = dateTimeProvider.TodayDateOnly.AddYears(2);

        foreach (var resource in resources)
        {
            // リソースに対応するビジネスアワーを取得（なければデフォルト値）
            var businessHours = businessHoursDict.GetValueOrDefault(resource.Floor.FacilityId)
                ?? new FacilityBusinessHoursRoot();

            AppointmentSlot slot;

            // リソース名とタイプに応じてスロット定義を作成
            if (resource.ApptResName == "内視鏡" || resource.ApptResName == "内視鏡(外部)")
            {
                // 胃部内視鏡：20分に1スロット（1時間に3スロット）
                slot = CreateTimeSlotResource(resource.ApptResId, slotStartDate, slotEndDate, intervalMinutes: 20, businessHours);
            }
            else if (resource.ApptResName == "CT")
            {
                // CT：10分に1スロット（1時間に6スロット）
                slot = CreateTimeSlotResource(resource.ApptResId, slotStartDate, slotEndDate, intervalMinutes: 10, businessHours);
            }
            else if (resource.ApptResName == "MR")
            {
                // MR：30分に1スロット（1時間に2スロット）
                slot = CreateTimeSlotResource(resource.ApptResId, slotStartDate, slotEndDate, intervalMinutes: 30, businessHours);
            }
            else if (resource.ApptResName == "腹部エコー" || resource.ApptResName == "乳腺エコー" || resource.ApptResName == "頸動脈エコー")
            {
                // エコー：15分に1スロット（1時間に4スロット）
                slot = CreateTimeSlotResource(resource.ApptResId, slotStartDate, slotEndDate, intervalMinutes: 15, businessHours);
            }
            else if (resource.AppointmentResourceType == Aloe.Apps.MedockLib.Constants.AppointmentResourceType.Main)
            {
                // 30分区切りで業務時間を分割、1時間当たり40人（1スロット20人）
                slot = CreateMainSlotResource(resource.ApptResId, slotStartDate, slotEndDate, maxPerSlot: 20, businessHours);
            }
            else if (resource.ApptResName == "胃Ba")
            {
                // 胃Ba：時間スロット形式（デフォルト20分間隔）
                slot = CreateTimeSlotResource(resource.ApptResId, slotStartDate, slotEndDate, intervalMinutes: 20, businessHours);
            }
            else
            {
                Console.WriteLine($"    [!] Unknown resource: {resource.ApptResName} (Type: {resource.ApptResTypeCode})");
                continue;
            }

            SeederHelper.InitializeAuditFields(slot, dateTimeProvider);
            slots.Add(slot);
        }

        context.AppointmentSlots.AddRange(slots);
        Console.WriteLine($"  [+] AppointmentSlots: {slots.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 時間スロット形式のリソース（内視鏡、CT、MR、エコー）を作成
    /// </summary>
    private static AppointmentSlot CreateTimeSlotResource(
        Guid resourceId,
        DateOnly startDate,
        DateOnly endDate,
        int intervalMinutes,
        FacilityBusinessHoursRoot businessHours)
    {
        var timeSlots = new List<AppointmentSlotItem>();

        // ビジネスアワーから時刻を取得
        var businessStart = TimeOnly.Parse(businessHours.Start);
        var businessEnd = TimeOnly.Parse(businessHours.End);
        var lunchStart = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.Start) : new TimeOnly(12, 0);
        var lunchEnd = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.End) : new TimeOnly(13, 0);

        // 午前のスロット（businessStart ～ lunchStart）
        var currentTime = businessStart;
        while (currentTime < lunchStart)
        {
            var endTime = currentTime.AddMinutes(intervalMinutes);
            if (endTime > lunchStart) endTime = lunchStart;

            timeSlots.Add(new AppointmentSlotItem
            {
                Start = currentTime.Hour * 60 + currentTime.Minute,
                End = endTime.Hour * 60 + endTime.Minute,
                Cap = 1
            });
            currentTime = endTime;
        }

        // 午後のスロット（lunchEnd ～ businessEnd）
        currentTime = lunchEnd;
        while (currentTime < businessEnd)
        {
            var endTime = currentTime.AddMinutes(intervalMinutes);
            if (endTime > businessEnd) endTime = businessEnd;

            timeSlots.Add(new AppointmentSlotItem
            {
                Start = currentTime.Hour * 60 + currentTime.Minute,
                End = endTime.Hour * 60 + endTime.Minute,
                Cap = 1
            });
            currentTime = endTime;
        }

        // 時間外スロットを追加
        // 早朝スロット（07:00 ～ businessStart）
        var outsideHoursStart = new TimeOnly(7, 0);
        if (businessStart > outsideHoursStart)
        {
            timeSlots.Add(new AppointmentSlotItem
            {
                Start = outsideHoursStart.Hour * 60 + outsideHoursStart.Minute,
                End = businessStart.Hour * 60 + businessStart.Minute,
                Cap = 0,
                IsOutsideHours = true
            });
        }

        // 昼休みスロット（lunchStart ～ lunchEnd）
        if (lunchEnd > lunchStart)
        {
            timeSlots.Add(new AppointmentSlotItem
            {
                Start = lunchStart.Hour * 60 + lunchStart.Minute,
                End = lunchEnd.Hour * 60 + lunchEnd.Minute,
                Cap = 0,
                IsOutsideHours = true
            });
        }

        // 夕方スロット（businessEnd ～ 19:00）
        var outsideHoursEnd = new TimeOnly(19, 0);
        if (businessEnd < outsideHoursEnd)
        {
            timeSlots.Add(new AppointmentSlotItem
            {
                Start = businessEnd.Hour * 60 + businessEnd.Minute,
                End = outsideHoursEnd.Hour * 60 + outsideHoursEnd.Minute,
                Cap = 0,
                IsOutsideHours = true
            });
        }

        var slotDefinition = new AppointmentSlotRoot
        {
            Slots = timeSlots
        };

        return new AppointmentSlot
        {
            ApptSlotId = Guid.CreateVersion7(),
            ApptResId = resourceId,
            ApptSlotsData = slotDefinition,
            IsActive = true,
            ActiveFrom = startDate,
            ActiveTo = endDate,
            IsDeleted = false
        };
    }

    /// <summary>
    /// Mainタイプのリソースを作成
    /// 30分区切りで業務時間を分割
    /// </summary>
    private static AppointmentSlot CreateMainSlotResource(
        Guid resourceId,
        DateOnly startDate,
        DateOnly endDate,
        int maxPerSlot,
        FacilityBusinessHoursRoot businessHours)
    {
        var timeSlots = new List<AppointmentSlotItem>();
        const int intervalMinutes = 30;

        // ビジネスアワーから時刻を取得
        var businessStart = TimeOnly.Parse(businessHours.Start);
        var businessEnd = TimeOnly.Parse(businessHours.End);
        var lunchStart = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.Start) : new TimeOnly(12, 0);
        var lunchEnd = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.End) : new TimeOnly(13, 0);

        // 午前のスロット（businessStart ～ lunchStart、30分区切り）
        var currentTime = businessStart;
        while (currentTime < lunchStart)
        {
            var endTime = currentTime.AddMinutes(intervalMinutes);
            if (endTime > lunchStart) endTime = lunchStart;

            timeSlots.Add(new AppointmentSlotItem
            {
                Start = currentTime.Hour * 60 + currentTime.Minute,
                End = endTime.Hour * 60 + endTime.Minute,
                Cap = maxPerSlot
            });
            currentTime = endTime;
        }

        // 午後のスロット（lunchEnd ～ businessEnd、30分区切り）
        currentTime = lunchEnd;
        while (currentTime < businessEnd)
        {
            var endTime = currentTime.AddMinutes(intervalMinutes);
            if (endTime > businessEnd) endTime = businessEnd;

            timeSlots.Add(new AppointmentSlotItem
            {
                Start = currentTime.Hour * 60 + currentTime.Minute,
                End = endTime.Hour * 60 + endTime.Minute,
                Cap = maxPerSlot
            });
            currentTime = endTime;
        }

        // 時間外スロットを追加
        // 早朝スロット（07:00 ～ businessStart）
        var outsideHoursStart = new TimeOnly(7, 0);
        if (businessStart > outsideHoursStart)
        {
            timeSlots.Add(new AppointmentSlotItem
            {
                Start = outsideHoursStart.Hour * 60 + outsideHoursStart.Minute,
                End = businessStart.Hour * 60 + businessStart.Minute,
                Cap = 0,
                IsOutsideHours = true
            });
        }

        // 昼休みスロット（lunchStart ～ lunchEnd）
        if (lunchEnd > lunchStart)
        {
            timeSlots.Add(new AppointmentSlotItem
            {
                Start = lunchStart.Hour * 60 + lunchStart.Minute,
                End = lunchEnd.Hour * 60 + lunchEnd.Minute,
                Cap = 0,
                IsOutsideHours = true
            });
        }

        // 夕方スロット（businessEnd ～ 19:00）
        var outsideHoursEnd = new TimeOnly(19, 0);
        if (businessEnd < outsideHoursEnd)
        {
            timeSlots.Add(new AppointmentSlotItem
            {
                Start = businessEnd.Hour * 60 + businessEnd.Minute,
                End = outsideHoursEnd.Hour * 60 + outsideHoursEnd.Minute,
                Cap = 0,
                IsOutsideHours = true
            });
        }

        var slotDefinition = new AppointmentSlotRoot
        {
            Slots = timeSlots
        };

        return new AppointmentSlot
        {
            ApptSlotId = Guid.CreateVersion7(),
            ApptResId = resourceId,
            ApptSlotsData = slotDefinition,
            IsActive = true,
            ActiveFrom = startDate,
            ActiveTo = endDate,
            IsDeleted = false
        };
    }
}
