using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockServer.Components.Pages;
using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class SearchFilterPanel : ComponentBase
{
    // EquipmentはAppointmentResourceに統合されました
    // AvailableEquipmentsパラメータは削除されました

    [Inject]
    private CalendarState CalendarState { get; set; } = default!;

    /// <summary>
    /// フィルター変更時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<SearchFilter> OnFilterApplied { get; set; }

    /// <summary>
    /// リアルタイム検索用のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<SearchFilter> OnFilterChangedRealtime { get; set; }

    /// <summary>
    /// パネルを閉じる時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    // フィルター値（CalendarState から参照 - セッション中保持）
    private HashSet<int> SelectedDays => this.CalendarState.FilterSelectedDays;
    private HashSet<string> SelectedTimeSlots => this.CalendarState.FilterSelectedTimeSlots;
    private int RequiredCapacity
    {
        get => this.CalendarState.FilterRequiredCapacity;
        set => this.CalendarState.FilterRequiredCapacity = value;
    }
    private HashSet<Guid> SelectedFloorIds => this.CalendarState.FilterSelectedFloorIds;
    private HashSet<Guid> SelectedResourceGroupIds => this.CalendarState.FilterSelectedResourceGroupIds;
    private HashSet<Guid> SelectedResourceIds => this.CalendarState.FilterSelectedResourceIds;
    private HashSet<Guid> SelectedPlanIds => this.CalendarState.FilterSelectedPlanIds;
    private HashSet<Guid> SelectedOptionPlanIds => this.CalendarState.FilterSelectedOptionPlanIds;

    // 選択肢データ（外部から注入）
    [Parameter]
    public List<FilterItem>? AvailableFloors { get; set; }

    [Parameter]
    public List<FilterItem>? AvailableResourceGroups { get; set; }

    [Parameter]
    public List<FilterItem>? AvailableResources { get; set; }

    [Parameter]
    public List<FilterItem>? AvailablePlans { get; set; }

    [Parameter]
    public List<FilterItem>? AvailableOptions { get; set; }

    // 定数
    private readonly string[] DayNames = { "日", "月", "火", "水", "木", "金", "土" };
    private readonly string[] TimeSlots = TimeSlotConstants.FilterTimeSlots;

    private int ActiveFilterCount =>
        (this.SelectedDays.Any() ? 1 : 0) +
        (this.SelectedTimeSlots.Any() ? 1 : 0) +
        (this.RequiredCapacity > 1 ? 1 : 0) +
        (this.SelectedFloorIds.Any() ? 1 : 0) +
        (this.SelectedResourceGroupIds.Any() ? 1 : 0) +
        (this.SelectedResourceIds.Any() ? 1 : 0) +
        (this.SelectedPlanIds.Any() ? 1 : 0) +
        (this.SelectedOptionPlanIds.Any() ? 1 : 0);

    private async Task HandleClose()
    {
        await this.OnClose.InvokeAsync();
    }

    // ToggleEquipmentメソッドは削除されました（EquipmentはAppointmentResourceに統合）

    private void ClearFilters()
    {
        this.CalendarState.ClearFilterSelections();
        this.OnFilterChanged();
    }

    private async void OnFilterChanged()
    {
        // リアルタイム検索
        var filter = this.BuildFilter();
        await this.OnFilterChangedRealtime.InvokeAsync(filter);
    }

    private SearchFilter BuildFilter()
    {
        return new SearchFilter
        {
            SelectedDays = this.SelectedDays.ToList(),
            TimeSlots = this.SelectedTimeSlots.ToList(),
            RequiredCapacity = this.RequiredCapacity,
            SelectedFloorIds = this.SelectedFloorIds.ToList(),
            SelectedResourceGroupIds = this.SelectedResourceGroupIds.ToList(),
            SelectedResourceIds = this.SelectedResourceIds.ToList(),
            SelectedPlanIds = this.SelectedPlanIds.ToList(),
            SelectedOptionPlanIds = this.SelectedOptionPlanIds.ToList()
        };
    }

    // SelectAllEquipments/ClearAllEquipmentsメソッドは削除されました（EquipmentはAppointmentResourceに統合）
}



