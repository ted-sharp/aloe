using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockServer.Components.Calendar;

namespace Aloe.Apps.MedockServer.Components.Pages;

/// <summary>
/// カレンダー表示用のサンプルデータ生成クラス（開発用）
/// </summary>
public static class SampleDataGenerator
{
    /// <summary>
    /// 日別統計サンプルデータを生成
    /// </summary>
    public static void GenerateDayStats(
        Dictionary<string, CalendarDayStats> dayStats,
        Dictionary<string, CalendarDayStats> originalDayStats,
        BusinessHoursDto? businessHours)
    {
        var random = new Random(42);
        var today = DateTime.Today;
        var startDate = new DateTime(today.Year, 1, 1);
        var endDate = new DateTime(today.Year, 12, 31);

        // 営業時間からslotTimesを生成
        var hours = businessHours ?? new BusinessHoursDto();
        var startTime = hours.GetStartTimeOnly();
        var endTime = hours.GetEndTimeOnly();
        var lunchStartTime = hours.GetLunchStartTimeOnly();
        var lunchEndTime = hours.GetLunchEndTimeOnly();

        // 営業時間内の時間スロットを生成（1時間単位）
        var slotTimes = GenerateSlotTimes(startTime, endTime, lunchStartTime, lunchEndTime);

        // slotTimesが空の場合はデフォルト値を使用
        if (slotTimes.Count == 0)
        {
            slotTimes = ["09:00", "10:00", "11:00", "13:00", "14:00", "15:00", "16:00", "17:00"];
        }

        // slotMaxesはslotTimesの数に合わせて生成（デフォルト8）
        var slotMaxes = slotTimes.Select(_ => 8).ToArray();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            var dayOfWeek = date.DayOfWeek;
            var isWeekend = dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;

            var (stats, originalStats) = GenerateDayStatsPair(
                random, slotTimes, slotMaxes, isWeekend, hours);

            dayStats[dateStr] = stats;
            originalDayStats[dateStr] = originalStats;
        }
    }

    /// <summary>
    /// 予約サンプルデータを生成
    /// </summary>
    public static List<CalendarAppointment> GenerateAppointments(BusinessHoursDto? businessHours)
    {
        var random = new Random(42);
        var today = DateTime.Today;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var hours = businessHours ?? new BusinessHoursDto();

        var names = new[] { "山田 太郎", "佐藤 花子", "鈴木 一郎", "田中 美咲", "高橋 健二" };
        var orgs = new[] { "株式会社ABC", "XYZ商事", "個人", "医療法人DEF", "○○健保組合" };

        var appointments = new List<CalendarAppointment>();

        for (var day = 0; day < 7; day++)
        {
            var date = weekStart.AddDays(day);
            var count = random.Next(3, 8);

            for (var i = 0; i < count; i++)
            {
                // 営業時間内でランダムに開始時間を選択
                var startHour = random.Next(hours.StartHour, hours.EndHour);
                var duration = random.Next(1, 3);
                var endHour = Math.Min(startHour + duration, hours.EndHour);

                appointments.Add(new CalendarAppointment
                {
                    Id = Guid.CreateVersion7(),
                    Date = DateOnly.FromDateTime(date),
                    StartTime = new TimeOnly(startHour, 0),
                    EndTime = new TimeOnly(endHour, 0),
                    PatientName = names[random.Next(names.Length)],
                    OrganizationName = orgs[random.Next(orgs.Length)],
                    Status = random.Next(0, 4)
                });
            }
        }

        return appointments;
    }

    private static List<string> GenerateSlotTimes(
        TimeOnly startTime,
        TimeOnly endTime,
        TimeOnly lunchStartTime,
        TimeOnly lunchEndTime)
    {
        var slotTimes = new List<string>();
        var currentTime = startTime;

        while (currentTime < endTime)
        {
            // 昼休み時間をスキップ
            if (currentTime >= lunchStartTime && currentTime < lunchEndTime)
            {
                currentTime = currentTime.AddHours(1);
                continue;
            }

            slotTimes.Add(currentTime.ToString("HH:mm"));
            currentTime = currentTime.AddHours(1);
        }

        return slotTimes;
    }

    private static (CalendarDayStats stats, CalendarDayStats originalStats) GenerateDayStatsPair(
        Random random,
        List<string> slotTimes,
        int[] slotMaxes,
        bool isWeekend,
        BusinessHoursDto businessHours)
    {
        var slots = new List<TimeSlotStats>();
        var amCount = 0;
        var pmCount = 0;
        var amMax = 0;
        var pmMax = 0;

        var lunchStart = businessHours.GetLunchStartTimeOnly();
        var lunchEnd = businessHours.GetLunchEndTimeOnly();

        for (var i = 0; i < slotTimes.Count; i++)
        {
            var max = isWeekend ? Math.Max(1, slotMaxes[i] / 2) : slotMaxes[i];
            var count = random.Next(0, max + 1);

            slots.Add(new TimeSlotStats
            {
                Time = slotTimes[i],
                Count = count,
                Max = max,
                IsGrayedOut = false
            });

            // 営業時間に基づいてAM/PMを判定
            var slotTime = TimeOnly.Parse(slotTimes[i]);

            if (slotTime < lunchStart)
            {
                amCount += count;
                amMax += max;
            }
            else if (slotTime >= lunchEnd)
            {
                pmCount += count;
                pmMax += max;
            }
            else
            {
                // 昼休み時間内はAMにカウント
                amCount += count;
                amMax += max;
            }
        }

        var dayStats = new CalendarDayStats
        {
            AmCount = amCount,
            PmCount = pmCount,
            AmMax = amMax,
            PmMax = pmMax,
            Slots = slots,
            IsGrayedOut = false
        };

        var originalStats = new CalendarDayStats
        {
            AmCount = amCount,
            PmCount = pmCount,
            AmMax = amMax,
            PmMax = pmMax,
            Slots = slots.Select(s => new TimeSlotStats
            {
                Time = s.Time,
                Count = s.Count,
                Max = s.Max,
                IsGrayedOut = false
            }).ToList(),
            IsGrayedOut = false
        };

        return (dayStats, originalStats);
    }
}
