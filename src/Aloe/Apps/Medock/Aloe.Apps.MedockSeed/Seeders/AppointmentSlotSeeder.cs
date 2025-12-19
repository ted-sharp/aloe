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
            .ToListAsync();

        if (!resources.Any())
        {
            Console.WriteLine("[SKIP] AppointmentSlot: No appointment resources found.");
            return;
        }

        Console.WriteLine("[INFO] Creating appointment slot seed data...");

        var slots = new List<AppointmentSlot>();
        var (startDate, endDate) = SeederHelper.GetDefaultDateRange(dateTimeProvider);
        // スロット定義は過去1年～未来2年程度
        var slotStartDate = dateTimeProvider.TodayDateOnly.AddYears(-1);
        var slotEndDate = dateTimeProvider.TodayDateOnly.AddYears(2);

        foreach (var resource in resources)
        {
            AppointmentSlot slot;

            // リソース名とタイプに応じてスロット定義を作成
            if (resource.ApptResName == "内視鏡" || resource.ApptResName == "内視鏡(外部)")
            {
                // 胃部内視鏡：20分に1スロット（1時間に3スロット）
                slot = CreateTimeSlotResource(resource.ApptResId, slotStartDate, slotEndDate, intervalMinutes: 20);
            }
            else if (resource.ApptResName == "CT")
            {
                // CT：10分に1スロット（1時間に6スロット）
                slot = CreateTimeSlotResource(resource.ApptResId, slotStartDate, slotEndDate, intervalMinutes: 10);
            }
            else if (resource.ApptResName == "MR")
            {
                // MR：30分に1スロット（1時間に2スロット）
                slot = CreateTimeSlotResource(resource.ApptResId, slotStartDate, slotEndDate, intervalMinutes: 30);
            }
            else if (resource.ApptResName == "腹部エコー" || resource.ApptResName == "乳腺エコー" || resource.ApptResName == "頸動脈エコー")
            {
                // エコー：15分に1スロット（1時間に4スロット）
                slot = CreateTimeSlotResource(resource.ApptResId, slotStartDate, slotEndDate, intervalMinutes: 15);
            }
            else if (resource.AppointmentResourceType == Aloe.Apps.MedockLib.Constants.AppointmentResourceType.Main) // ロッカー（Mainタイプ）
            {
                // 30分区切りで業務時間を分割、1時間当たり20人（1スロット10人）
                // 午前: 8:00-12:00（4時間）= 8スロット、午後: 13:00-17:00（4時間）= 8スロット
                slot = CreateMainSlotResource(resource.ApptResId, slotStartDate, slotEndDate, maxPerSlot: 10);
            }
            else if (resource.ApptResTypeCode == 6) // 胃Ba（旧コード、Equipmentに移行予定。enum に該当値がないため int 比較のまま）
            {
                // 胃Ba：時間スロット形式（デフォルト20分間隔）
                slot = CreateTimeSlotResource(resource.ApptResId, slotStartDate, slotEndDate, intervalMinutes: 20);
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
    private static AppointmentSlot CreateTimeSlotResource(Guid resourceId, DateOnly startDate, DateOnly endDate, int intervalMinutes)
    {
        // 指定された間隔でスロットを生成（09:00-12:00、13:00-17:00の範囲、1時間の枠で作成）
        var timeSlots = new List<AppointmentSlotItem>();

        // 午前のスロット（09:00-12:00）
        for (int hour = 9; hour < 12; hour++)
        {
            // 1時間の枠内で、指定された間隔でスロットを作成
            for (int minute = 0; minute < 60; minute += intervalMinutes)
            {
                var timeString = $"{hour:D2}:{minute:D2}";
                timeSlots.Add(new AppointmentSlotItem
                {
                    Time = timeString,
                    Max = 1,
                    Duration = intervalMinutes
                });
            }
        }

        // 午後のスロット（13:00-17:00）
        for (int hour = 13; hour <= 17; hour++)
        {
            // 1時間の枠内で、指定された間隔でスロットを作成
            for (int minute = 0; minute < 60; minute += intervalMinutes)
            {
                var timeString = $"{hour:D2}:{minute:D2}";
                timeSlots.Add(new AppointmentSlotItem
                {
                    Time = timeString,
                    Max = 1,
                    Duration = intervalMinutes
                });
            }
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
    /// AM/PM制限形式のリソース（エコーなど）を作成
    /// </summary>
    private static AppointmentSlot CreateAmPmSlotResource(Guid resourceId, DateOnly startDate, DateOnly endDate, int maxAm, int maxPm)
    {
        var timeSlots = new List<AppointmentSlotItem>
        {
            new AppointmentSlotItem
            {
                Time = "AM",
                Max = maxAm,
                Duration = 0
            },
            new AppointmentSlotItem
            {
                Time = "PM",
                Max = maxPm,
                Duration = 0
            }
        };

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
    /// Mainタイプのリソース（ロッカーなど）を作成
    /// 30分区切りで業務時間を分割、1時間当たり20人（1スロット10人）
    /// 午前: 8:00-12:00（4時間）= 8スロット、午後: 13:00-17:00（4時間）= 8スロット
    /// </summary>
    private static AppointmentSlot CreateMainSlotResource(Guid resourceId, DateOnly startDate, DateOnly endDate, int maxPerSlot)
    {
        var timeSlots = new List<AppointmentSlotItem>();

        // 午前のスロット（8:00-12:00、30分区切り）
        for (int hour = 8; hour < 12; hour++)
        {
            for (int minute = 0; minute < 60; minute += 30)
            {
                var timeString = $"{hour:D2}:{minute:D2}";
                timeSlots.Add(new AppointmentSlotItem
                {
                    Time = timeString,
                    Max = maxPerSlot, // 1スロット10人
                    Duration = 30 // 30分間隔
                });
            }
        }

        // 午後のスロット（13:00-16:30、30分区切り、4時間=8スロット）
        for (int hour = 13; hour < 17; hour++)
        {
            for (int minute = 0; minute < 60; minute += 30)
            {
                var timeString = $"{hour:D2}:{minute:D2}";
                timeSlots.Add(new AppointmentSlotItem
                {
                    Time = timeString,
                    Max = maxPerSlot, // 1スロット10人
                    Duration = 30 // 30分間隔
                });
            }
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
