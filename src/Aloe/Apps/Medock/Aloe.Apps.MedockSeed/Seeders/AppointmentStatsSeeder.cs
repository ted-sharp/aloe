using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentStatsSeeder
{

    /// <summary>
    /// グラフデータのJSONB構造
    /// </summary>
    private class GraphDefinition
    {
        [JsonPropertyName("slots")]
        public List<GraphSlotItem> Slots { get; set; } = new();
    }

    /// <summary>
    /// グラフスロットアイテム
    /// </summary>
    private class GraphSlotItem
    {
        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("max")]
        public int Max { get; set; }
    }

    public static async Task SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
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

        // 必要なデータを取得
        var appointments = await context.Appointments
            .Where(a => !a.IsDeleted && a.ApptDate.HasValue)
            .ToListAsync();

        if (!appointments.Any())
        {
            Console.WriteLine("[SKIP] AppointmentStats: No appointments found.");
            return;
        }

        var resourceAssignments = await context.AppointmentResourceAssignments
            .Where(ara => !ara.IsDeleted)
            .ToListAsync();

        if (!resourceAssignments.Any())
        {
            Console.WriteLine("[SKIP] AppointmentStats: No appointment resource assignments found.");
            return;
        }

        var slots = await context.AppointmentSlots
            .Where(s => !s.IsDeleted && s.IsActive)
            .ToListAsync();

        if (!slots.Any())
        {
            Console.WriteLine("[SKIP] AppointmentStats: No appointment slots found.");
            return;
        }

        var slotOverrides = await context.AppointmentSlotOverrides
            .Where(so => !so.IsDeleted)
            .ToListAsync();

        var resources = await context.AppointmentResources
            .Where(r => !r.IsDeleted)
            .ToListAsync();

        Console.WriteLine("[INFO] Creating appointment stats seed data...");

        // 日付・リソースごとにグループ化
        var statsMap = new Dictionary<(DateOnly Date, Guid ResourceId), AppointmentStatsData>();

        // 1. 予約数を集計（appointment_resource_assignmentsとappointmentsをJOIN）
        var appointmentDict = appointments.ToDictionary(a => a.ApptId);
        var assignmentGroups = resourceAssignments
            .Where(ara => appointmentDict.ContainsKey(ara.ApptId))
            .GroupBy(ara => new
            {
                Date = appointmentDict[ara.ApptId].ApptDate!.Value,
                ResourceId = ara.ApptResId
            })
            .ToList();

        foreach (var group in assignmentGroups)
        {
            var key = (group.Key.Date, group.Key.ResourceId);
            if (!statsMap.ContainsKey(key))
            {
                statsMap[key] = new AppointmentStatsData
                {
                    Date = group.Key.Date,
                    ResourceId = group.Key.ResourceId,
                    AppointmentCount = 0,
                    TimeSlotCounts = new Dictionary<string, int>()
                };
            }

            // 予約数をカウント
            var appointmentsInGroup = group
                .Select(ara => appointmentDict[ara.ApptId])
                .Where(a => a.ApptDate.HasValue)
                .ToList();

            statsMap[key].AppointmentCount = appointmentsInGroup.Count;

            // 時間帯ごとの予約数を集計
            // リソースのタイプを取得して、AM/PM形式か時間スロット形式かを判定
            var resource = resources.FirstOrDefault(r => r.ApptResId == group.Key.ResourceId);
            foreach (var appointment in appointmentsInGroup)
            {
                if (appointment.ApptStartTime.HasValue)
                {
                    string timeKey;
                    // AM/PM形式のリソース（エコー、ロッカー）の場合
                    if (resource != null && (resource.ApptResTypeCode == 2 || resource.ApptResTypeCode == 5))
                    {
                        var hour = appointment.ApptStartTime.Value.Hour;
                        timeKey = (hour >= 8 && hour < 13) ? "AM" : "PM";
                    }
                    else
                    {
                        // 時間スロット形式（内視鏡、CT、MR）の場合
                        timeKey = appointment.ApptStartTime.Value.ToString("HH:mm");
                    }

                    if (!statsMap[key].TimeSlotCounts.ContainsKey(timeKey))
                    {
                        statsMap[key].TimeSlotCounts[timeKey] = 0;
                    }
                    statsMap[key].TimeSlotCounts[timeKey]++;
                }
            }
        }

        // 2. キャパシティとグラフデータを計算（appointment_slotsから）
        var (startDate, endDate) = SeederHelper.GetDefaultDateRange(dateTimeProvider);
        var slotDict = slots.ToDictionary(s => s.ApptResId);
        var overrideDict = slotOverrides
            .GroupBy(so => (so.ApptDate, so.ApptResId))
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var resource in resources)
        {
            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                var key = (currentDate, resource.ApptResId);

                // スロット定義を取得（overrideがあれば優先）
                AppointmentSlotRoot? slotDef = null;
                if (overrideDict.TryGetValue((currentDate, resource.ApptResId), out var slotOverride))
                {
                    slotDef = slotOverride.ApptSlotsData;
                }
                else if (slotDict.TryGetValue(resource.ApptResId, out var slot))
                {
                    // 有効期間内かチェック
                    if (slot.ActiveFrom <= currentDate && currentDate <= slot.ActiveTo)
                    {
                        slotDef = slot.ApptSlotsData;
                    }
                }

                if (slotDef != null && slotDef.Slots.Any())
                {
                    if (!statsMap.ContainsKey(key))
                    {
                        statsMap[key] = new AppointmentStatsData
                        {
                            Date = currentDate,
                            ResourceId = resource.ApptResId,
                            AppointmentCount = 0,
                            TimeSlotCounts = new Dictionary<string, int>()
                        };
                    }

                    var statsData = statsMap[key];

                    // キャパシティを計算（各スロットのmax値を合計）
                    statsData.Capacity = slotDef.Slots.Sum(s => s.Max);

                    // グラフデータを生成
                    var graphSlots = new List<GraphSlotItem>();
                    foreach (var slotItem in slotDef.Slots)
                    {
                        var count = statsData.TimeSlotCounts.TryGetValue(slotItem.Time, out var c) ? c : 0;
                        graphSlots.Add(new GraphSlotItem
                        {
                            Time = slotItem.Time,
                            Count = count,
                            Max = slotItem.Max
                        });
                    }

                    statsData.GraphData = graphSlots;
                }

                currentDate = currentDate.AddDays(1);
            }
        }

        // 3. AppointmentStatsエンティティを作成
        var statsList = new List<AppointmentStats>();
        foreach (var (key, statsData) in statsMap)
        {
            var graphJson = JsonSerializer.Serialize(new GraphDefinition
            {
                Slots = statsData.GraphData
            });

            var stat = new AppointmentStats
            {
                ApptStatId = Guid.CreateVersion7(),
                ApptDate = statsData.Date,
                ApptResId = statsData.ResourceId,
                ApptCap = statsData.Capacity,
                ApptCount = statsData.AppointmentCount,
                ApptGraph = graphJson,
                IsDeleted = false
            };

            SeederHelper.InitializeAuditFields(stat, dateTimeProvider);
            statsList.Add(stat);
        }

        context.AppointmentStats.AddRange(statsList);
        Console.WriteLine($"  [+] AppointmentStats: {statsList.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }


    /// <summary>
    /// 統計データの一時保持用クラス
    /// </summary>
    private class AppointmentStatsData
    {
        public DateOnly Date { get; set; }
        public Guid ResourceId { get; set; }
        public int AppointmentCount { get; set; }
        public int Capacity { get; set; }
        public Dictionary<string, int> TimeSlotCounts { get; set; } = new();
        public List<GraphSlotItem> GraphData { get; set; } = new();
    }
}

