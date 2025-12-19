using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Data.Entities;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aloe.Apps.MedockServer.Components.Calendar;

/// <summary>
/// カレンダーキャンバスのJSInterop関連処理とデータ変換
/// </summary>
public static class CalendarCanvasInterop
{
    /// <summary>
    /// グラフデータのJSONB構造（パース用）
    /// </summary>
    private class GraphDefinition
    {
        [JsonPropertyName("slots")]
        public List<GraphSlotItem> Slots { get; set; } = new();
    }

    /// <summary>
    /// グラフスロットアイテム（パース用）
    /// </summary>
    private class GraphSlotItem
    {
        [JsonPropertyName("start")]
        public TimeOnly Start { get; set; }

        [JsonPropertyName("end")]
        public TimeOnly End { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("cap")]
        public int Cap { get; set; }

        [JsonPropertyName("available")]
        public int Available { get; set; }
    }

    /// <summary>
    /// カレンダーデータをJavaScript用のオブジェクトに変換します。
    /// </summary>
    public static object BuildDataObject(
        IEnumerable<AppointmentDto>? appointments,
        Dictionary<string, List<AppointmentStats>>? mainStats,
        Dictionary<string, bool>? mainStatsGrayedOut,
        Dictionary<string, string>? holidays)
    {
        var appointmentArray = appointments?.Select(a => new
        {
            id = a.Id.ToString(),
            date = a.Date.ToString("yyyy-MM-dd"),
            startTime = a.StartTime?.ToString("HH:mm") ?? "09:00",
            endTime = a.EndTime?.ToString("HH:mm") ?? "10:00",
            patientName = a.PatientName,
            organizationName = a.OrganizationName,
            status = a.Status,
            // 将来的に使用可能な追加プロパティ
            patientId = a.PatientId,
            organizationId = a.OrganizationId,
            floorName = a.FloorName,
            floorId = a.FloorId
        }).ToArray() ?? Array.Empty<object>();

        var mainStatsDict = new Dictionary<string, object>();
        if (mainStats != null)
        {
            foreach (var kvp in mainStats)
            {
                var dateStr = kvp.Key;
                var statsList = kvp.Value;

                // 全てのMainリソースのApptGraphをパースしてスロットを合算
                // 時間範囲をキーとして使用（"HH:mm-HH:mm"形式）
                var slotMap = new Dictionary<string, (TimeOnly Start, TimeOnly End, int Count, int Cap)>();

                foreach (var stat in statsList)
                {
                    try
                    {
                        var graphData = JsonSerializer.Deserialize<GraphDefinition>(stat.ApptGraph);
                        if (graphData?.Slots != null)
                        {
                            foreach (var slot in graphData.Slots)
                            {
                                var timeRangeKey = $"{slot.Start:HH:mm}-{slot.End:HH:mm}";
                                if (slotMap.ContainsKey(timeRangeKey))
                                {
                                    var existing = slotMap[timeRangeKey];
                                    slotMap[timeRangeKey] = (existing.Start, existing.End, existing.Count + slot.Count, existing.Cap + slot.Cap);
                                }
                                else
                                {
                                    slotMap[timeRangeKey] = (slot.Start, slot.End, slot.Count, slot.Cap);
                                }
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // JSONパースエラーは無視して続行
                    }
                }

                // スロット情報を配列に変換
                var slots = slotMap.Select(kvp => new
                {
                    start = kvp.Value.Start.ToString("HH:mm"),
                    end = kvp.Value.End.ToString("HH:mm"),
                    count = kvp.Value.Count,
                    cap = kvp.Value.Cap,
                    available = kvp.Value.Cap - kvp.Value.Count,
                    isGrayedOut = false, // スロットごとのグレーアウトは後で実装可能
                    filteredCount = 0
                }).OrderBy(s => s.start).ToArray();

                var isGrayedOut = mainStatsGrayedOut?.TryGetValue(dateStr, out var grayed) == true && grayed;

                mainStatsDict[dateStr] = new
                {
                    slots = slots,
                    isGrayedOut = isGrayedOut
                };
            }
        }

        var holidaysDict = holidays ?? new Dictionary<string, string>();

        return new
        {
            appointments = appointmentArray,
            mainStats = mainStatsDict,
            holidays = holidaysDict
        };
    }

    /// <summary>
    /// カレンダーオプションを構築します。
    /// </summary>
    public static object BuildOptions(
        int weekDays,
        bool showSlots,
        bool showSimpleView,
        int startHour,
        int endHour,
        BusinessHoursDto? businessHours)
    {
        var businessHoursData = businessHours != null
            ? new
            {
                startTime = businessHours.StartTime,
                endTime = businessHours.EndTime,
                lunchStartTime = businessHours.LunchStartTime,
                lunchEndTime = businessHours.LunchEndTime
            }
            : null;

        return new
        {
            weekDays = weekDays,
            showSlots = showSlots,
            showSimpleView = showSimpleView,
            startHour = startHour,
            endHour = endHour,
            businessHours = businessHoursData
        };
    }
}
