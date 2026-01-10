using Aloe.Apps.MedockLib.Data.Entities;
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
        Dictionary<string, List<AppointmentStats>> mainStats,
        Dictionary<string, List<AppointmentStats>> originalMainStats,
        Dictionary<string, bool> mainStatsGrayedOut,
        CalendarViewType currentView,
        DateOnly currentDate,
        Dictionary<(DateOnly ApptDate, Guid ApptResId), List<AppointmentStatSlots>>? mainStatsSlots = null)
    {
        if (!filter.IsActive)
        {
            this.ResetFilter(mainStatsGrayedOut);
            return;
        }

        foreach (var kvp in mainStats)
        {
            var dateStr = kvp.Key;
            var statsList = kvp.Value;
            var date = DateOnly.Parse(dateStr);

            var isDateGrayed = this.IsDateGrayed(date, filter);
            var hasAvailableSlot = this.ProcessSlots(statsList, filter, date, isDateGrayed, mainStatsSlots);

            mainStatsGrayedOut[dateStr] = isDateGrayed || !hasAvailableSlot;
        }
    }

    /// <summary>
    /// フィルターをリセットする
    /// </summary>
    public void ResetFilter(Dictionary<string, bool> mainStatsGrayedOut)
    {
        foreach (var key in mainStatsGrayedOut.Keys.ToList())
        {
            mainStatsGrayedOut[key] = false;
        }
    }

    private bool IsDateGrayed(DateOnly date, SearchFilterPanel.SearchFilter filter)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        return filter.SelectedDays.Any() && !filter.SelectedDays.Contains(dayOfWeek);
    }

    /// <summary>
    /// 分から"HH:mm"形式の文字列を生成します（表示用）
    /// </summary>
    private static string FormatMinutesToTimeString(int minutes)
    {
        var hours = minutes / 60;
        var mins = minutes % 60;
        return $"{hours:D2}:{mins:D2}";
    }

    /// <summary>
    /// "HH:mm"形式の文字列から分に変換します
    /// </summary>
    private static bool TryParseTimeStringToMinutes(string timeString, out int minutes)
    {
        minutes = 0;
        if (string.IsNullOrWhiteSpace(timeString))
            return false;

        var parts = timeString.Split(':');
        if (parts.Length != 2)
            return false;

        if (!int.TryParse(parts[0], out var hours) || !int.TryParse(parts[1], out var mins))
            return false;

        if (hours < 0 || hours >= 24 || mins < 0 || mins >= 60)
            return false;

        minutes = hours * 60 + mins;
        return true;
    }

    private bool ProcessSlots(
        List<AppointmentStats> statsList,
        SearchFilterPanel.SearchFilter filter,
        DateOnly date,
        bool isDateGrayed,
        Dictionary<(DateOnly ApptDate, Guid ApptResId), List<AppointmentStatSlots>>? mainStatsSlots = null)
    {
        var hasAvailableSlot = false;

        // 全てのMainリソースのAppointmentStatSlotsを取得してスロットを合算
        // 時間範囲をキーとして使用（"HH:mm-HH:mm"形式）で、内部はint（分）で管理
        var slotMap = new Dictionary<string, (int StartMinutes, int EndMinutes, int Count, int Cap)>();

        foreach (var stat in statsList)
        {
            // Get slots for this stat from mainStatsSlots if available
            var slots = new List<AppointmentStatSlots>();
            if (mainStatsSlots != null)
            {
                var key = (stat.ApptDate, stat.ApptResId);
                if (mainStatsSlots.TryGetValue(key, out var foundSlots))
                {
                    slots = foundSlots;
                }
            }

            foreach (var statSlot in slots.Where(s => !s.IsDeleted))
            {
                var slotStartMinutes = statSlot.SlotStartMin;
                var slotEndMinutes = statSlot.SlotEndMin;
                var timeRangeKey = $"{FormatMinutesToTimeString(slotStartMinutes)}-{FormatMinutesToTimeString(slotEndMinutes)}";

                if (slotMap.ContainsKey(timeRangeKey))
                {
                    var existing = slotMap[timeRangeKey];
                    slotMap[timeRangeKey] = (existing.StartMinutes, existing.EndMinutes, existing.Count + statSlot.SlotCount, existing.Cap + statSlot.SlotCap);
                }
                else
                {
                    slotMap[timeRangeKey] = (slotStartMinutes, slotEndMinutes, statSlot.SlotCount, statSlot.SlotCap);
                }
            }
        }

        // スロットごとにフィルターを適用
        foreach (var kvp in slotMap)
        {
            var timeRangeKey = kvp.Key;
            var (startMinutes, endMinutes, count, cap) = kvp.Value;
            var isSlotGrayed = false;

            // 時間スロットフィルター: 時間範囲が選択された時間スロットと一致するかチェック
            if (filter.TimeSlots.Any())
            {
                var matchesTimeSlot = false;
                foreach (var selectedTimeSlot in filter.TimeSlots)
                {
                    // selectedTimeSlotが時間範囲形式（"HH:mm-HH:mm"）の場合
                    if (selectedTimeSlot == timeRangeKey)
                    {
                        matchesTimeSlot = true;
                        break;
                    }

                    // selectedTimeSlotが時刻形式（"HH:mm"）の場合、1時間範囲として比較
                    // 例: "09:00"が選択されている場合、スロットの開始時刻が9時台（9:00〜9:59）ならマッチ
                    if (TryParseTimeStringToMinutes(selectedTimeSlot, out var selectedMinutes))
                    {
                        var selectedHour = selectedMinutes / 60;
                        var slotStartHour = startMinutes / 60;
                        if (slotStartHour == selectedHour)
                        {
                            matchesTimeSlot = true;
                            break;
                        }
                    }
                }
                if (!matchesTimeSlot)
                {
                    isSlotGrayed = true;
                }
            }

            var availableCapacity = cap - count;
            if (filter.RequiredCapacity > 1 && availableCapacity < filter.RequiredCapacity)
                isSlotGrayed = true;

            if (!isSlotGrayed && !isDateGrayed)
                hasAvailableSlot = true;
        }

        return hasAvailableSlot;
    }
}
