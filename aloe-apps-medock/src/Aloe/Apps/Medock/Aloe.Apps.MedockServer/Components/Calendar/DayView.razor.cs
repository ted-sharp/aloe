using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.Dtos;
using Aloe.Apps.MedockLib.Services.Dtos.Appointments;
using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class DayView : ComponentBase
{
    [Inject]
    private IDateTimeProvider DateTimeProvider { get; set; } = default!;

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
        return AppointmentFilterHelper.GetHourAppointments(this.Appointments, this.CurrentDate, hour);
    }

    private string GetStatusBorderClass(int status)
    {
        return AppointmentStatusHelper.GetBorderClass(status);
    }

    private string GetStatusBadgeClass(int status)
    {
        return AppointmentStatusHelper.GetBadgeClass(status);
    }

    private string GetStatusText(int status)
    {
        return AppointmentStatusHelper.GetStatusText(status);
    }
}


