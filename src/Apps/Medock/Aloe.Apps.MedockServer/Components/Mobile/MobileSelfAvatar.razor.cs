using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Mobile;

public partial class MobileSelfAvatar : ComponentBase
{
    [Parameter]
    public string UserInitial { get; set; } = "U";

    [Parameter]
    public string UserDisplayName { get; set; } = "";

    [Parameter]
    public string UserEmail { get; set; } = "";

    [Parameter]
    public string FacilityName { get; set; } = "";

    private bool _menuOpen;

    private void ToggleMenu() => this._menuOpen = !this._menuOpen;
    private void CloseMenu() => this._menuOpen = false;
}
