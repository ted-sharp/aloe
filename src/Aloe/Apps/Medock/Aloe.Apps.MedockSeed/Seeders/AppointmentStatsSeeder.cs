using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Diagnostics;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentStatsSeeder
{
    public static async Task SeedAsync(
        MedockDbContext context,
        IDbContextFactory<MedockDbContext> contextFactory,
        IDateTimeProvider dateTimeProvider,
        Dictionary<(Guid ApptResId, DateOnly ApptDate, int SlotStartMin), int> slotAggregation)
    {
        // テーブルが存在するか確認
        try
        {
            var hasExistingData = await context.AppointmentStats.AnyAsync();
            if (hasExistingData)
            {
                Console.WriteLine("[SKIP] AppointmentStats already exist.");
                return;
            }
        }
        catch (PostgresException ex) when (ex.SqlState == "42P01")
        {
            // テーブルが存在しない場合は続行（初回実行時）
        }

        Console.WriteLine("[INFO] Aggregating appointment stats from appointments...");
        var startStopwatch = System.Diagnostics.Stopwatch.StartNew();

        var dateRange = SeederHelper.GetDefaultDateRange(dateTimeProvider);
        var startDate = dateRange.StartDate;
        var endDate = dateRange.EndDate;

        // 全スケジュールを取得（スロットとオーバーライドを含む）
        var schedules = await context.AppointmentSchedules
            .AsNoTracking()
            .Include(s => s.AppointmentScheduleSlots)
                .ThenInclude(slot => slot.CapOverrides)
            .Include(s => s.AppointmentScheduleOverrides)
                .ThenInclude(o => o.AppointmentScheduleSlotOverrides)
            .Where(s => !s.IsDeleted && s.IsActive)
            .ToListAsync();

        // 祝日を取得
        var holidays = await SeederHelper.LoadHolidaySetAsync(context);

        // In-memory aggregation data received from AppointmentSeeder
        Console.WriteLine($"[INFO] Processing {slotAggregation.Count} slot aggregations...");

        // Pre-index aggregation by (ResId, Date) for faster out-of-hours lookup
        var aggregationByResAndDate = slotAggregation
            .GroupBy(kvp => (kvp.Key.ApptResId, kvp.Key.ApptDate))
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(kvp => kvp.Key.SlotStartMin, kvp => kvp.Value));

        var stats = new List<AppointmentStats>();
        var statSlots = new List<AppointmentStatSlots>();

        var processSw = Stopwatch.StartNew();
        // Process each schedule (resource)
        foreach (var schedule in schedules)
        {
            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                // 日曜日・祝日はスキップ（休診日）
                if (currentDate.DayOfWeek == DayOfWeek.Sunday || holidays.Contains(currentDate))
                {
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                // Get applicable slots for this date (from schedule configuration)
                var applicableSlots = GetApplicableSlotsForDate(schedule, currentDate);

                if (applicableSlots.Any())
                {
                    // Create AppointmentStatSlots records (with accurate counts from aggregation)
                    var slotRecords = new List<AppointmentStatSlots>();
                    var configuredSlotStarts = new HashSet<int>(applicableSlots.Select(s => s.SlotStartMin));

                    foreach (var slot in applicableSlots)
                    {
                        // Lookup actual count from aggregation data
                        var aggregationKey = (schedule.ApptResId, currentDate, slot.SlotStartMin);
                        var slotCount = slotAggregation.TryGetValue(aggregationKey, out var count) ? count : 0;

                        var statSlot = new AppointmentStatSlots
                        {
                            ApptStatSlotId = Guid.CreateVersion7(),
                            ApptDate = currentDate,
                            ApptResId = schedule.ApptResId,
                            SlotStart = slot.SlotStartMin,
                            SlotEnd = slot.SlotEndMin,
                            SlotCap = slot.SlotCap,
                            SlotCount = slotCount  // ACCURATE count from aggregation
                        };

                        SeederHelper.InitializeAuditFields(statSlot, dateTimeProvider);
                        slotRecords.Add(statSlot);
                    }

                    statSlots.AddRange(slotRecords);

                    // Check for out-of-hours appointments (not matching any configured slot)
                    // Optimized: Use pre-indexed lookup O(1) instead of full list scan
                    var outOfHoursCount = 0;
                    if (aggregationByResAndDate.TryGetValue((schedule.ApptResId, currentDate), out var dayAggregations))
                    {
                        // 時間外予約のスロットレコードを作成（赤いライン表示用）
                        var outOfHoursSlots = dayAggregations
                            .Where(kvp => !configuredSlotStarts.Contains(kvp.Key))
                            .ToList();

                        foreach (var outOfHoursSlot in outOfHoursSlots)
                        {
                            var slotStartMin = outOfHoursSlot.Key;
                            var slotCount = outOfHoursSlot.Value;
                            outOfHoursCount += slotCount;

                            // 時間外スロットのレコードを追加（SlotCap=0で識別可能）
                            // 昼休み時間（12:00-13:00 = 720-780分）を越えないようにスロット終了時刻を調整
                            const int lunchStartMin = 720;   // 12:00
                            const int lunchEndMin = 780;     // 13:00
                            const int slotDurationMin = 30;  // 30分単位

                            int slotEndMin = slotStartMin + slotDurationMin;

                            // スロットが昼休み時間を越える場合、昼休み開始時刻で打ち切る
                            if (slotStartMin < lunchStartMin && slotEndMin > lunchStartMin)
                            {
                                slotEndMin = lunchStartMin;  // 昼休み開始で終了
                            }
                            // スロットが昼休み時間を越える場合（開始が昼休み中の場合）、昼休み終了時刻で打ち切る
                            else if (slotStartMin >= lunchStartMin && slotStartMin < lunchEndMin && slotEndMin > lunchEndMin)
                            {
                                slotEndMin = lunchEndMin;  // 昼休み終了で終了
                            }

                            var outOfHoursStatSlot = new AppointmentStatSlots
                            {
                                ApptStatSlotId = Guid.CreateVersion7(),
                                ApptDate = currentDate,
                                ApptResId = schedule.ApptResId,
                                SlotStart = slotStartMin,
                                SlotEnd = slotEndMin,
                                SlotCap = 0, // 時間外なので容量は0
                                SlotCount = slotCount
                            };

                            SeederHelper.InitializeAuditFields(outOfHoursStatSlot, dateTimeProvider);
                            statSlots.Add(outOfHoursStatSlot);
                        }
                    }

                    // Aggregate slot records to create AppointmentStats (daily total)
                    var totalCapacity = slotRecords.Sum(s => s.SlotCap);
                    var totalCount = slotRecords.Sum(s => s.SlotCount) + outOfHoursCount;

                    var stat = new AppointmentStats
                    {
                        ApptStatId = Guid.CreateVersion7(),
                        ApptDate = currentDate,
                        ApptResId = schedule.ApptResId,
                        ApptCap = totalCapacity,
                        ApptCount = totalCount  // Includes both slot and out-of-hours appointments
                    };

                    SeederHelper.InitializeAuditFields(stat, dateTimeProvider);
                    stats.Add(stat);
                }

                currentDate = currentDate.AddDays(1);
            }
        }
        processSw.Stop();

        // Use BulkInsertAsync for better performance instead of AddRangeAsync + SaveChangesAsync
        if (stats.Any())
        {
            var statInsertSw = Stopwatch.StartNew();
            await context.BulkInsertAsync(stats, new BulkConfig
            {
                SetOutputIdentity = false,
                BatchSize = 5000,
                PropertiesToExclude = new List<string> { nameof(AppointmentStats.ApptAvailable) }
            });
            statInsertSw.Stop();
            Console.WriteLine($"  [BulkInsert] AppointmentStats: {stats.Count} records - took {statInsertSw.Elapsed.TotalSeconds:F2}s");
        }

        if (statSlots.Any())
        {
            var slotsInsertSw = Stopwatch.StartNew();
            await context.BulkInsertAsync(statSlots, new BulkConfig
            {
                SetOutputIdentity = false,
                BatchSize = 5000,
                PropertiesToExclude = new List<string> { nameof(AppointmentStatSlots.SlotAvailable) }
            });
            slotsInsertSw.Stop();
            Console.WriteLine($"  [BulkInsert] AppointmentStatSlots: {statSlots.Count} records - took {slotsInsertSw.Elapsed.TotalSeconds:F2}s");
        }

        startStopwatch.Stop();
        Console.WriteLine($"  [+] Created {stats.Count} stats records with {statSlots.Count} slot details (from aggregated data) - took {startStopwatch.Elapsed.TotalSeconds:F2}s");
    }

    /// <summary>
    /// 指定日付に適用されるスロットを取得します
    /// オーバーライドを考慮して、実際に使用されるスロットリストを返す
    /// </summary>
    private static List<(int SlotStartMin, int SlotEndMin, int SlotCap)> GetApplicableSlotsForDate(
        AppointmentSchedule schedule,
        DateOnly date)
    {
        var result = new List<(int SlotStartMin, int SlotEndMin, int SlotCap)>();

        // その日のオーバーライドをチェック
        var dateOverride = schedule.AppointmentScheduleOverrides
            .FirstOrDefault(o => o.ApptDate == date && !o.IsDeleted);

        if (dateOverride != null)
        {
            // オーバーライドが存在する場合、スロットオーバーライドをチェック
            var overrideSlots = dateOverride.AppointmentScheduleSlotOverrides
                .Where(s => !s.IsDeleted)
                .ToList();

            if (overrideSlots.Any())
            {
                // フルスロット置換: オーバーライドスロットを使用
                foreach (var slot in overrideSlots)
                {
                    result.Add((slot.SlotStartMin, slot.SlotEndMin, slot.SlotCap));
                }
            }
            else
            {
                // オーバーライドが存在するが、スロットオーバーライドがない場合は、
                // その日は休診日（スロットなし）として扱う
                // resultは空のままで返される
            }
        }
        else
        {
            // 通常営業日: その曜日のスロットを使用
            var dayOfWeek = (int)date.DayOfWeek;
            var daySlots = schedule.AppointmentScheduleSlots
                .Where(s => !s.IsDeleted && s.DaysOfWeek.Contains(dayOfWeek))
                .ToList();

            foreach (var slot in daySlots)
            {
                var slotCap = slot.SlotCap;

                // 容量オーバーライドをチェック
                if (slot.CapOverrides != null && slot.CapOverrides.Any())
                {
                    var capOverride = slot.CapOverrides
                        .FirstOrDefault(co => co.ApptDate == date && !co.IsDeleted);

                    if (capOverride != null)
                    {
                        slotCap = capOverride.SlotCap;
                    }
                }

                result.Add((slot.SlotStartMin, slot.SlotEndMin, slotCap));
            }
        }

        return result;
    }
}
