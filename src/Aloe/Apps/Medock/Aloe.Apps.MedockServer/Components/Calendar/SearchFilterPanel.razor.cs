using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockServer.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class SearchFilterPanel : ComponentBase
{
    // EquipmentはAppointmentResourceに統合されました
    // AvailableEquipmentsパラメータは削除されました

    [Inject]
    private CalendarState CalendarState { get; set; } = default!;

    [Inject]
    private ICalendarResourceFilterService CalendarResourceFilterService { get; set; } = default!;

    [Inject]
    private IDbContextFactory<MedockDbContext> DbContextFactory { get; set; } = default!;

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
    private HashSet<Guid> SelectedResourceIds => this.CalendarState.FilterSelectedResourceIds;
    private HashSet<Guid> SelectedPlanIds => this.CalendarState.FilterSelectedPlanIds;

    // 選択肢データ（外部から注入）
    [Parameter]
    public List<FilterItem>? AvailableFloors { get; set; }

    [Parameter]
    public List<FilterItem>? AvailableResources { get; set; }

    [Parameter]
    public List<FilterItem>? AvailablePlans { get; set; }

    // 定数
    private readonly string[] DayNames = { "日", "月", "火", "水", "木", "金", "土" };
    private readonly string[] TimeSlots = TimeSlotConstants.FilterTimeSlots;

    // 選択されているPlanに関連するオプションIDのキャッシュ
    private HashSet<Guid> _relatedOptionIds = new();

    private int ActiveFilterCount =>
        (this.SelectedDays.Any() ? 1 : 0) +
        (this.SelectedTimeSlots.Any() ? 1 : 0) +
        (this.RequiredCapacity > 1 ? 1 : 0) +
        (this.SelectedFloorIds.Any() ? 1 : 0) +
        (this.SelectedResourceIds.Any() ? 1 : 0) +
        (this.SelectedPlanIds.Any() ? 1 : 0);

    private async Task HandleClose()
    {
        await this.OnClose.InvokeAsync();
    }

    // ToggleEquipmentメソッドは削除されました（EquipmentはAppointmentResourceに統合）

    private void ClearFilters()
    {
        this.CalendarState.ClearFilterSelections();
        this._relatedOptionIds.Clear(); // 関連オプションIDのキャッシュをクリア
        this.OnFilterChanged();
        this.StateHasChanged(); // UIを更新してオプションの選択可否状態を反映
    }

    private async void OnFilterChanged()
    {
        // リアルタイム検索
        var filter = await this.BuildFilterAsync();
        await this.OnFilterChangedRealtime.InvokeAsync(filter);
    }

    private async Task<SearchFilter> BuildFilterAsync()
    {
        // ユーザーが手動で選択したリソースID
        var selectedResourceIds = this.SelectedResourceIds.ToList();

        // フロア/プランが選択されている場合、それらに関連するリソースIDも追加
        var autoResourceIds = await this.GetAutoResourceIdsAsync();

        // 手動選択と自動選択をマージ（重複除去）
        var allResourceIds = selectedResourceIds.Union(autoResourceIds).ToList();

        return new SearchFilter
        {
            SelectedDays = this.SelectedDays.ToList(),
            TimeSlots = this.SelectedTimeSlots.ToList(),
            RequiredCapacity = this.RequiredCapacity,
            SelectedFloorIds = this.SelectedFloorIds.ToList(),
            SelectedResourceIds = allResourceIds,
            SelectedPlanIds = this.SelectedPlanIds.ToList()
        };
    }

    /// <summary>
    /// 選択されているフロア/プランに関連するリソースIDを取得
    /// </summary>
    private async Task<List<Guid>> GetAutoResourceIdsAsync()
    {
        return await this.CalendarResourceFilterService.GetRelatedResourceIdsAsync(
            this.SelectedFloorIds,
            this.SelectedPlanIds);
    }

    /// <summary>
    /// 選択されたPlanに関連するオプションIDを取得
    /// </summary>
    private async Task<List<Guid>> GetRelatedOptionIdsAsync(Guid planId)
    {
        try
        {
            using var context = this.DbContextFactory.CreateDbContext();
            var optionIds = await context.PlanOptions
                .AsNoTracking()
                .Where(po => po.PlanId == planId && !po.IsDeleted)
                .Select(po => po.OptionPlanId)
                .ToListAsync();
            return optionIds;
        }
        catch
        {
            return new List<Guid>();
        }
    }

    /// <summary>
    /// オプションが選択可能かどうかを判定（選択されているPlanに関連するオプションか）
    /// </summary>
    private bool IsOptionSelectable(FilterItem option)
    {
        if (option.PlanTypeCode != 2)
        {
            return true; // Option以外は常に選択可能
        }

        // 選択されたPlan（PlanTypeCode=1）が存在するか確認
        if (this.AvailablePlans == null)
        {
            return false;
        }

        var selectedPlan = this.AvailablePlans
            .FirstOrDefault(p => p.PlanTypeCode == 1 && this.SelectedPlanIds.Contains(p.Id));
        
        if (selectedPlan == null)
        {
            return false; // Planが選択されていない場合は選択不可
        }

        // 選択されているPlanに関連するオプションかどうかを確認
        return this._relatedOptionIds.Contains(option.Id);
    }

    /// <summary>
    /// プランのバッジクラスを取得
    /// </summary>
    private string GetPlanBadgeClass(FilterItem plan, bool isSelected)
    {
        if (plan.PlanTypeCode == 1)
        {
            // Plan: 選択時はprimary、未選択時はoutline
            return isSelected ? "badge-primary" : "badge-outline";
        }
        else if (plan.PlanTypeCode == 2)
        {
            // Option: 選択時はsecondary、未選択時はoutline
            if (isSelected)
            {
                return "badge-secondary";
            }
            else
            {
                // 選択不可の場合はghost
                if (!this.IsOptionSelectable(plan))
                {
                    return "badge-ghost opacity-50";
                }
                return "badge-outline";
            }
        }
        return "badge-outline";
    }

    // SelectAllEquipments/ClearAllEquipmentsメソッドは削除されました（EquipmentはAppointmentResourceに統合）
}



