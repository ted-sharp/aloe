using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Mobile;

public partial class MobileLongPressMenu : ComponentBase
{
    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    public DateOnly? SelectedDate { get; set; }

    [Parameter]
    public EventCallback<int> OnCreateAppointment { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    private async Task HandleCreateAppointment()
    {
        await this.OnCreateAppointment.InvokeAsync(0); // status=0: 予約
        await this.OnClose.InvokeAsync();
    }

    private async Task HandleCreateTentative()
    {
        await this.OnCreateAppointment.InvokeAsync(1); // status=1: 仮予約
        await this.OnClose.InvokeAsync();
    }

    private async Task HandleClose()
    {
        await this.OnClose.InvokeAsync();
    }
}
