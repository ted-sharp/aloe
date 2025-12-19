using Aloe.Apps.MedockLib.Services;
using System.Linq;

namespace Aloe.Apps.MedockServer.Components.Calendar;

/// <summary>
/// カレンダーキャンバスのJSInterop関連処理とデータ変換
/// </summary>
public static class CalendarCanvasInterop
{
    /// <summary>
    /// カレンダーデータをJavaScript用のオブジェクトに変換します。
    /// </summary>
    public static object BuildDataObject(
        IEnumerable<AppointmentDto>? appointments,
        Dictionary<string, CalendarDayStats>? dayStats,
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

        var dayStatsDict = dayStats != null
            ? dayStats.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)new
                {
                    am = kvp.Value.AmCount,
                    pm = kvp.Value.PmCount,
                    amMax = kvp.Value.AmMax,
                    pmMax = kvp.Value.PmMax,
                    slots = kvp.Value.Slots?.Select(s => new
                    {
                        time = s.Time,
                        count = s.Count,
                        max = s.Max,
                        isGrayedOut = s.IsGrayedOut,
                        filteredCount = s.FilteredCount
                    }).ToArray(),
                    isGrayedOut = kvp.Value.IsGrayedOut
                })
            : new Dictionary<string, object>();

        var holidaysDict = holidays ?? new Dictionary<string, string>();

        return new
        {
            appointments = appointmentArray,
            dayStats = dayStatsDict,
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
