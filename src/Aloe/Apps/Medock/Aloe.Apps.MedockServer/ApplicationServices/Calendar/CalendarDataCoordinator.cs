using Aloe.Apps.MedockServer.Components.Pages;

namespace Aloe.Apps.MedockServer.ApplicationServices.Calendar;

/// <summary>
/// カレンダーデータロード処理の調整クラス
/// </summary>
public class CalendarDataCoordinator
{
    private readonly CalendarApplicationService _dataService;

    public CalendarDataCoordinator(CalendarApplicationService dataService)
    {
        this._dataService = dataService;
    }

    /// <summary>
    /// カレンダーのすべてのデータを更新します
    /// </summary>
    public async Task RefreshCalendarDataAsync(
        CalendarState state,
        Func<Task> reapplyFilter)
    {
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

        await this._dataService.LoadAppointmentsAsync(
            state,
            state.CurrentView,
            state.CurrentDate,
            state.WeekDays);

        await this._dataService.LoadHolidaysAsync(
            state,
            state.CurrentView,
            state.CurrentDate,
            state.WeekDays);

        await reapplyFilter();
    }
}
