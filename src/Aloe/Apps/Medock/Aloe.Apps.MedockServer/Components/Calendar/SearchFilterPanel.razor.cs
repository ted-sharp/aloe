using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockServer.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class SearchFilterPanel : ComponentBase
{
    // EquipmentはAppointmentResourceに統合されました
    // AvailableEquipmentsパラメータは削除されました

    [Inject]
    private CalendarState CalendarState { get; set; } = default!;

    [Inject]
    private IDbContextFactory<MedockDbContext> DbContextFactory { get; set; } = default!;

    [Inject]
    private ILogger<SearchFilterPanel> Logger { get; set; } = default!;

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
        var filter = await this.BuildFilterAsync();
        await this.OnFilterChangedRealtime.InvokeAsync(filter);
    }

    private async Task<SearchFilter> BuildFilterAsync()
    {
        // ユーザーが手動で選択したリソースID
        var selectedResourceIds = this.SelectedResourceIds.ToList();

        // リソースグループ/プラン/オプションが選択されている場合、それらに関連するリソースIDも追加
        var autoResourceIds = await this.GetAutoResourceIdsAsync();

        // 手動選択と自動選択をマージ（重複除去）
        var allResourceIds = selectedResourceIds.Union(autoResourceIds).ToList();

        return new SearchFilter
        {
            SelectedDays = this.SelectedDays.ToList(),
            TimeSlots = this.SelectedTimeSlots.ToList(),
            RequiredCapacity = this.RequiredCapacity,
            SelectedFloorIds = this.SelectedFloorIds.ToList(),
            SelectedResourceGroupIds = this.SelectedResourceGroupIds.ToList(),
            SelectedResourceIds = allResourceIds,
            SelectedPlanIds = this.SelectedPlanIds.ToList(),
            SelectedOptionPlanIds = this.SelectedOptionPlanIds.ToList()
        };
    }

    /// <summary>
    /// 選択されているリソースグループ/プラン/オプションに関連するリソースIDを取得
    /// </summary>
    private async Task<List<Guid>> GetAutoResourceIdsAsync()
    {
        var autoResourceIds = new HashSet<Guid>();

        try
        {
            await using var context = await this.DbContextFactory.CreateDbContextAsync();

            // 選択されているフロアに属するリソース
            if (this.SelectedFloorIds.Any())
            {
                var floorResourceIds = await context.AppointmentResources
                    .AsNoTracking()
                    .Where(r => !r.IsDeleted &&
                               this.SelectedFloorIds.Contains(r.FloorId) &&
                               r.ApptResTypeCode == (int)AppointmentResourceType.Equipment)
                    .Select(r => r.ApptResId)
                    .Distinct()
                    .ToListAsync();

                foreach (var resourceId in floorResourceIds)
                {
                    autoResourceIds.Add(resourceId);
                }
            }

            // 選択されているリソースグループに関連するリソース
            if (this.SelectedResourceGroupIds.Any())
            {
                var groupResourceIds = await context.AppointmentResourceGroupMembers
                    .AsNoTracking()
                    .Where(m => !m.IsDeleted &&
                               this.SelectedResourceGroupIds.Contains(m.ApptResGroupId) &&
                               m.AppointmentResource.ApptResTypeCode == (int)AppointmentResourceType.Equipment &&
                               !m.AppointmentResource.IsDeleted)
                    .Select(m => m.ApptResId)
                    .Distinct()
                    .ToListAsync();

                foreach (var resourceId in groupResourceIds)
                {
                    autoResourceIds.Add(resourceId);
                }
            }

            // 選択されているプランに関連するリソース
            if (this.SelectedPlanIds.Any())
            {
                var planResourceIds = await context.PlanResourceRequirements
                    .AsNoTracking()
                    .Where(prr => !prr.IsDeleted &&
                                 this.SelectedPlanIds.Contains(prr.PlanId) &&
                                 prr.AppointmentResource.ApptResTypeCode == (int)AppointmentResourceType.Equipment &&
                                 !prr.AppointmentResource.IsDeleted)
                    .Select(prr => prr.ApptResId)
                    .Distinct()
                    .ToListAsync();

                foreach (var resourceId in planResourceIds)
                {
                    autoResourceIds.Add(resourceId);
                }
            }

            // 選択されているオプションに関連するリソース
            if (this.SelectedOptionPlanIds.Any())
            {
                var optionResourceIds = await context.PlanResourceRequirements
                    .AsNoTracking()
                    .Where(prr => !prr.IsDeleted &&
                                 this.SelectedOptionPlanIds.Contains(prr.PlanId) &&
                                 prr.AppointmentResource.ApptResTypeCode == (int)AppointmentResourceType.Equipment &&
                                 !prr.AppointmentResource.IsDeleted)
                    .Select(prr => prr.ApptResId)
                    .Distinct()
                    .ToListAsync();

                foreach (var resourceId in optionResourceIds)
                {
                    autoResourceIds.Add(resourceId);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting automatic resource IDs");
        }

        return autoResourceIds.ToList();
    }

    // SelectAllEquipments/ClearAllEquipmentsメソッドは削除されました（EquipmentはAppointmentResourceに統合）
}



