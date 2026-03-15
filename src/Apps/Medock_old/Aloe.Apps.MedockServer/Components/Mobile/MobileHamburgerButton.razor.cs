using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Mobile;

public partial class MobileHamburgerButton : ComponentBase
{
    [Parameter]
    public EventCallback OnClick { get; set; }

    private async Task HandleClick()
    {
        await this.OnClick.InvokeAsync();
    }
}
