using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentScheduleOverrideSeeder
{
    public static async Task SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
    {
        Console.WriteLine("[INFO] Creating appointment schedule overrides...");
        var startStopwatch = System.Diagnostics.Stopwatch.StartNew();

        // 既存のオーバーライドをロード
        var existingOverrides = await context.AppointmentScheduleOverrides
            .AsNoTracking()
            .Where(o => !o.IsDeleted)
            .Select(o => new { o.ApptScheduleId, o.ApptDate })
            .ToListAsync();
        var existingOverrideSet = existingOverrides
            .Select(o => (o.ApptScheduleId, o.ApptDate))
            .ToHashSet();

        // 既存の容量オーバーライドをロード
        var existingCapOverrides = await context.AppointmentScheduleSlotCapOverrides
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .Select(c => new { c.ApptScheduleSlotId, c.ApptDate })
            .ToListAsync();
        var existingCapOverrideSet = existingCapOverrides
            .Select(c => (c.ApptScheduleSlotId, c.ApptDate))
            .ToHashSet();

        Console.WriteLine($"  [DEBUG] Found {existingOverrideSet.Count} existing schedule overrides");
        Console.WriteLine($"  [DEBUG] Found {existingCapOverrideSet.Count} existing capacity overrides");

        var dateRange = SeederHelper.GetDefaultDateRange(dateTimeProvider);
        var startDate = dateRange.StartDate;
        var endDate = dateRange.EndDate;

        var overrides = new List<AppointmentScheduleOverride>();
        var overrideSlots = new List<AppointmentScheduleSlotOverride>();
        var capOverrides = new List<AppointmentScheduleSlotCapOverride>();
        var skippedOverrideCount = 0;
        var skippedCapOverrideCount = 0;

        // 同じSeed実行内で追加されるレコードも追跡するためのHashSet
        var newOverrideSet = new HashSet<(Guid ApptScheduleId, DateOnly ApptDate)>();
        var newCapOverrideSet = new HashSet<(Guid ApptScheduleSlotId, DateOnly ApptDate)>();

        // スケジュールとスロットを取得
        var schedules = await context.AppointmentSchedules
            .AsNoTracking()
            .Include(s => s.AppointmentScheduleSlots)
            .Where(s => !s.IsDeleted && s.IsActive)
            .ToListAsync();

        // 祝日を取得
        var holidays = await SeederHelper.LoadHolidaySetAsync(context);

        // 各リソースのスケジュールに対して、オーバーライドを作成
        foreach (var schedule in schedules)
        {
            var resource = await context.AppointmentResources
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ApptResId == schedule.ApptResId);

            if (resource == null)
                continue;

            // 祝日については何もしない（営業日判定はアプリケーション側で実施）
            // デモ用のオーバーライドを作成

            if (resource.ApptResTypeCode == (int)AppointmentResourceType.Main)
            {
                var (skippedOverrides, skippedCapOverrides) = CreateMainResourceOverrides(
                    schedule,
                    startDate,
                    endDate,
                    dateTimeProvider,
                    overrides,
                    overrideSlots,
                    capOverrides,
                    existingOverrideSet,
                    existingCapOverrideSet,
                    newOverrideSet,
                    newCapOverrideSet);
                skippedOverrideCount += skippedOverrides;
                skippedCapOverrideCount += skippedCapOverrides;
            }
        }

        Console.WriteLine($"  [DEBUG] Attempting to insert {overrides.Count} schedule overrides, {overrideSlots.Count} slot overrides, and {capOverrides.Count} capacity overrides");

        if (overrides.Any())
        {
            await context.AppointmentScheduleOverrides.AddRangeAsync(overrides);
        }

        if (overrideSlots.Any())
        {
            await context.AppointmentScheduleSlotOverrides.AddRangeAsync(overrideSlots);
        }

        if (capOverrides.Any())
        {
            await context.AppointmentScheduleSlotCapOverrides.AddRangeAsync(capOverrides);
        }

        if (overrides.Any() || overrideSlots.Any() || capOverrides.Any())
        {
            await context.SaveChangesAsync();
        }

        startStopwatch.Stop();
        Console.WriteLine($"  [+] Created {overrides.Count} schedule overrides with {capOverrides.Count} capacity adjustments");
        if (skippedOverrideCount > 0 || skippedCapOverrideCount > 0)
        {
            Console.WriteLine($"  [SKIP] Skipped {skippedOverrideCount} existing schedule overrides and {skippedCapOverrideCount} existing capacity overrides");
        }
        Console.WriteLine($"  [OK] Completed in {startStopwatch.Elapsed.TotalSeconds:F2}s");
    }

    /// <summary>
    /// Main リソース用のオーバーライドを作成
    /// 毎月1日：スロットオーバーライドで全スロットを再定義（容量1.5倍）
    /// 毎月2日：容量オーバーライドで容量のみ変更（容量1.5倍）
    /// </summary>
    /// <returns>スキップされたオーバーライド数と容量オーバーライド数のタプル</returns>
    private static (int SkippedOverrides, int SkippedCapOverrides) CreateMainResourceOverrides(
        AppointmentSchedule schedule,
        DateOnly startDate,
        DateOnly endDate,
        IDateTimeProvider dateTimeProvider,
        List<AppointmentScheduleOverride> overrides,
        List<AppointmentScheduleSlotOverride> overrideSlots,
        List<AppointmentScheduleSlotCapOverride> capOverrides,
        HashSet<(Guid ApptScheduleId, DateOnly ApptDate)> existingOverrideSet,
        HashSet<(Guid ApptScheduleSlotId, DateOnly ApptDate)> existingCapOverrideSet,
        HashSet<(Guid ApptScheduleId, DateOnly ApptDate)> newOverrideSet,
        HashSet<(Guid ApptScheduleSlotId, DateOnly ApptDate)> newCapOverrideSet)
    {
        var skippedOverrideCount = 0;
        var skippedCapOverrideCount = 0;

        var currentDate = startDate;
        while (currentDate <= endDate)
        {
            // 毎月1日：スロットオーバーライドで全スロットを再定義（容量1.5倍）
            if (currentDate.Day == 1 && currentDate.DayOfWeek != DayOfWeek.Sunday)
            {
                var overrideKey = (schedule.ApptScheduleId, currentDate);

                if (!existingOverrideSet.Contains(overrideKey) && !newOverrideSet.Contains(overrideKey))
                {
                    // オーバーライド親レコードを作成
                    var @override = new AppointmentScheduleOverride
                    {
                        ApptScheduleOverrideId = Guid.CreateVersion7(),
                        ApptScheduleId = schedule.ApptScheduleId,
                        ApptDate = currentDate
                    };

                    SeederHelper.InitializeAuditFields(@override, dateTimeProvider);
                    overrides.Add(@override);
                    newOverrideSet.Add(overrideKey);

                    // 該当日に有効なスロットを全て、容量1.5倍で再定義
                    foreach (var slot in schedule.AppointmentScheduleSlots.Where(s => !s.IsDeleted))
                    {
                        var dayOfWeek = (int)currentDate.DayOfWeek;

                        if (slot.DaysOfWeek.Contains(dayOfWeek))
                        {
                            var newCapacity = (int)Math.Ceiling(slot.SlotCap * 1.5);
                            var slotOverride = new AppointmentScheduleSlotOverride
                            {
                                ApptScheduleSlotOverrideId = Guid.CreateVersion7(),
                                ApptScheduleOverrideId = @override.ApptScheduleOverrideId,
                                SlotStartMin = slot.SlotStartMin,
                                SlotEndMin = slot.SlotEndMin,
                                SlotCap = newCapacity
                            };

                            SeederHelper.InitializeAuditFields(slotOverride, dateTimeProvider);
                            overrideSlots.Add(slotOverride);
                        }
                    }
                }
            }

            // 毎月2日：容量オーバーライドで容量のみ変更（容量1.5倍）
            if (currentDate.Day == 2 && currentDate.DayOfWeek != DayOfWeek.Sunday)
            {
                // 該当日のすべてのスロットに対して容量オーバーライドを作成
                foreach (var slot in schedule.AppointmentScheduleSlots.Where(s => !s.IsDeleted))
                {
                    var dayOfWeek = (int)currentDate.DayOfWeek;

                    if (slot.DaysOfWeek.Contains(dayOfWeek))
                    {
                        var capOverrideKey = (slot.ApptScheduleSlotId, currentDate);

                        if (!existingCapOverrideSet.Contains(capOverrideKey) && !newCapOverrideSet.Contains(capOverrideKey))
                        {
                            var newCapacity = (int)Math.Ceiling(slot.SlotCap * 1.5);
                            var capOverride = new AppointmentScheduleSlotCapOverride
                            {
                                ApptScheduleSlotCapOverrideId = Guid.CreateVersion7(),
                                ApptScheduleSlotId = slot.ApptScheduleSlotId,
                                ApptDate = currentDate,
                                SlotCap = newCapacity
                            };

                            SeederHelper.InitializeAuditFields(capOverride, dateTimeProvider);
                            capOverrides.Add(capOverride);
                            newCapOverrideSet.Add(capOverrideKey);
                        }
                        else
                        {
                            skippedCapOverrideCount++;
                        }
                    }
                }
            }

            currentDate = currentDate.AddDays(1);
        }

        return (skippedOverrideCount, skippedCapOverrideCount);
    }
}
