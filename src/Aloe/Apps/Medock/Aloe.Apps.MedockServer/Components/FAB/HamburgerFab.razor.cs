using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.FAB;

public partial class HamburgerFab : ComponentBase
{
    [Parameter]
    public bool IsDrawerOpen { get; set; }

    [Parameter]
    public EventCallback<bool> IsDrawerOpenChanged { get; set; }

    private async Task Toggle()
    {
        this.IsDrawerOpen = !this.IsDrawerOpen;
        await this.IsDrawerOpenChanged.InvokeAsync(this.IsDrawerOpen);
    }
}


