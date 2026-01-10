using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services.Dtos;
using Aloe.Apps.MedockLib.Services.Dtos.Appointments;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// カレンダー表示用データ構築サービス実装
/// </summary>
public class CalendarDataService : ICalendarDataService
{
    /// <inheritdoc />
    public async Task<CalendarDataDto> BuildCalendarDataAsync(
        IEnumerable<AppointmentDto> appointments,
        Dictionary<string, List<AppointmentStats>> mainStats,
        Dictionary<string, bool> mainStatsGrayedOut,
        Dictionary<string, string> holidays,
        List<string>? filterTimeSlots = null,
        Dictionary<string, List<ResourceStatSlotsDto>>? equipmentStats = null,
        BusinessHoursDto? businessHours = null,
        Dictionary<(DateOnly ApptDate, Guid ApptResId), List<AppointmentStatSlots>>? mainStatsSlots = null)
    {
        var appointmentArray = appointments?.Select(a => new AppointmentDataDto
        {
            Id = a.Id.ToString(),
            Date = a.Date.ToString("yyyy-MM-dd"),
            StartMin = a.StartMin,
            PatientName = a.PatientName,
            OrganizationName = a.OrganizationName,
            Status = a.Status,
            PatientId = a.PatientId,
            OrganizationId = a.OrganizationId,
            FloorName = a.FloorName,
            FloorId = a.FloorId
        }).ToList() ?? new List<AppointmentDataDto>();

        // フィルター時間帯のHour値を事前に計算
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

        var mainStatsDict = new Dictionary<string, ResourceStatSlotsDto>();
        if (mainStats != null)
        {
            foreach (var kvp in mainStats)
            {
                var dateStr = kvp.Key;
                var statsList = kvp.Value;

                var stat = statsList.FirstOrDefault();

                // 並列配列を初期化
                int[] slotStarts = Array.Empty<int>();
                int[] slotEnds = Array.Empty<int>();
                int[] slotCounts = Array.Empty<int>();
                int[] slotCaps = Array.Empty<int>();
                int[] slotAvailables = Array.Empty<int>();
                byte[]? slotFlags = null;
                int[]? slotFilteredCounts = null;

                // Try to get slots from the mainStatsSlots dictionary
                List<AppointmentStatSlots> validSlots = new();
                if (stat != null && mainStatsSlots != null)
                {
                    var key = (stat.ApptDate, stat.ApptResId);
                    if (mainStatsSlots.TryGetValue(key, out var slots))
                    {
                        validSlots = slots
                            .Where(s => !s.IsDeleted)
                            .OrderBy(s => s.SlotStart)
                            .ToList();
                    }
                }

                if (stat != null && validSlots.Count > 0)
                {
                    var count = validSlots.Count;
                    slotStarts = new int[count];
                    slotEnds = new int[count];
                    slotCounts = new int[count];
                    slotCaps = new int[count];
                    slotAvailables = new int[count];
                    slotFlags = new byte[count];
                    slotFilteredCounts = new int[count];

                    for (int i = 0; i < count; i++)
                    {
                        var statSlot = validSlots[i];
                        var slotStartTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(statSlot.SlotStart));
                        var slotEndTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(statSlot.SlotEnd));
                        var isSlotGrayed = false;
                        if (filterHours != null && filterHours.Count > 0)
                        {
                            isSlotGrayed = !filterHours.Contains(slotStartTime.Hour);
                        }

                        slotStarts[i] = statSlot.SlotStart;
                        slotEnds[i] = statSlot.SlotEnd;
                        slotCounts[i] = statSlot.SlotCount;
                        slotCaps[i] = statSlot.SlotCap;
                        slotAvailables[i] = statSlot.SlotAvailable;

                        // 業務時間外スロットの種類を判定（Before/After/Lunch）
                        var isOutsideHoursBefore = false;
                        var isOutsideHoursAfter = false;
                        var isOutsideHoursLunch = false;
                        if (businessHours != null)
                        {
                            var businessStart = TimeOnly.Parse(businessHours.StartTime);
                            var businessEnd = TimeOnly.Parse(businessHours.EndTime);

                            // 朝の時間外: スロットが業務開始時刻より前に終わる
                            if (slotEndTime <= businessStart)
                            {
                                isOutsideHoursBefore = true;
                            }
                            // 夕方の時間外: スロットが業務終了時刻以降に開始する、または業務終了時刻以降で終わる
                            else if (slotStartTime >= businessEnd || slotEndTime > businessEnd)
                            {
                                isOutsideHoursAfter = true;
                            }
                            // 昼休み時間帯にある場合
                            else if (!String.IsNullOrEmpty(businessHours.LunchStartTime) &&
                                     !String.IsNullOrEmpty(businessHours.LunchEndTime))
                            {
                                var lunchStart = TimeOnly.Parse(businessHours.LunchStartTime);
                                var lunchEnd = TimeOnly.Parse(businessHours.LunchEndTime);

                                if (slotStartTime >= lunchStart && slotEndTime <= lunchEnd)
                                {
                                    isOutsideHoursLunch = true;
                                }
                            }
                        }

                        // フラグをビット単位で設定
                        byte flags = 0;
                        if (isSlotGrayed) flags |= 0b0001;          // ビット0: IsGrayedOut
                        if (isOutsideHoursBefore) flags |= 0b0010;  // ビット1: IsOutsideHoursBefore（朝）
                        if (isOutsideHoursAfter) flags |= 0b0100;   // ビット2: IsOutsideHoursAfter（夕方）
                        if (isOutsideHoursLunch) flags |= 0b1000;   // ビット3: IsOutsideHoursLunch（昼休み）
                        slotFlags[i] = flags;

                        slotFilteredCounts[i] = 0; // 現時点では常に0
                    }
                }

                var isDayGrayedOut = mainStatsGrayedOut?.TryGetValue(dateStr, out var grayed) == true && grayed;

                // リソースタイプコードを取得（statからAppointmentResource経由で取得）
                var resourceTypeCode = stat?.AppointmentResource?.ApptResTypeCode ?? 0;

                mainStatsDict[dateStr] = new ResourceStatSlotsDto
                {
                    ResourceId = null, // Mainでは使用しない
                    ResourceName = null, // Mainでは使用しない
                    TotalAvailable = 0, // Mainでは使用しない
                    TotalCapacity = 0, // Mainでは使用しない
                    SlotStartMins = slotStarts,
                    SlotEndMins = slotEnds,
                    SlotCounts = slotCounts,
                    SlotCaps = slotCaps,
                    SlotAvailables = slotAvailables,
                    SlotFlags = slotFlags,
                    SlotFilteredCounts = slotFilteredCounts,
                    IsDayGrayedOut = isDayGrayedOut,
                    ResourceTypeCode = resourceTypeCode,
                    PlanTypeCode = null // プランタイプは現時点では未対応
                };
            }
        }

        var holidaysDict = holidays ?? new Dictionary<string, string>();

        // Equipment統計データを処理（最適化版：既に配列化済み）
        var equipmentStatsDict = new Dictionary<string, Dictionary<string, ResourceStatSlotsDto>>();
        if (equipmentStats != null)
        {
            foreach (var kvp in equipmentStats)
            {
                var dateStr = kvp.Key;
                var statsList = kvp.Value;

                var resourcesDict = new Dictionary<string, ResourceStatSlotsDto>();

                // 既に配列化済みなのでそのまま使用
                foreach (var stat in statsList)
                {
                    resourcesDict[stat.ResourceId ?? String.Empty] = stat;
                }

                equipmentStatsDict[dateStr] = resourcesDict;
            }
        }

        var result = new CalendarDataDto
        {
            Appointments = appointmentArray,
            MainStats = mainStatsDict,
            EquipmentStats = equipmentStatsDict,
            Holidays = holidaysDict
        };

        return result;
    }
}

