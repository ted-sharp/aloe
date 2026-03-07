using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Mobile;

public partial class MobileDayDetail : ComponentBase
{
    [Parameter]
    public DateOnly? SelectedDate { get; set; }

    [Parameter]
    public List<AppointmentStats>? DayStats { get; set; }

    [Parameter]
    public EventCallback<DateOnly> OnCreateRequested { get; set; }

    private async Task HandleCreateClick()
    {
        if (this.SelectedDate.HasValue)
        {
            await this.OnCreateRequested.InvokeAsync(this.SelectedDate.Value);
        }
    }
}
