using Aloe.Apps.MedockLib.Services;
using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.FAB;

public partial class UserAvatarFab : ComponentBase
{
    [Parameter]
    public string UserInitial { get; set; } = "U";

    [Parameter]
    public string UserDisplayName { get; set; } = "";

    [Parameter]
    public string UserEmail { get; set; } = "";

    [Parameter]
    public string TenantName { get; set; } = "";

    [Parameter]
    public string FacilityName { get; set; } = "";

    [Parameter]
    public string UserRole { get; set; } = "";

    [Parameter]
    public Guid? CurrentFacilityId { get; set; }

    [Parameter]
    public bool HasMultipleFacilities { get; set; }

    [Parameter]
    public List<FacilityInfo>? AvailableFacilities { get; set; }

    [Parameter]
    public EventCallback OnLogout { get; set; }

    [Parameter]
    public EventCallback<Guid> OnFacilitySwitch { get; set; }

    private bool IsMenuOpen { get; set; }
    private bool IsDarkMode { get; set; }
    private bool ShowFacilityModal { get; set; }

    private void ToggleMenu()
    {
        this.IsMenuOpen = !this.IsMenuOpen;
    }

    private void CloseMenu()
    {
        this.IsMenuOpen = false;
    }

    private async Task HandleLogout()
    {
        this.CloseMenu();
        await this.OnLogout.InvokeAsync();
    }

    private async Task ToggleTheme()
    {
        // テーマ切り替えはJSInteropで実装
        await Task.CompletedTask;
    }

    private void ShowFacilitySelector()
    {
        this.ShowFacilityModal = true;
        this.CloseMenu();
    }

    private void CloseFacilityModal()
    {
        this.ShowFacilityModal = false;
    }

    private async Task HandleFacilitySwitch(Guid facilityId)
    {
        this.ShowFacilityModal = false;
        await this.OnFacilitySwitch.InvokeAsync(facilityId);
    }
}




