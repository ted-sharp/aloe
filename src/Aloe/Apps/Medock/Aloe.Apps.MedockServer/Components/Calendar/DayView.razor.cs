using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.Dtos;
using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class DayView : ComponentBase
{
    [Parameter]
    public DateOnly CurrentDate { get; set; }

    [Parameter]
    public EventCallback<DateTime> OnTimeSlotClick { get; set; }

    [Parameter]
    public IEnumerable<AppointmentDto>? Appointments { get; set; }

    /// <summary>
    /// 指定された時間の予約を取得します
    /// </summary>
    private IEnumerable<(string Name, string Org, int Status)> GetHourAppointments(int hour)
    {
        if (this.Appointments == null) yield break;

        var hourStart = new TimeOnly(hour, 0);
        var hourEnd = new TimeOnly(hour + 1, 0);

        foreach (var appt in this.Appointments)
        {
            if (appt.Date != this.CurrentDate) continue;
            if (appt.StartTime == null) continue;

            // 予約の開始時間がこの時間帯に含まれるか、または予約がこの時間帯と重なる場合
            if (appt.StartTime.Value >= hourStart && appt.StartTime.Value < hourEnd)
            {
                yield return (
                    appt.PatientName ?? "未設定",
                    appt.OrganizationName ?? "",
                    appt.Status
                );
            }
        }
    }

    private string GetStatusBorderClass(int status)
    {
        return status switch
        {
            0 => "border-l-4 border-warning",
            1 => "border-l-4 border-info",
            2 => "border-l-4 border-success",
            3 => "border-l-4 border-error",
            _ => ""
        };
    }

    private string GetStatusBadgeClass(int status)
    {
        return status switch
        {
            0 => "badge-warning",
            1 => "badge-info",
            2 => "badge-success",
            3 => "badge-error",
            _ => ""
        };
    }

    private string GetStatusText(int status)
    {
        return status switch
        {
            0 => "予約",
            1 => "待機中",
            2 => "来院済み",
            3 => "キャンセル",
            _ => "不明"
        };
    }
}


