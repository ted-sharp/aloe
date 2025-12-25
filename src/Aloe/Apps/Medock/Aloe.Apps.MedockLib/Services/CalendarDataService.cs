using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services.Dtos;
using System.Text.Json;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// カレンダー表示用データ構築サービス実装
/// </summary>
public class CalendarDataService : ICalendarDataService
{
    /// <inheritdoc />
    public Task<CalendarDataDto> BuildCalendarDataAsync(
        IEnumerable<AppointmentDto> appointments,
        Dictionary<string, List<AppointmentStats>> mainStats,
        Dictionary<string, bool> mainStatsGrayedOut,
        Dictionary<string, string> holidays,
        List<string>? filterTimeSlots = null)
    {
        var appointmentArray = appointments?.Select(a => new AppointmentDataDto
        {
            Id = a.Id.ToString(),
            Date = a.Date.ToString("yyyy-MM-dd"),
            StartTime = a.StartTime?.ToString("HH:mm") ?? BusinessHoursConstants.DefaultAppointmentStartTime,
            EndTime = a.EndTime?.ToString("HH:mm") ?? BusinessHoursConstants.DefaultAppointmentEndTime,
            PatientName = a.PatientName,
            OrganizationName = a.OrganizationName,
            Status = a.Status,
            PatientId = a.PatientId,
            OrganizationId = a.OrganizationId,
            FloorName = a.FloorName,
            FloorId = a.FloorId
        }).ToList() ?? new List<AppointmentDataDto>();

        // フィルター時間帯のHour値を事前に計算（パフォーマンス向上のため）
        HashSet<int>? filterHours = null;
        if (filterTimeSlots != null && filterTimeSlots.Count > 0)
        {
            filterHours = new HashSet<int>();
            foreach (var timeSlot in filterTimeSlots)
            {
                if (TimeOnly.TryParse(timeSlot, out var time))
                {
                    filterHours.Add(time.Hour);
                }
            }
        }

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
                        var graphData = JsonSerializer.Deserialize<AppointmentGraphRoot>(stat.ApptGraph);
                        if (graphData?.Slots != null)
                        {
                            slots = graphData.Slots.Select(slot =>
                            {
                                // スロットの開始時刻がフィルター時間帯にマッチするかをチェック
                                var isSlotGrayed = false;
                                if (filterHours != null && filterHours.Count > 0)
                                {
                                    // フィルターがある場合、スロットの開始時刻のHourがフィルターに含まれていなければグレーアウト
                                    isSlotGrayed = !filterHours.Contains(slot.Start.Hour);
                                }

                                return new SlotDataDto
                                {
                                    Start = slot.Start.ToString("HH:mm"),
                                    End = slot.End.ToString("HH:mm"),
                                    Count = slot.Count,
                                    Cap = slot.Cap,
                                    Available = slot.Cap - slot.Count,
                                    IsGrayedOut = isSlotGrayed,
                                    FilteredCount = 0,
                                    IsOutsideHours = slot.HasOutsideHours // 時間外スロットフラグをマッピング
                                };
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

