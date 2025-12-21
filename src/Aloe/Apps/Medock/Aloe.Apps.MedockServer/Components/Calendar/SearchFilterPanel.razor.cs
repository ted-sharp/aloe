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
    private HashSet<Guid> SelectedPlanIds { get; set; } = new(); // プラン選択
    private HashSet<Guid> SelectedOptionPlanIds { get; set; } = new(); // オプション（プラン）選択

    // 選択肢データ（外部から注入）
    [Parameter]
    public List<FilterItem>? AvailableFloors { get; set; }

    [Parameter]
    public List<FilterItem>? AvailableResourceGroups { get; set; }

    [Parameter]
    public List<FilterItem>? AvailablePlans { get; set; }

    [Parameter]
    public List<FilterItem>? AvailableOptions { get; set; }

    // 定数
    private readonly string[] DayNames = { "日", "月", "火", "水", "木", "金", "土" };
    private readonly string[] TimeSlots = { "08:00", "09:00", "10:00", "11:00", "13:00", "14:00", "15:00", "16:00" };

    private int ActiveFilterCount =>
        (this.SelectedDays.Any() ? 1 : 0) +
        (this.SelectedTimeSlots.Any() ? 1 : 0) +
        (this.RequiredCapacity > 1 ? 1 : 0) +
        (this.SelectedFloorIds.Any() ? 1 : 0) +
        (this.SelectedResourceGroupIds.Any() ? 1 : 0) +
        (this.SelectedPlanIds.Any() ? 1 : 0) +
        (this.SelectedOptionPlanIds.Any() ? 1 : 0);

    private async Task HandleClose()
    {
        await this.OnClose.InvokeAsync();
    }

    private void ToggleDay(int dayIndex)
    {
        if (this.SelectedDays.Contains(dayIndex))
        {
            this.SelectedDays.Remove(dayIndex);
        }
        else
        {
            this.SelectedDays.Add(dayIndex);
        }
        this.OnFilterChanged();
    }

    // ToggleEquipmentメソッドは削除されました（EquipmentはAppointmentResourceに統合）

    private void ToggleTimeSlot(string timeSlot)
    {
        if (this.SelectedTimeSlots.Contains(timeSlot))
        {
            this.SelectedTimeSlots.Remove(timeSlot);
        }
        else
        {
            this.SelectedTimeSlots.Add(timeSlot);
        }
        this.OnFilterChanged();
    }

    private void ClearFilters()
    {
        this.SelectedDays = new();
        this.SelectedTimeSlots.Clear();
        this.RequiredCapacity = 1;
        this.SelectedFloorIds.Clear();
        this.SelectedResourceGroupIds.Clear();
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
            SelectedPlanIds = this.SelectedPlanIds.ToList(),
            SelectedOptionPlanIds = this.SelectedOptionPlanIds.ToList()
        };
    }

    private void ToggleFloor(Guid floorId)
    {
        if (this.SelectedFloorIds.Contains(floorId))
        {
            this.SelectedFloorIds.Remove(floorId);
        }
        else
        {
            this.SelectedFloorIds.Add(floorId);
        }
        this.OnFilterChanged();
    }

    private void ToggleResourceGroup(Guid groupId)
    {
        if (this.SelectedResourceGroupIds.Contains(groupId))
        {
            this.SelectedResourceGroupIds.Remove(groupId);
        }
        else
        {
            this.SelectedResourceGroupIds.Add(groupId);
        }
        this.OnFilterChanged();
    }

    private void TogglePlan(Guid planId)
    {
        if (this.SelectedPlanIds.Contains(planId))
        {
            this.SelectedPlanIds.Remove(planId);
        }
        else
        {
            this.SelectedPlanIds.Add(planId);
        }
        this.OnFilterChanged();
    }

    private void ToggleOption(Guid optionPlanId)
    {
        if (this.SelectedOptionPlanIds.Contains(optionPlanId))
        {
            this.SelectedOptionPlanIds.Remove(optionPlanId);
        }
        else
        {
            this.SelectedOptionPlanIds.Add(optionPlanId);
        }
        this.OnFilterChanged();
    }

    private void SelectAllFloors()
    {
        if (this.AvailableFloors != null)
        {
            this.SelectedFloorIds = this.AvailableFloors.Select(f => f.Id).ToHashSet();
        }
        this.OnFilterChanged();
    }

    private void ClearAllFloors()
    {
        this.SelectedFloorIds.Clear();
        this.OnFilterChanged();
    }

    private void SelectAllResourceGroups()
    {
        if (this.AvailableResourceGroups != null)
        {
            this.SelectedResourceGroupIds = this.AvailableResourceGroups.Select(g => g.Id).ToHashSet();
        }
        this.OnFilterChanged();
    }

    private void ClearAllResourceGroups()
    {
        this.SelectedResourceGroupIds.Clear();
        this.OnFilterChanged();
    }

    private void SelectAllPlans()
    {
        if (this.AvailablePlans != null)
        {
            this.SelectedPlanIds = this.AvailablePlans.Select(p => p.Id).ToHashSet();
        }
        this.OnFilterChanged();
    }

    private void ClearAllPlans()
    {
        this.SelectedPlanIds.Clear();
        this.OnFilterChanged();
    }

    private void SelectAllOptions()
    {
        if (this.AvailableOptions != null)
        {
            this.SelectedOptionPlanIds = this.AvailableOptions.Select(o => o.Id).ToHashSet();
        }
        this.OnFilterChanged();
    }

    private void ClearAllOptions()
    {
        this.SelectedOptionPlanIds.Clear();
        this.OnFilterChanged();
    }

    /// <summary>
    /// フィルター選択肢の項目
    /// </summary>
    public class FilterItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = String.Empty;
    }

    private void SelectAllDays()
    {
        this.SelectedDays = Enumerable.Range(0, 7).ToHashSet();
        this.OnFilterChanged();
    }

    private void ClearAllDays()
    {
        this.SelectedDays.Clear();
        this.OnFilterChanged();
    }

    private void SelectAllTimeSlots()
    {
        this.SelectedTimeSlots = this.TimeSlots.ToHashSet();
        this.OnFilterChanged();
    }

    private void ClearAllTimeSlots()
    {
        this.SelectedTimeSlots.Clear();
        this.OnFilterChanged();
    }

    // SelectAllEquipments/ClearAllEquipmentsメソッドは削除されました（EquipmentはAppointmentResourceに統合）

    /// <summary>
    /// 検索フィルターのデータ
    /// </summary>
    public class SearchFilter
    {
        public List<int> SelectedDays { get; set; } = new();
        public List<string> TimeSlots { get; set; } = new();
        public int RequiredCapacity { get; set; } = 1;
        public List<Guid> SelectedFloorIds { get; set; } = new();
        public List<Guid> SelectedResourceGroupIds { get; set; } = new();
        public List<Guid> SelectedPlanIds { get; set; } = new();
        public List<Guid> SelectedOptionPlanIds { get; set; } = new();

        /// <summary>
        /// フィルターが有効かどうか
        /// </summary>
        public bool IsActive =>
            this.SelectedDays.Any() ||
            this.TimeSlots.Any() ||
            this.RequiredCapacity > 1 ||
            this.SelectedFloorIds.Any() ||
            this.SelectedResourceGroupIds.Any() ||
            this.SelectedPlanIds.Any() ||
            this.SelectedOptionPlanIds.Any();
    }
}



