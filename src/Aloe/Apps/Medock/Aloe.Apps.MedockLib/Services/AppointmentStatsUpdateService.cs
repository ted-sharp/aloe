using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Repositories;
using Aloe.Apps.MedockLib.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 予約統計の自動再計算サービス実装
/// 予約の作成・更新・削除時にStats/StatSlotsを再計算する
/// </summary>
public class AppointmentStatsUpdateService : IAppointmentStatsUpdateService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<AppointmentStatsUpdateService> _logger;
    private readonly IAppointmentStatsRepository _statsRepository;

    public AppointmentStatsUpdateService(
        IDateTimeProvider dateTimeProvider,
        ILogger<AppointmentStatsUpdateService> logger,
        IAppointmentStatsRepository statsRepository)
    {
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
        _statsRepository = statsRepository;
    }

    /// <inheritdoc />
    public async Task RecalculateStatsAsync(
        MedockDbContext context,
        List<DateOnly> affectedDates,
        List<Guid> affectedResourceIds)
    {
        if (!affectedDates.Any() || !affectedResourceIds.Any())
        {
            return;
        }

        _logger.LogDebug("Starting stats recalculation for {DateCount} dates and {ResourceCount} resources",
            affectedDates.Count, affectedResourceIds.Count);

        // スケジュール・オーバーライドを取得
        var schedules = await LoadSchedulesAsync(context, affectedResourceIds);
        if (!schedules.Any())
        {
            _logger.LogDebug("No active schedules found for resources");
            return;
        }

        // 影響範囲の予約を取得
        var appointments = await LoadAppointmentsAsync(context, affectedDates, affectedResourceIds);

        // 統計を集計
        var (stats, statSlots) = AggregateStats(
            schedules,
            appointments,
            affectedDates,
            affectedResourceIds);

        // Stats/StatSlotsをUpsert（既存をソフト削除後に新規挿入）
        await _statsRepository.UpsertStatsAndSlotsAsync(
            context,
            affectedDates,
            affectedResourceIds,
            stats,
            statSlots);

        _logger.LogDebug("Stats recalculated: {StatsCount} stats records, {SlotsCount} slot records",
            stats.Count, statSlots.Count);
    }

    /// <summary>
    /// スケジュール・オーバーライド情報を読み込む
    /// </summary>
    private async Task<Dictionary<Guid, AppointmentSchedule>> LoadSchedulesAsync(
        MedockDbContext context,
        List<Guid> resourceIds)
    {
        var schedules = await context.AppointmentSchedules
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.IsActive && resourceIds.Contains(s.ApptResId))
            .Include(s => s.AppointmentScheduleSlots)
                .ThenInclude(slot => slot.CapOverrides)
            .Include(s => s.AppointmentScheduleOverrides)
                .ThenInclude(o => o.AppointmentScheduleSlotOverrides)
            .ToListAsync();

        return schedules.ToDictionary(s => s.ApptResId, s => s);
    }

    /// <summary>
    /// 影響範囲の予約を読み込む
    /// </summary>
    private async Task<Dictionary<(DateOnly ApptDate, Guid ApptResId), List<Appointment>>> LoadAppointmentsAsync(
        MedockDbContext context,
        List<DateOnly> dates,
        List<Guid> resourceIds)
    {
        var appointments = await context.Appointments
            .AsNoTracking()
            .Where(a => !a.IsDeleted &&
                        dates.Contains(a.ApptDate.Value) &&
                        a.AppointmentResourceAssignments.Any(ara =>
                            !ara.IsDeleted && resourceIds.Contains(ara.ApptResId)))
            .Include(a => a.AppointmentResourceAssignments.Where(ara => !ara.IsDeleted))
            .ToListAsync();

        // (Date, ResourceId)でグループ化
        var result = new Dictionary<(DateOnly ApptDate, Guid ApptResId), List<Appointment>>();
        foreach (var appt in appointments)
        {
            if (!appt.ApptDate.HasValue) continue;

            foreach (var assignment in appt.AppointmentResourceAssignments)
            {
                var key = (appt.ApptDate.Value, assignment.ApptResId);
                if (!result.ContainsKey(key))
                {
                    result[key] = new List<Appointment>();
                }
                result[key].Add(appt);
            }
        }

        return result;
    }

    /// <summary>
    /// 予約データから統計を集計
    /// 対象日付・リソース全てについてStats/StatSlotsレコードを生成
    /// </summary>
    private (List<AppointmentStats> stats, List<AppointmentStatSlots> slots) AggregateStats(
        Dictionary<Guid, AppointmentSchedule> schedules,
        Dictionary<(DateOnly ApptDate, Guid ApptResId), List<Appointment>> appointments,
        List<DateOnly> affectedDates,
        List<Guid> affectedResourceIds)
    {
        var stats = new List<AppointmentStats>();
        var statSlots = new List<AppointmentStatSlots>();
        var now = _dateTimeProvider.NowRoundedToSeconds;

        // (Date, ResourceId)の全組み合わせについて集計
        foreach (var date in affectedDates)
        {
            foreach (var resourceId in affectedResourceIds)
            {
                if (!schedules.TryGetValue(resourceId, out var schedule))
                {
                    continue;
                }

                // この日付に適用されるスロットを取得
                var applicableSlots = AppointmentScheduleHelper.GetApplicableSlotsForDate(schedule, date);

                if (applicableSlots.Any())
                {
                    // スロットごとの予約数を集計
                    var slotCounts = new Dictionary<int, int>(); // SlotStartMin -> count
                    var configuredSlotStarts = new HashSet<int>(applicableSlots.Select(s => s.SlotStartMin));

                    // 対象の予約を取得
                    var appointmentsForKey = appointments.TryGetValue((date, resourceId), out var appts)
                        ? appts
                        : new List<Appointment>();

                    // 各予約をスロットにマッチング
                    foreach (var appt in appointmentsForKey)
                    {
                        var matchingSlotStart = FindMatchingSlotStartMin(appt.ApptStartMin, applicableSlots);
                        if (matchingSlotStart.HasValue)
                        {
                            // 通常スロット内
                            if (!slotCounts.ContainsKey(matchingSlotStart.Value))
                            {
                                slotCounts[matchingSlotStart.Value] = 0;
                            }
                            slotCounts[matchingSlotStart.Value]++;
                        }
                        else
                        {
                            // 時間外予約
                            if (!slotCounts.ContainsKey(appt.ApptStartMin))
                            {
                                slotCounts[appt.ApptStartMin] = 0;
                            }
                            slotCounts[appt.ApptStartMin]++;
                        }
                    }

                    // AppointmentStatSlotsレコード作成（通常スロット）
                    var slotRecords = new List<AppointmentStatSlots>();
                    foreach (var slot in applicableSlots)
                    {
                        var count = slotCounts.TryGetValue(slot.SlotStartMin, out var c) ? c : 0;
                        var statSlot = new AppointmentStatSlots
                        {
                            ApptStatSlotId = Guid.CreateVersion7(),
                            ApptDate = date,
                            ApptResId = resourceId,
                            SlotStartMin = slot.SlotStartMin,
                            SlotEndMin = slot.SlotEndMin,
                            SlotCap = slot.SlotCap,
                            SlotCount = count,
                            IsDeleted = false,
                            CreatedAt = now,
                            UpdatedAt = now
                        };
                        slotRecords.Add(statSlot);
                        statSlots.Add(statSlot);
                    }

                    // 時間外スロットレコード作成
                    var outOfHoursCount = 0;
                    var outOfHoursSlotStarts = slotCounts
                        .Where(kvp => !configuredSlotStarts.Contains(kvp.Key))
                        .ToList();

                    foreach (var outOfHoursEntry in outOfHoursSlotStarts)
                    {
                        var slotStartMin = outOfHoursEntry.Key;
                        var count = outOfHoursEntry.Value;
                        outOfHoursCount += count;

                        // 時間外スロットのSlotEndMinを計算（昼休み考慮）
                        var slotEndMin = CalculateOutOfHoursSlotEndMin(slotStartMin);

                        var outOfHoursStatSlot = new AppointmentStatSlots
                        {
                            ApptStatSlotId = Guid.CreateVersion7(),
                            ApptDate = date,
                            ApptResId = resourceId,
                            SlotStartMin = slotStartMin,
                            SlotEndMin = slotEndMin,
                            SlotCap = 0, // 時間外なので容量は0
                            SlotCount = count,
                            IsDeleted = false,
                            CreatedAt = now,
                            UpdatedAt = now
                        };
                        statSlots.Add(outOfHoursStatSlot);
                    }

                    // AppointmentStatsレコード作成（日次集計）
                    var totalCapacity = slotRecords.Sum(s => s.SlotCap);
                    var totalCount = slotRecords.Sum(s => s.SlotCount) + outOfHoursCount;

                    var stat = new AppointmentStats
                    {
                        ApptStatId = Guid.CreateVersion7(),
                        ApptDate = date,
                        ApptResId = resourceId,
                        ApptCap = totalCapacity,
                        ApptCount = totalCount,
                        IsDeleted = false,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    stats.Add(stat);
                }
            }
        }

        return (stats, statSlots);
    }

    /// <summary>
    /// 予約開始時刻が属するスロットの開始時刻を取得
    /// 半開区間 [SlotStartMin, SlotEndMin) でマッチング
    /// </summary>
    private int? FindMatchingSlotStartMin(
        int apptStartMin,
        List<(int SlotStartMin, int SlotEndMin, int SlotCap)> slots)
    {
        foreach (var slot in slots)
        {
            if (apptStartMin >= slot.SlotStartMin && apptStartMin < slot.SlotEndMin)
            {
                return slot.SlotStartMin;
            }
        }
        return null; // 時間外
    }

    /// <summary>
    /// 時間外スロットの終了時刻を計算
    /// 昼休み時間（12:00-13:00 = 720-780分）を越えないようにする
    /// </summary>
    private int CalculateOutOfHoursSlotEndMin(int slotStartMin)
    {
        const int lunchStartMin = 720;   // 12:00
        const int lunchEndMin = 780;     // 13:00
        const int slotDurationMin = 30;  // 30分単位

        int slotEndMin = slotStartMin + slotDurationMin;

        // スロットが昼休みの前から入ってくる場合、昼休み開始で打ち切る
        if (slotStartMin < lunchStartMin && slotEndMin > lunchStartMin)
        {
            slotEndMin = lunchStartMin;
        }
        // スロットが昼休みを越える場合（開始が昼休み中の場合）、昼休み終了で打ち切る
        else if (slotStartMin >= lunchStartMin && slotStartMin < lunchEndMin && slotEndMin > lunchEndMin)
        {
            slotEndMin = lunchEndMin;
        }

        return slotEndMin;
    }
}
