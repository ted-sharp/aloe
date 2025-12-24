using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class CalendarCanvas
{
    // ============================================================
    // JSInvokable callbacks (called from JavaScript)
    // ============================================================

    [JSInvokable]
    public async Task OnDateSelectedCallback(string dateStr)
    {
        if (DateOnly.TryParse(dateStr, out var date))
        {
            await this.OnDateSelected.InvokeAsync(date);
        }
    }

    [JSInvokable("OnDateSelected")]
    public async Task OnDateSelectedFromJs(string dateStr)
    {
        if (DateOnly.TryParse(dateStr, out var date))
        {
            await this.OnDateSelected.InvokeAsync(date);
        }
    }

    [JSInvokable("OnAppointmentClicked")]
    public async Task OnAppointmentClickedFromJs(string apptId)
    {
        if (Guid.TryParse(apptId, out var id))
        {
            await this.OnAppointmentClicked.InvokeAsync(id);
        }
    }

    [JSInvokable("OnCreateRequested")]
    public async Task OnCreateRequestedFromJs(string dateStr, string timeStr)
    {
        if (DateOnly.TryParse(dateStr, out var date) &&
            TimeOnly.TryParse(timeStr, out var time))
        {
            await this.OnCreateRequested.InvokeAsync((date, time));
        }
    }

    [JSInvokable("OnAppointmentMoved")]
    public async Task OnAppointmentMovedFromJs(string apptId, string dateStr, string timeStr)
    {
        if (Guid.TryParse(apptId, out var id) &&
            DateOnly.TryParse(dateStr, out var date) &&
            TimeOnly.TryParse(timeStr, out var time))
        {
            await this.OnAppointmentMoved.InvokeAsync((id, date, time));
        }
    }

    [JSInvokable("OnMonthClicked")]
    public async Task OnMonthClickedFromJs(int year, int month)
    {
        await this.OnMonthClicked.InvokeAsync((year, month));
    }

    [JSInvokable("OnDateSelectedSingle")]
    public async Task OnDateSelectedSingleFromJs(string dateStr)
    {
        if (DateOnly.TryParse(dateStr, out var date))
        {
            await this.OnDateSelectedSingle.InvokeAsync(date);
        }
    }

    [JSInvokable("OnDateDoubleClicked")]
    public async Task OnDateDoubleClickedFromJs(string dateStr)
    {
        if (DateOnly.TryParse(dateStr, out var date))
        {
            await this.OnDateDoubleClicked.InvokeAsync(date);
        }
    }

    [JSInvokable("OnDateRangeSelected")]
    public async Task OnDateRangeSelectedFromJs(string startDateStr, string endDateStr)
    {
        if (DateOnly.TryParse(startDateStr, out var startDate) &&
            DateOnly.TryParse(endDateStr, out var endDate))
        {
            await this.OnDateRangeSelected.InvokeAsync((startDate, endDate));
        }
    }

    [JSInvokable("ShowDayDetail")]
    public async Task ShowDayDetailFromJs(string dateStr)
    {
        if (DateOnly.TryParse(dateStr, out var date))
        {
            await this.OnShowDayDetail.InvokeAsync(date);
        }
    }
}

