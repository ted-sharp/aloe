using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockServer.Components.Calendar;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aloe.Apps.MedockServer.Components.Pages;

/// <summary>
/// カレンダー表示用のサンプルデータ生成クラス（開発用）
/// </summary>
public static class SampleDataGenerator
{
    /// <summary>
    /// グラフスロットアイテム（JSON生成用）
    /// </summary>
    private class GraphSlotItem
    {
        [JsonPropertyName("time")]
        public string Time { get; set; } = String.Empty;

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("max")]
        public int Max { get; set; }
    }

    /// <summary>
    /// Mainリソース統計サンプルデータを生成
    /// 注意: このメソッドは開発用です。実際のエンティティ作成にはリソースIDなどが必要です。
    /// </summary>
    public static void GenerateMainStats(
        Dictionary<string, List<AppointmentStats>> mainStats,
        Dictionary<string, List<AppointmentStats>> originalMainStats,
        Dictionary<string, bool> mainStatsGrayedOut,
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

            var statsList = GenerateAppointmentStatsList(
                random, slotTimes, slotMaxes, isWeekend, DateOnly.FromDateTime(date));

            mainStats[dateStr] = statsList;
            originalMainStats[dateStr] = statsList.Select(s => new AppointmentStats
            {
                ApptStatId = s.ApptStatId,
                ApptDate = s.ApptDate,
                ApptResId = s.ApptResId,
                ApptCap = s.ApptCap,
                ApptCount = s.ApptCount,
                ApptAvailable = s.ApptAvailable,
                ApptGraph = s.ApptGraph,
                IsDeleted = s.IsDeleted,
                CreatedAt = s.CreatedAt,
                CreatedUserId = s.CreatedUserId,
                CreatedSessionId = s.CreatedSessionId,
                UpdatedAt = s.UpdatedAt,
                UpdatedUserId = s.UpdatedUserId,
                UpdatedSessionId = s.UpdatedSessionId
            }).ToList();
            mainStatsGrayedOut[dateStr] = false;
        }
    }

    /// <summary>
    /// 予約サンプルデータを生成（開発・テスト用）
    /// </summary>
    public static List<AppointmentDto> GenerateAppointments(BusinessHoursDto? businessHours)
    {
        var random = new Random(42);
        var today = DateTime.Today;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var hours = businessHours ?? new BusinessHoursDto();

        var names = new[] { "山田 太郎", "佐藤 花子", "鈴木 一郎", "田中 美咲", "高橋 健二" };
        var orgs = new[] { "株式会社ABC", "XYZ商事", "個人", "医療法人DEF", "○○健保組合" };

        var appointments = new List<AppointmentDto>();

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

                appointments.Add(new AppointmentDto
                {
                    Id = Guid.CreateVersion7(),
                    Date = DateOnly.FromDateTime(date),
                    StartTime = new TimeOnly(startHour, 0),
                    EndTime = new TimeOnly(endHour, 0),
                    PatientName = names[random.Next(names.Length)],
                    OrganizationName = orgs[random.Next(orgs.Length)],
                    Status = random.Next(0, 4),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
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

    private static List<AppointmentStats> GenerateAppointmentStatsList(
        Random random,
        List<string> slotTimes,
        int[] slotMaxes,
        bool isWeekend,
        DateOnly date)
    {
        var slots = new List<GraphSlotItem>();
        var totalCount = 0;
        var totalMax = 0;

        for (var i = 0; i < slotTimes.Count; i++)
        {
            var max = isWeekend ? Math.Max(1, slotMaxes[i] / 2) : slotMaxes[i];
            var count = random.Next(0, max + 1);

            slots.Add(new GraphSlotItem
            {
                Time = slotTimes[i],
                Count = count,
                Max = max
            });

            totalCount += count;
            totalMax += max;
        }

        // JSON形式のグラフデータを生成
        var graphData = new
        {
            slots = slots
        };
        var apptGraph = JsonSerializer.Serialize(graphData);

        // サンプル用のAppointmentStatsを作成（リソースIDはダミー）
        var stats = new AppointmentStats
        {
            ApptStatId = Guid.CreateVersion7(),
            ApptDate = date,
            ApptResId = Guid.Empty, // ダミーID
            ApptCap = totalMax,
            ApptCount = totalCount,
            ApptAvailable = totalMax - totalCount,
            ApptGraph = apptGraph,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedUserId = Guid.Empty,
            CreatedSessionId = Guid.Empty,
            UpdatedAt = DateTime.UtcNow,
            UpdatedUserId = Guid.Empty,
            UpdatedSessionId = Guid.Empty
        };

        return new List<AppointmentStats> { stats };
    }
}
