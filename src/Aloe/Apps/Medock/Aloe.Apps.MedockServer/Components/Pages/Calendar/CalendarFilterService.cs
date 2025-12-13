using Aloe.Apps.MedockServer.Components.Calendar;
using Aloe.Apps.MedockServer.Components.FAB;

namespace Aloe.Apps.MedockServer.Components.Pages;

/// <summary>
/// カレンダーのフィルター処理サービス
/// </summary>
public class CalendarFilterService
{
    public CalendarFilterService()
    {
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

        // 設備条件フィルターは削除されました（AppointmentResourceに統合）
        Dictionary<(DateOnly date, string timeSlot), int>? statsDict = null;

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
