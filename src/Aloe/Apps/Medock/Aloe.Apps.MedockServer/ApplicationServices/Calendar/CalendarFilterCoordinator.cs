using Aloe.Apps.MedockServer.Components.Pages;
using Aloe.Apps.MedockServer.Components.Calendar;

namespace Aloe.Apps.MedockServer.ApplicationServices.Calendar;

/// <summary>
/// カレンダーのフィルター処理を調整するクラス
/// </summary>
public class CalendarFilterCoordinator
{
    private readonly CalendarFilterService _filterService;
    private readonly CalendarApplicationService _dataService;

    public CalendarFilterCoordinator(
        CalendarFilterService filterService,
        CalendarApplicationService dataService)
    {
        this._filterService = filterService;
        this._dataService = dataService;
    }

    /// <summary>
    /// フィルターが適用されたときの処理
    /// </summary>
    public async Task HandleFilterAppliedAsync(
        SearchFilterPanel.SearchFilter filter,
        CalendarState state,
        Func<Task> onFilterApplied)
    {
        state.CurrentFilter = filter;
        await this.ReapplyCurrentFilterAsync(state);
        await onFilterApplied();
    }

    /// <summary>
    /// 現在のフィルターを再適用する（月切り替えやビュー変更後に呼び出す）
    /// </summary>
    public async Task ReapplyCurrentFilterAsync(CalendarState state)
    {
        if (state.CurrentFilter is null)
        {
            return;
        }

        await this._filterService.ApplyFilterAsync(
            state.CurrentFilter,
            state.MainStats,
            state.MainStatsGrayedOut,
            state.CurrentView,
            state.CurrentDate);
    }

    /// <summary>
    /// フィルターが リアルタイムで変更されたときの処理
    /// </summary>
    public async Task HandleFilterChangedRealtimeAsync(
        SearchFilterPanel.SearchFilter filter,
        CalendarState state,
        Func<Task> onDataRefreshed,
        Func<Task> onFilterApplied)
    {
        state.CurrentFilter = filter;

        // フロア、リソース、プランのフィルターが変更された場合はデータを再取得
        var needsReload = filter.SelectedFloorIds.Any() ||
                         filter.SelectedResourceIds.Any() ||
                         filter.SelectedPlanIds.Any();

        if (needsReload)
        {
            await this.RefreshAndReapplyFilterAsync(state, onDataRefreshed);
        }
        else
        {
            await this.ReapplyCurrentFilterAsync(state);
            await onFilterApplied();
        }
    }

    /// <summary>
    /// データを再取得してフィルターを再適用
    /// </summary>
    private async Task RefreshAndReapplyFilterAsync(
        CalendarState state,
        Func<Task> onDataRefreshed)
    {
        // データロード（この段階ではフィルター非適用）
        await this._dataService.LoadMainStatsAsync(
            state,
            state.CurrentView,
            state.CurrentDate,
            state.WeekDays);

        await this._dataService.LoadMainStatsSlotsAsync(
            state,
            state.CurrentView,
            state.CurrentDate,
            state.WeekDays);

        await this._dataService.LoadEquipmentStatsAsync(
            state,
            state.CurrentView,
            state.CurrentDate,
            state.WeekDays);

        await this.ReapplyCurrentFilterAsync(state);
        await onDataRefreshed();
    }
}
