using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentStatsSeeder
{
    public static async Task SeedAsync(MedockDbContext context, IDbContextFactory<MedockDbContext> contextFactory, IDateTimeProvider dateTimeProvider)
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

        // SQL で予約統計を事前集計（メモリ効率化）
        var appointmentCounts = await context.AppointmentResourceAssignments
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.Appointment.ApptDate >= startDate && a.Appointment.ApptDate <= endDate && !a.Appointment.IsDeleted)
            .GroupBy(a => new { a.ApptResId, a.Appointment.ApptDate })
            .Select(g => new { ResId = g.Key.ApptResId, Date = g.Key.ApptDate, Count = g.Count() })
            .ToListAsync();

        // 祝日を取得
        var holidays = await SeederHelper.LoadHolidaySetAsync(context);

        // 予約カウントを辞書に変換（高速参照用）
        var appointmentCountDict = appointmentCounts.ToDictionary(x => (x.ResId, x.Date), x => x.Count);

        var stats = new List<AppointmentStats>();
        var statSlots = new List<AppointmentStatSlots>();

        // 各スケジュール（リソース）に対してStats を作成
        foreach (var schedule in schedules)
        {
            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                // その日に適用されるスロットを取得
                var applicableSlots = GetApplicableSlotsForDate(schedule, currentDate);

                if (applicableSlots.Any())
                {
                    // AppointmentStats レコードを作成
                    var totalCapacity = applicableSlots.Sum(s => s.SlotCap);

                    // 当日のこのリソースの予約数を取得（SQL事前集計済み）
                    var dayCount = appointmentCountDict.TryGetValue((schedule.ApptResId, currentDate), out var count) ? count : 0;

                    var stat = new AppointmentStats
                    {
                        ApptStatId = Guid.CreateVersion7(),
                        ApptDate = currentDate,
                        ApptResId = schedule.ApptResId,
                        ApptCap = totalCapacity,
                        ApptCount = dayCount
                    };

                    SeederHelper.InitializeAuditFields(stat, dateTimeProvider);
                    stats.Add(stat);

                    // 各スロットに対して AppointmentStatSlots レコードを作成
                    foreach (var slot in applicableSlots)
                    {
                        // NOTE: スロット単位のカウントは、より詳細な計算が必要なため、
                        // 簡易的に日全体のカウントを等分配する（正確な時間マッチングはスキップ）
                        var slotCount = dayCount > 0 && applicableSlots.Count > 0
                            ? dayCount / applicableSlots.Count
                            : 0;

                        var statSlot = new AppointmentStatSlots
                        {
                            ApptStatSlotId = Guid.CreateVersion7(),
                            ApptDate = currentDate,
                            ApptResId = schedule.ApptResId,
                            SlotStart = slot.SlotStartMin,
                            SlotEnd = slot.SlotEndMin,
                            SlotCap = slot.SlotCap,
                            SlotCount = slotCount
                        };

                        SeederHelper.InitializeAuditFields(statSlot, dateTimeProvider);
                        statSlots.Add(statSlot);
                    }
                }

                currentDate = currentDate.AddDays(1);
            }
        }

        // データベースに保存
        if (stats.Any())
        {
            await context.AppointmentStats.AddRangeAsync(stats);
        }

        if (statSlots.Any())
        {
            await context.AppointmentStatSlots.AddRangeAsync(statSlots);
        }

        if (stats.Any() || statSlots.Any())
        {
            await context.SaveChangesAsync();
        }

        startStopwatch.Stop();
        Console.WriteLine($"  [+] Created {stats.Count} stats records with {statSlots.Count} slot details - took {startStopwatch.Elapsed.TotalSeconds:F2}s");
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
