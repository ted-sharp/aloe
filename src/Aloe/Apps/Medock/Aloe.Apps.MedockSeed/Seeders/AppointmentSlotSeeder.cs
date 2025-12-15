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

            // リソースタイプに応じてスロット定義を作成
            switch (resource.ApptResTypeCode)
            {
                case 1: // 内視鏡
                case 3: // CT
                case 4: // MR
                    // 時間がかかる検査：15分単位の時間スロット
                    slot = CreateTimeSlotResource(resource.ApptResId, slotStartDate, slotEndDate);
                    break;

                case 2: // エコー
                    // AM/PMで大枠制限
                    slot = CreateAmPmSlotResource(resource.ApptResId, slotStartDate, slotEndDate, maxAm: 10, maxPm: 10);
                    break;

                case 5: // ロッカー
                    // AM/PMで各80個制限
                    slot = CreateAmPmSlotResource(resource.ApptResId, slotStartDate, slotEndDate, maxAm: 80, maxPm: 80);
                    break;

                default:
                    Console.WriteLine($"    [!] Unknown resource type: {resource.ApptResTypeCode} for {resource.ApptResName}");
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
    /// 時間スロット形式のリソース（内視鏡、CT、MR）を作成
    /// </summary>
    private static AppointmentSlot CreateTimeSlotResource(Guid resourceId, DateOnly startDate, DateOnly endDate)
    {
        // 15分単位のスロット（09:00-18:00の範囲、1スロット30分）
        var timeSlots = new List<object>();

        // 午前のスロット（09:00-11:45）
        foreach (var time in SeederHelper.TimeSlots.MorningSlots)
        {
            timeSlots.Add(new
            {
                time = time,
                max = 1,
                duration = 30
            });
        }

        // 午後のスロット（13:00-17:00）
        foreach (var time in SeederHelper.TimeSlots.AfternoonSlots)
        {
            timeSlots.Add(new
            {
                time = time,
                max = 1,
                duration = 30
            });
        }

        var slotsJson = JsonSerializer.Serialize(new { slots = timeSlots });

        return new AppointmentSlot
        {
            ApptSlotId = Guid.CreateVersion7(),
            ApptResId = resourceId,
            ApptSlots = slotsJson,
            IsActive = true,
            ActiveFrom = startDate,
            ActiveTo = endDate,
            IsDeleted = false
        };
    }

    /// <summary>
    /// AM/PM制限形式のリソース（エコー、ロッカー）を作成
    /// </summary>
    private static AppointmentSlot CreateAmPmSlotResource(Guid resourceId, DateOnly startDate, DateOnly endDate, int maxAm, int maxPm)
    {
        var timeSlots = new List<object>
        {
            new { time = "AM", max = maxAm, duration = 0 },
            new { time = "PM", max = maxPm, duration = 0 }
        };

        var slotsJson = JsonSerializer.Serialize(new { slots = timeSlots });

        return new AppointmentSlot
        {
            ApptSlotId = Guid.CreateVersion7(),
            ApptResId = resourceId,
            ApptSlots = slotsJson,
            IsActive = true,
            ActiveFrom = startDate,
            ActiveTo = endDate,
            IsDeleted = false
        };
    }
}

