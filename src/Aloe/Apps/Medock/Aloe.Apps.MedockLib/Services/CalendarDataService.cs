using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// カレンダー表示用データ構築サービス実装
/// </summary>
public class CalendarDataService : ICalendarDataService
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

        /// <summary>
        /// 時間外の予約が含まれているかどうか
        /// 
        /// 注意: スロット定義（AppointmentSlot）は業務時間内（例：09:00-12:00、13:00-17:00）のみ存在しますが、
        /// 予約データ（Appointments）は時刻を自由に指定できるため、時間外（開始前・終了後・昼休み時間外）の予約も存在し得ます。
        /// 時間外の予約は、近いスロットに吸収される（集計される）が、このフラグが立つことで時間外の予約が含まれていることを示します。
        /// </summary>
        [JsonPropertyName("hasOutsideHours")]
        public bool HasOutsideHours { get; set; } = false;
    }

    /// <inheritdoc />
    public Task<CalendarDataDto> BuildCalendarDataAsync(
        IEnumerable<AppointmentDto> appointments,
        Dictionary<string, List<AppointmentStats>> mainStats,
        Dictionary<string, bool> mainStatsGrayedOut,
        Dictionary<string, string> holidays)
    {
        var appointmentArray = appointments?.Select(a => new AppointmentDataDto
        {
            Id = a.Id.ToString(),
            Date = a.Date.ToString("yyyy-MM-dd"),
            StartTime = a.StartTime?.ToString("HH:mm") ?? "09:00",
            EndTime = a.EndTime?.ToString("HH:mm") ?? "10:00",
            PatientName = a.PatientName,
            OrganizationName = a.OrganizationName,
            Status = a.Status,
            PatientId = a.PatientId,
            OrganizationId = a.OrganizationId,
            FloorName = a.FloorName,
            FloorId = a.FloorId
        }).ToList() ?? new List<AppointmentDataDto>();

        var mainStatsDict = new Dictionary<string, MainStatsDataDto>();
        if (mainStats != null)
        {
            foreach (var kvp in mainStats)
            {
                var dateStr = kvp.Key;
                var statsList = kvp.Value;

                // Mainリソースは各Floorに1つだけのため、合算処理は不要
                // 最初の要素（唯一の要素）のApptGraphをそのまま使用
                var stat = statsList.FirstOrDefault();
                List<SlotDataDto> slots = new();

                if (stat != null)
                {
                    try
                    {
                        var graphData = JsonSerializer.Deserialize<GraphDefinition>(stat.ApptGraph);
                        if (graphData?.Slots != null)
                        {
                            slots = graphData.Slots.Select(slot => new SlotDataDto
                            {
                                Start = slot.Start.ToString("HH:mm"),
                                End = slot.End.ToString("HH:mm"),
                                Count = slot.Count,
                                Cap = slot.Cap,
                                Available = slot.Cap - slot.Count,
                                IsGrayedOut = false,
                                FilteredCount = 0,
                                IsOutsideHours = slot.HasOutsideHours // 時間外スロットフラグをマッピング
                            }).OrderBy(s => s.Start).ToList();
                        }
                    }
                    catch (JsonException)
                    {
                        // JSONパースエラーは無視して続行（空のslotsリストのまま）
                    }
                }

                var isGrayedOut = mainStatsGrayedOut?.TryGetValue(dateStr, out var grayed) == true && grayed;

                mainStatsDict[dateStr] = new MainStatsDataDto
                {
                    Slots = slots,
                    IsGrayedOut = isGrayedOut
                };
            }
        }

        var holidaysDict = holidays ?? new Dictionary<string, string>();

        var result = new CalendarDataDto
        {
            Appointments = appointmentArray,
            MainStats = mainStatsDict,
            Holidays = holidaysDict
        };

        return Task.FromResult(result);
    }
}

