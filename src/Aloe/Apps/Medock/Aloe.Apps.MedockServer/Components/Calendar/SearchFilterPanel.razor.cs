using Aloe.Apps.MedockLib.Constants;
using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class SearchFilterPanel : ComponentBase
{
    // EquipmentはAppointmentResourceに統合されました
    // AvailableEquipmentsパラメータは削除されました

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

    // フィルター値
    private HashSet<int> SelectedDays { get; set; } = new(); // デフォルト全オフ
    private HashSet<string> SelectedTimeSlots { get; set; } = new(); // 時間帯複数選択
    private int RequiredCapacity { get; set; } = 1;
    private HashSet<Guid> SelectedFloorIds { get; set; } = new(); // フロア選択
    private HashSet<Guid> SelectedResourceGroupIds { get; set; } = new(); // リソースグループ選択
    private HashSet<Guid> SelectedResourceIds { get; set; } = new(); // リソース選択
    private HashSet<Guid> SelectedPlanIds { get; set; } = new(); // プラン選択
    private HashSet<Guid> SelectedOptionPlanIds { get; set; } = new(); // オプション（プラン）選択

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
        this.SelectedDays = new();
        this.SelectedTimeSlots.Clear();
        this.RequiredCapacity = 1;
        this.SelectedFloorIds.Clear();
        this.SelectedResourceGroupIds.Clear();
        this.SelectedResourceIds.Clear();
        this.SelectedPlanIds.Clear();
        this.SelectedOptionPlanIds.Clear();
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



