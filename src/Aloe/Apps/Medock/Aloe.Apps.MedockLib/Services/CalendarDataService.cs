using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services.Dtos;

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
        List<string>? filterTimeSlots = null,
        Dictionary<string, List<AppointmentStats>>? equipmentStats = null)
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
                // 最初の要素（唯一の要素）のAppointmentStatSlotsをそのまま使用
                var stat = statsList.FirstOrDefault();
                List<SlotDataDto> slots = new();

                if (stat != null && stat.AppointmentStatSlots != null)
                {
                    slots = stat.AppointmentStatSlots
                        .Where(s => !s.IsDeleted)
                        .Select(statSlot =>
                        {
                            // スロットの開始時刻を分単位からTimeOnlyに変換
                            var slotStartTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(statSlot.SlotStart));
                            var slotEndTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(statSlot.SlotEnd));

                            // スロットの開始時刻がフィルター時間帯にマッチするかをチェック
                            var isSlotGrayed = false;
                            if (filterHours != null && filterHours.Count > 0)
                            {
                                // フィルターがある場合、スロットの開始時刻のHourがフィルターに含まれていなければグレーアウト
                                isSlotGrayed = !filterHours.Contains(slotStartTime.Hour);
                            }

                            return new SlotDataDto
                            {
                                Start = slotStartTime.ToString("HH:mm"),
                                End = slotEndTime.ToString("HH:mm"),
                                Count = statSlot.SlotCount,
                                Cap = statSlot.SlotCap,
                                Available = statSlot.SlotAvailable,
                                IsGrayedOut = isSlotGrayed,
                                FilteredCount = 0,
                                IsOutsideHours = false // TODO: 時間外スロットフラグの判定が必要な場合は追加
                            };
                        }).OrderBy(s => s.Start).ToList();
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

        // Equipment統計データを処理
        var equipmentStatsDict = new Dictionary<string, EquipmentStatsDataDto>();
        if (equipmentStats != null)
        {
            foreach (var kvp in equipmentStats)
            {
                var dateStr = kvp.Key;
                var statsList = kvp.Value;

                var resourcesDict = new Dictionary<string, EquipmentResourceStatsDto>();

                // Equipmentリソースごとにグループ化
                var resourceGroups = statsList.GroupBy(s => s.ApptResId);

                foreach (var resourceGroup in resourceGroups)
                {
                    var firstStat = resourceGroup.First();
                    var resourceId = firstStat.ApptResId.ToString();
                    var resourceName = firstStat.AppointmentResource?.ApptResName ?? "";

                    // スロットデータを配列構築用リストに収集
                    var slotDataList = new List<(int start, int end, int available)>();
                    var totalAvailable = 0;
                    var totalCapacity = 0;

                    foreach (var stat in resourceGroup)
                    {
                        if (stat.AppointmentStatSlots != null)
                        {
                            foreach (var statSlot in stat.AppointmentStatSlots.Where(s => !s.IsDeleted))
                            {
                                totalAvailable += statSlot.SlotAvailable;
                                totalCapacity += statSlot.SlotCap;

                                slotDataList.Add((
                                    statSlot.SlotStart,      // 既に分単位
                                    statSlot.SlotEnd,
                                    statSlot.SlotAvailable
                                ));
                            }
                        }
                    }

                    // 時刻順にソート（念のため、通常は既にソート済み）
                    slotDataList.Sort((a, b) => a.start.CompareTo(b.start));

                    // 配列に変換
                    var slotCount = slotDataList.Count;
                    var startMinutes = new int[slotCount];
                    var endMinutes = new int[slotCount];
                    var availables = new int[slotCount];

                    for (int i = 0; i < slotCount; i++)
                    {
                        var slot = slotDataList[i];
                        startMinutes[i] = slot.start;
                        endMinutes[i] = slot.end;
                        availables[i] = slot.available;
                    }

                    resourcesDict[resourceId] = new EquipmentResourceStatsDto
                    {
                        ResourceId = resourceId,
                        ResourceName = resourceName,
                        TotalAvailable = totalAvailable,
                        TotalCapacity = totalCapacity,
                        SlotStartMinutes = startMinutes,
                        SlotEndMinutes = endMinutes,
                        SlotAvailables = availables,
                        SlotFlags = null  // 現時点ではフィルタ未実装なのでnull
                    };
                }

                equipmentStatsDict[dateStr] = new EquipmentStatsDataDto
                {
                    Resources = resourcesDict
                };
            }
        }

        var result = new CalendarDataDto
        {
            Appointments = appointmentArray,
            MainStats = mainStatsDict,
            EquipmentStats = equipmentStatsDict,
            Holidays = holidaysDict
        };

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<CalendarDataDto> BuildCalendarDataFromOptimizedAsync(
        IEnumerable<AppointmentDto> appointments,
        Dictionary<string, List<AppointmentStats>> mainStats,
        Dictionary<string, bool> mainStatsGrayedOut,
        Dictionary<string, string> holidays,
        List<string>? filterTimeSlots = null,
        Dictionary<string, List<EquipmentResourceStatsDto>>? equipmentStats = null)
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

        var mainStatsDict = new Dictionary<string, MainStatsDataDto>();
        if (mainStats != null)
        {
            foreach (var kvp in mainStats)
            {
                var dateStr = kvp.Key;
                var statsList = kvp.Value;

                var stat = statsList.FirstOrDefault();
                List<SlotDataDto> slots = new();

                if (stat != null && stat.AppointmentStatSlots != null)
                {
                    slots = stat.AppointmentStatSlots
                        .Where(s => !s.IsDeleted)
                        .Select(statSlot =>
                        {
                            var slotStartTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(statSlot.SlotStart));
                            var isSlotGrayed = false;
                            if (filterHours != null && filterHours.Count > 0)
                            {
                                isSlotGrayed = !filterHours.Contains(slotStartTime.Hour);
                            }

                            return new SlotDataDto
                            {
                                Start = slotStartTime.ToString("HH:mm"),
                                End = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(statSlot.SlotEnd)).ToString("HH:mm"),
                                Count = statSlot.SlotCount,
                                Cap = statSlot.SlotCap,
                                Available = statSlot.SlotAvailable,
                                IsGrayedOut = isSlotGrayed,
                                FilteredCount = 0,
                                IsOutsideHours = false
                            };
                        }).OrderBy(s => s.Start).ToList();
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

        // Equipment統計データを処理（最適化版：既に配列化済み）
        var equipmentStatsDict = new Dictionary<string, EquipmentStatsDataDto>();
        if (equipmentStats != null)
        {
            foreach (var kvp in equipmentStats)
            {
                var dateStr = kvp.Key;
                var statsList = kvp.Value;

                var resourcesDict = new Dictionary<string, EquipmentResourceStatsDto>();

                // 既に配列化済みなのでそのまま使用
                foreach (var stat in statsList)
                {
                    resourcesDict[stat.ResourceId] = stat;
                }

                equipmentStatsDict[dateStr] = new EquipmentStatsDataDto
                {
                    Resources = resourcesDict
                };
            }
        }

        var result = new CalendarDataDto
        {
            Appointments = appointmentArray,
            MainStats = mainStatsDict,
            EquipmentStats = equipmentStatsDict,
            Holidays = holidaysDict
        };

        return Task.FromResult(result);
    }
}

