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
        DateOnly currentDate)
    {
        if (!filter.IsActive)
        {
            this.ResetFilter(mainStatsGrayedOut);
            return;
        }

        // 設備条件フィルターは削除されました（AppointmentResourceに統合）
        Dictionary<(DateOnly date, string timeSlot), int>? statsDict = null;

        foreach (var kvp in mainStats)
        {
            var dateStr = kvp.Key;
            var statsList = kvp.Value;
            var date = DateOnly.Parse(dateStr);

            var isDateGrayed = this.IsDateGrayed(date, filter);
            var hasAvailableSlot = this.ProcessSlots(statsList, filter, statsDict, date, isDateGrayed);

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

    private bool ProcessSlots(
        List<AppointmentStats> statsList,
        SearchFilterPanel.SearchFilter filter,
        Dictionary<(DateOnly date, string timeSlot), int>? statsDict,
        DateOnly date,
        bool isDateGrayed)
    {
        var hasAvailableSlot = false;

        // 全てのMainリソースのAppointmentStatSlotsを取得してスロットを合算
        // 時間範囲をキーとして使用（"HH:mm-HH:mm"形式）
        var slotMap = new Dictionary<string, (TimeOnly Start, TimeOnly End, int Count, int Cap)>();

        foreach (var stat in statsList)
        {
            if (stat.AppointmentStatSlots != null)
            {
                foreach (var statSlot in stat.AppointmentStatSlots.Where(s => !s.IsDeleted))
                {
                    var slotStartTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(statSlot.SlotStart));
                    var slotEndTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(statSlot.SlotEnd));
                    var timeRangeKey = $"{slotStartTime:HH:mm}-{slotEndTime:HH:mm}";
                    
                    if (slotMap.ContainsKey(timeRangeKey))
                    {
                        var existing = slotMap[timeRangeKey];
                        slotMap[timeRangeKey] = (existing.Start, existing.End, existing.Count + statSlot.SlotCount, existing.Cap + statSlot.SlotCap);
                    }
                    else
                    {
                        slotMap[timeRangeKey] = (slotStartTime, slotEndTime, statSlot.SlotCount, statSlot.SlotCap);
                    }
                }
            }
        }

        // スロットごとにフィルターを適用
        foreach (var kvp in slotMap)
        {
            var timeRangeKey = kvp.Key;
            var (start, end, count, cap) = kvp.Value;
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
                    if (TimeOnly.TryParse(selectedTimeSlot, out var selectedTime))
                    {
                        if (start.Hour == selectedTime.Hour)
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

            // 設備条件フィルターのカウントを辞書から取得
            if (statsDict != null)
            {
                var key = (date, timeRangeKey);
                // statsDictから取得したカウントは使用しない（将来の拡張用）
            }

            if (!isSlotGrayed && !isDateGrayed)
                hasAvailableSlot = true;
        }

        return hasAvailableSlot;
    }
}
