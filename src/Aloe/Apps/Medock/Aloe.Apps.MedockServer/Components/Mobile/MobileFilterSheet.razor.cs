using Aloe.Apps.MedockServer.Components.Calendar;
using Aloe.Apps.MedockServer.Components.Pages;
using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Mobile;

public partial class MobileFilterSheet : ComponentBase
{
    [Inject]
    private CalendarState CalendarState { get; set; } = default!;

    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    public List<SearchFilterPanel.FilterItem>? AvailableFloors { get; set; }

    [Parameter]
    public List<SearchFilterPanel.FilterItem>? AvailableResources { get; set; }

    [Parameter]
    public List<SearchFilterPanel.FilterItem>? AvailablePlans { get; set; }

    [Parameter]
    public EventCallback<SearchFilterPanel.SearchFilter> OnApply { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    private HashSet<int> _selectedDays = new();
    private HashSet<Guid> _selectedFloorIds = new();
    private HashSet<Guid> _selectedResourceIds = new();
    private int _requiredCapacity = 1;

    protected override void OnParametersSet()
    {
        if (this.IsOpen)
        {
            // CalendarState から現在の選択状態を読み込む
            this._selectedDays = new HashSet<int>(this.CalendarState.FilterSelectedDays);
            this._selectedFloorIds = new HashSet<Guid>(this.CalendarState.FilterSelectedFloorIds);
            this._selectedResourceIds = new HashSet<Guid>(this.CalendarState.FilterSelectedResourceIds);
            this._requiredCapacity = this.CalendarState.FilterRequiredCapacity;
        }
    }

    private void ToggleDay(int dayIndex)
    {
        if (!this._selectedDays.Remove(dayIndex))
            this._selectedDays.Add(dayIndex);
    }

    private void ToggleFloor(Guid id)
    {
        if (!this._selectedFloorIds.Remove(id))
            this._selectedFloorIds.Add(id);
    }

    private void ToggleResource(Guid id)
    {
        if (!this._selectedResourceIds.Remove(id))
            this._selectedResourceIds.Add(id);
    }

    private void HandleCapacityChange(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var val))
            this._requiredCapacity = val;
    }

    private void ClearFilters()
    {
        this._selectedDays.Clear();
        this._selectedFloorIds.Clear();
        this._selectedResourceIds.Clear();
        this._requiredCapacity = 1;
    }

    private async Task ApplyFilters()
    {
        // CalendarState に反映
        this.CalendarState.FilterSelectedDays = new HashSet<int>(this._selectedDays);
        this.CalendarState.FilterSelectedFloorIds = new HashSet<Guid>(this._selectedFloorIds);
        this.CalendarState.FilterSelectedResourceIds = new HashSet<Guid>(this._selectedResourceIds);
        this.CalendarState.FilterRequiredCapacity = this._requiredCapacity;

        var filter = new SearchFilterPanel.SearchFilter
        {
            SelectedDays = this._selectedDays.ToList(),
            SelectedFloorIds = this._selectedFloorIds.ToList(),
            SelectedResourceIds = this._selectedResourceIds.ToList(),
            RequiredCapacity = this._requiredCapacity
        };

        await this.OnApply.InvokeAsync(filter);
        await this.OnClose.InvokeAsync();
    }

    private async Task HandleClose()
    {
        await this.OnClose.InvokeAsync();
    }
}
