using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.Dtos;
using Aloe.Apps.MedockLib.Services.Dtos.Appointments;
using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class WeekView : ComponentBase
{
    [Parameter]
    public DateOnly CurrentDate { get; set; }

    [Parameter]
    public EventCallback<DateTime> OnTimeSlotClick { get; set; }

    [Parameter]
    public IEnumerable<AppointmentDto>? Appointments { get; set; }

    /// <summary>
    /// 指定された日付と時間の予約を取得します
    /// </summary>
    private IEnumerable<(string Name, string Org, int Status)> GetHourAppointments(DateOnly date, int hour)
    {
        if (this.Appointments == null) yield break;

        var hourStart = new TimeOnly(hour, 0);
        var hourEnd = new TimeOnly(hour + 1, 0);

        foreach (var appt in this.Appointments)
        {
            if (appt.Date != date) continue;
            // if (appt.StartTime == null) continue; // StartMin is int, always valid

            // 予約の開始時間がこの時間帯に含まれるか、または予約がこの時間帯と重なる場合
            if (appt.StartMin >= hourStart.Hour * 60 && appt.StartMin < (hour + 1) * 60)
            {
                yield return (
                    appt.PatientName ?? "未設定",
                    appt.OrganizationName ?? "",
                    appt.Status
                );
            }
        }
    }

    private string GetStatusClass(int status)
    {
        return status switch
        {
            0 => "bg-warning/20 border-l-4 border-warning",
            1 => "bg-info/20 border-l-4 border-info",
            2 => "bg-success/20 border-l-4 border-success",
            3 => "bg-error/20 border-l-4 border-error",
            _ => "bg-base-200"
        };
    }
}


