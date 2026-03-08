using Aloe.Apps.MedockLib.Services.Dtos;
using System.Text.Json;
using System.Linq;

namespace Aloe.Apps.MedockServer.Components.Calendar;

/// <summary>
/// カレンダーキャンバスのJSInterop関連処理とデータ変換
/// </summary>
public static class CalendarCanvasInterop
{
    /// <summary>
    /// カレンダーデータをJavaScript用のオブジェクトに変換します。
    /// サービス層で構築されたDTOをそのままJSに渡します。
    /// </summary>
    public static object BuildDataObject(CalendarDataDto data)
    {
        // DTOをそのままJSONシリアライズ可能なオブジェクトに変換
        return new
        {
            appointments = data.Appointments.Select(a => new
            {
                id = a.Id,
                date = a.Date,
                startMin = a.StartMin,
                patientName = a.PatientName,
                organizationName = a.OrganizationName,
                status = a.Status,
                patientId = a.PatientId,
                organizationId = a.OrganizationId,
                floorName = a.FloorName,
                floorId = a.FloorId
            }).ToArray(),
            mainStats = data.MainStats.ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    resourceId = kvp.Value.ResourceId,
                    resourceName = kvp.Value.ResourceName,
                    totalAvailable = kvp.Value.TotalAvailable,
                    totalCapacity = kvp.Value.TotalCapacity,
                    slotStartMins = kvp.Value.SlotStartMins,
                    slotEndMins = kvp.Value.SlotEndMins,
                    slotCounts = kvp.Value.SlotCounts,
                    slotCaps = kvp.Value.SlotCaps,
                    slotAvailables = kvp.Value.SlotAvailables,
                    slotFlags = kvp.Value.SlotFlags,
                    slotFilteredCounts = kvp.Value.SlotFilteredCounts,
                    isDayGrayedOut = kvp.Value.IsDayGrayedOut
                }
            ),
            equipmentStats = data.EquipmentStats.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToDictionary(
                    r => r.Key,
                    r => new
                    {
                        resourceId = r.Value.ResourceId,
                        resourceName = r.Value.ResourceName,
                        totalAvailable = r.Value.TotalAvailable,
                        totalCapacity = r.Value.TotalCapacity,
                        slotStartMins = r.Value.SlotStartMins,
                        slotEndMins = r.Value.SlotEndMins,
                        slotCounts = r.Value.SlotCounts,
                        slotCaps = r.Value.SlotCaps,
                        slotAvailables = r.Value.SlotAvailables,
                        slotFlags = r.Value.SlotFlags,
                        slotFilteredCounts = r.Value.SlotFilteredCounts,
                        isDayGrayedOut = r.Value.IsDayGrayedOut
                    }
                )
            ),
            holidays = data.Holidays
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
