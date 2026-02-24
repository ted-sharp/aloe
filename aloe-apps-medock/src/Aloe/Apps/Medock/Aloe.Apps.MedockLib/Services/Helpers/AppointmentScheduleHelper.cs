using Aloe.Apps.MedockLib.Data.Entities;

namespace Aloe.Apps.MedockLib.Services.Helpers;

/// <summary>
/// スケジュール処理に関する共有ヘルパーメソッド
/// AppointmentStatsSeederとAppointmentStatsUpdateServiceで共有される
/// </summary>
public static class AppointmentScheduleHelper
{
    /// <summary>
    /// 指定日付に適用されるスロットを取得します
    /// オーバーライドを考慮して、実際に使用されるスロットリストを返す
    /// </summary>
    public static List<(int SlotStartMin, int SlotEndMin, int SlotCap)> GetApplicableSlotsForDate(
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
