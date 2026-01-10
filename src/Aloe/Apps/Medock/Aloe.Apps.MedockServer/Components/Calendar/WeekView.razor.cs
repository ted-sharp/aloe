using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.Dtos;
using Aloe.Apps.MedockLib.Services.Dtos.Appointments;
using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class WeekView : ComponentBase
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
    /// 指定された日付と時間の予約を取得します
    /// </summary>
    private IEnumerable<(string Name, string Org, int Status)> GetHourAppointments(DateOnly date, int hour)
    {
        return AppointmentFilterHelper.GetHourAppointments(this.Appointments, date, hour);
    }

    private string GetStatusClass(int status)
    {
        return AppointmentStatusHelper.GetBackgroundWithBorderClass(status);
    }
}


