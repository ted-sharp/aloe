using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentScheduleSeeder
{
    public static async Task SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
    {
        // テーブルが存在するか確認
        if (await context.AppointmentSchedules.AnyAsync())
        {
            Console.WriteLine("[SKIP] AppointmentSchedules already exist.");
            return;
        }

        Console.WriteLine("[INFO] Creating appointment schedules...");
        var startStopwatch = System.Diagnostics.Stopwatch.StartNew();

        var dateRange = SeederHelper.GetDefaultDateRange(dateTimeProvider);
        var startDate = dateRange.StartDate;
        var endDate = dateRange.EndDate;

        // 全リソースを取得
        var resources = await context.AppointmentResources
            .AsNoTracking()
            .Where(r => !r.IsDeleted)
            .ToListAsync();

        var schedules = new List<AppointmentSchedule>();
        var scheduleSlots = new List<AppointmentScheduleSlot>();

        // リソースごとにスケジュールを作成
        foreach (var resource in resources)
        {
            if (resource.ApptResTypeCode == (int)AppointmentResourceType.Main)
            {
                CreateMainResourceSchedule(
                    resource,
                    startDate,
                    endDate,
                    dateTimeProvider,
                    schedules,
                    scheduleSlots);
            }
            else if (resource.ApptResTypeCode == (int)AppointmentResourceType.Equipment)
            {
                CreateEquipmentResourceSchedule(
                    resource,
                    startDate,
                    endDate,
                    dateTimeProvider,
                    schedules,
                    scheduleSlots);
            }
        }

        if (schedules.Any())
        {
            await context.AppointmentSchedules.AddRangeAsync(schedules);
            await context.AppointmentScheduleSlots.AddRangeAsync(scheduleSlots);
            await context.SaveChangesAsync();
        }

        startStopwatch.Stop();
        Console.WriteLine($"  [+] Created {schedules.Count} schedules with {scheduleSlots.Count} slots - took {startStopwatch.Elapsed.TotalSeconds:F2}s");
    }

    /// <summary>
    /// Main リソース用スケジュール作成（月〜土営業、30分単位）
    /// 月火木金：9:00-17:00（全日営業）
    /// 水曜・土曜：9:00-12:30（午前のみ、半日営業）
    /// </summary>
    private static void CreateMainResourceSchedule(
        AppointmentResource resource,
        DateOnly startDate,
        DateOnly endDate,
        IDateTimeProvider dateTimeProvider,
        List<AppointmentSchedule> schedules,
        List<AppointmentScheduleSlot> scheduleSlots)
    {
        var schedule = new AppointmentSchedule
        {
            ApptScheduleId = Guid.CreateVersion7(),
            ApptResId = resource.ApptResId,
            IsActive = true,
            ActiveFrom = startDate,
            ActiveTo = endDate
        };

        SeederHelper.InitializeAuditFields(schedule, dateTimeProvider);
        schedules.Add(schedule);

        const int slotDurationMin = 30;    // 30分単位
        const int capacity = 20;            // 1スロットあたり20件

        // 月火木金：9:00-17:00（通常営業日、ランチタイム12:00-13:00除外）
        const int businessStartMin = 540;  // 9:00
        const int lunchStartMin = 720;    // 12:00
        const int lunchEndMin = 780;      // 13:00
        const int businessEndMin = 1020;   // 17:00
        var normalDays = new[] { 1, 2, 4, 5 }; // 月火木金

        // 午前：9:00-12:00
        for (int min = businessStartMin; min < lunchStartMin; min += slotDurationMin)
        {
            scheduleSlots.Add(new AppointmentScheduleSlot
            {
                ApptScheduleSlotId = Guid.CreateVersion7(),
                ApptScheduleId = schedule.ApptScheduleId,
                DaysOfWeek = normalDays,
                SlotStartMin = min,
                SlotEndMin = min + slotDurationMin,
                SlotCap = capacity
            });

            SeederHelper.InitializeAuditFields(scheduleSlots.Last(), dateTimeProvider);
        }

        // 午後：13:00-17:00
        for (int min = lunchEndMin; min < businessEndMin; min += slotDurationMin)
        {
            scheduleSlots.Add(new AppointmentScheduleSlot
            {
                ApptScheduleSlotId = Guid.CreateVersion7(),
                ApptScheduleId = schedule.ApptScheduleId,
                DaysOfWeek = normalDays,
                SlotStartMin = min,
                SlotEndMin = min + slotDurationMin,
                SlotCap = capacity
            });

            SeederHelper.InitializeAuditFields(scheduleSlots.Last(), dateTimeProvider);
        }

        // 水曜・土曜：9:00-12:30（午前のみ、半日営業）
        const int halfDayEndMin = 750;   // 12:30
        var halfDays = new[] { 3, 6 };   // 水曜・土曜

        for (int min = businessStartMin; min < halfDayEndMin; min += slotDurationMin)
        {
            scheduleSlots.Add(new AppointmentScheduleSlot
            {
                ApptScheduleSlotId = Guid.CreateVersion7(),
                ApptScheduleId = schedule.ApptScheduleId,
                DaysOfWeek = halfDays,
                SlotStartMin = min,
                SlotEndMin = min + slotDurationMin,
                SlotCap = capacity
            });

            SeederHelper.InitializeAuditFields(scheduleSlots.Last(), dateTimeProvider);
        }
    }

    /// <summary>
    /// Equipment リソース用スケジュール作成（時間単位、可変スケジュール）
    /// </summary>
    private static void CreateEquipmentResourceSchedule(
        AppointmentResource resource,
        DateOnly startDate,
        DateOnly endDate,
        IDateTimeProvider dateTimeProvider,
        List<AppointmentSchedule> schedules,
        List<AppointmentScheduleSlot> scheduleSlots)
    {
        var schedule = new AppointmentSchedule
        {
            ApptScheduleId = Guid.CreateVersion7(),
            ApptResId = resource.ApptResId,
            IsActive = true,
            ActiveFrom = startDate,
            ActiveTo = endDate
        };

        SeederHelper.InitializeAuditFields(schedule, dateTimeProvider);
        schedules.Add(schedule);

        // Equipment は 1 時間単位で営業
        // 例：CT/MR は午前と午後の2時間ブロック
        const int slotDurationMin = 60;    // 1時間単位
        const int capacity = 5;             // Equipment は容量が少ない

        // 午前枠：9:00-12:00
        var morningSlots = new List<AppointmentScheduleSlot>();
        for (int min = 540; min < 720; min += slotDurationMin)
        {
            morningSlots.Add(new AppointmentScheduleSlot
            {
                ApptScheduleSlotId = Guid.CreateVersion7(),
                ApptScheduleId = schedule.ApptScheduleId,
                DaysOfWeek = new[] { 1, 2, 3, 4, 5, 6 }, // 月〜土
                SlotStartMin = min,
                SlotEndMin = min + slotDurationMin,
                SlotCap = capacity
            });

            SeederHelper.InitializeAuditFields(morningSlots.Last(), dateTimeProvider);
        }

        // 午後枠：13:00-17:00
        // 水曜日（3）と土曜日（6）は半日営業のため、午後のスロットは作成しない
        var afternoonSlots = new List<AppointmentScheduleSlot>();
        for (int min = 780; min < 1020; min += slotDurationMin)
        {
            afternoonSlots.Add(new AppointmentScheduleSlot
            {
                ApptScheduleSlotId = Guid.CreateVersion7(),
                ApptScheduleId = schedule.ApptScheduleId,
                DaysOfWeek = new[] { 1, 2, 4, 5 }, // 月火木金（水曜・土曜は除外）
                SlotStartMin = min,
                SlotEndMin = min + slotDurationMin,
                SlotCap = capacity
            });

            SeederHelper.InitializeAuditFields(afternoonSlots.Last(), dateTimeProvider);
        }

        scheduleSlots.AddRange(morningSlots);
        scheduleSlots.AddRange(afternoonSlots);
    }
}
