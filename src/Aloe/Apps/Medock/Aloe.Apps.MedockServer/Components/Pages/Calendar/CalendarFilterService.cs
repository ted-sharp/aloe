using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockServer.Components.Calendar;
using Aloe.Apps.MedockServer.Components.FAB;

namespace Aloe.Apps.MedockServer.Components.Pages;

/// <summary>
/// カレンダーのフィルター処理サービス
/// </summary>
public class CalendarFilterService
{
    private readonly IEquipmentService _equipmentService;

    public CalendarFilterService(IEquipmentService equipmentService)
    {
        this._equipmentService = equipmentService;
    }

    /// <summary>
    /// フィルターをカレンダー統計に適用する
    /// </summary>
    public async Task ApplyFilterAsync(
        SearchFilterPanel.SearchFilter filter,
        Dictionary<string, CalendarDayStats> dayStats,
        Dictionary<string, CalendarDayStats> originalDayStats,
        CalendarViewType currentView,
        DateOnly currentDate)
    {
        if (!filter.IsActive)
        {
            this.ResetFilter(dayStats, originalDayStats);
            return;
        }

        // 設備条件フィルターが選択されている場合、期間全体のデータを一括取得
        Dictionary<(DateOnly date, string timeSlot), int>? statsDict = null;
        if (filter.EquipIds.Any())
        {
            statsDict = await this.LoadEquipmentStatsAsync(filter.EquipIds, currentView, currentDate);
        }

        foreach (var kvp in dayStats)
        {
            var dateStr = kvp.Key;
            var stats = kvp.Value;
            var date = DateOnly.Parse(dateStr);

            var isDateGrayed = this.IsDateGrayed(date, filter);
            var hasAvailableSlot = this.ProcessSlots(stats, filter, statsDict, date, isDateGrayed);

            stats.IsGrayedOut = isDateGrayed || !hasAvailableSlot;
        }
    }

    /// <summary>
    /// フィルターをリセットする
    /// </summary>
    public void ResetFilter(
        Dictionary<string, CalendarDayStats> dayStats,
        Dictionary<string, CalendarDayStats> originalDayStats)
    {
        foreach (var kvp in originalDayStats)
        {
            if (dayStats.TryGetValue(kvp.Key, out var stats))
            {
                stats.IsGrayedOut = false;
                if (stats.Slots != null)
                {
                    foreach (var slot in stats.Slots)
                    {
                        slot.IsGrayedOut = false;
                        slot.FilteredCount = 0;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 設備統計を取得する
    /// </summary>
    public async Task<int> CalculateEquipmentFilteredCountAsync(DateOnly date, string timeSlot, List<Guid> equipIds)
    {
        try
        {
            var stats = await this._equipmentService.GetEquipmentStatsAsync(equipIds, date);

            return stats.Sum(s => s.ApptGraph.Slots
                .Where(slot => slot.Time == timeSlot)
                .Sum(slot => slot.Count));
        }
        catch
        {
            return 0;
        }
    }

    private async Task<Dictionary<(DateOnly date, string timeSlot), int>> LoadEquipmentStatsAsync(
        List<Guid> equipIds,
        CalendarViewType currentView,
        DateOnly currentDate)
    {
        // 表示期間を決定
        var startDate = currentView == CalendarViewType.Year
            ? new DateOnly(currentDate.Year, 1, 1)
            : new DateOnly(currentDate.Year, currentDate.Month, 1);
        var endDate = currentView == CalendarViewType.Year
            ? new DateOnly(currentDate.Year, 12, 31)
            : new DateOnly(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));

        // 期間内の設備統計を一括取得
        var equipmentStats = await this._equipmentService.GetEquipmentStatsByDateRangeAsync(
            equipIds, startDate, endDate);

        // 辞書に変換: (date, timeSlot) -> count (複数設備の合計)
        return equipmentStats
            .SelectMany(s => s.ApptGraph.Slots.Select(slot => new
            {
                Key = (s.ApptDate, slot.Time),
                Count = slot.Count
            }))
            .GroupBy(x => x.Key)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));
    }

    private bool IsDateGrayed(DateOnly date, SearchFilterPanel.SearchFilter filter)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        return filter.SelectedDays.Any() && !filter.SelectedDays.Contains(dayOfWeek);
    }

    private bool ProcessSlots(
        CalendarDayStats stats,
        SearchFilterPanel.SearchFilter filter,
        Dictionary<(DateOnly date, string timeSlot), int>? statsDict,
        DateOnly date,
        bool isDateGrayed)
    {
        var hasAvailableSlot = false;

        if (stats.Slots != null)
        {
            foreach (var slot in stats.Slots)
            {
                var isSlotGrayed = false;

                if (filter.TimeSlots.Any() && !filter.TimeSlots.Contains(slot.Time))
                    isSlotGrayed = true;

                var availableCapacity = slot.Max - slot.Count;
                if (filter.RequiredCapacity > 1 && availableCapacity < filter.RequiredCapacity)
                    isSlotGrayed = true;

                // 設備条件フィルターのカウントを辞書から取得
                if (statsDict != null)
                {
                    var key = (date, slot.Time);
                    slot.FilteredCount = statsDict.TryGetValue(key, out var count) ? count : 0;
                }
                else
                {
                    slot.FilteredCount = 0;
                }

                slot.IsGrayedOut = isSlotGrayed || isDateGrayed;

                if (!slot.IsGrayedOut)
                    hasAvailableSlot = true;
            }
        }
        else
        {
            var amAvailable = stats.AmMax - stats.AmCount;
            var pmAvailable = stats.PmMax - stats.PmCount;
            hasAvailableSlot = amAvailable >= filter.RequiredCapacity || pmAvailable >= filter.RequiredCapacity;
        }

        return hasAvailableSlot;
    }
}
