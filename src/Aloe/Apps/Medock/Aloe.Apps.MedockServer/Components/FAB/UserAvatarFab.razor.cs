using Aloe.Apps.MedockLib.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Aloe.Apps.MedockServer.Components.FAB;

public partial class UserAvatarFab : ComponentBase
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

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
    private string SelectedTheme { get; set; } = "light";
    private bool ShowFacilityModal { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // 現在のテーマを取得
        try
        {
            SelectedTheme = await JSRuntime.InvokeAsync<string>(
                "eval", "document.documentElement.getAttribute('data-theme') || 'light'") ?? "light";
        }
        catch
        {
            SelectedTheme = "light";
        }
    }

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

    private async Task ChangeTheme(string newTheme)
    {
        try
        {
            // テーマを適用（calendar-main.js の setTheme 関数を呼び出し）
            await JSRuntime.InvokeVoidAsync(
                "eval", $"(function(){{document.documentElement.setAttribute('data-theme', '{newTheme}');try{{localStorage.setItem('medock-theme', '{newTheme}');}}catch(e){{}}}})()");

            SelectedTheme = newTheme;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error changing theme: {ex.Message}");
        }
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




