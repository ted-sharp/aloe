using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockServer.Components.Calendar;
using Aloe.Apps.MedockServer.Components.FAB;
using Aloe.Apps.MedockServer.Components.Pages;
using Microsoft.AspNetCore.Components.Authorization;

namespace Aloe.Apps.MedockServer.Components.Pages;

/// <summary>
/// カレンダーページのデータロード処理サービス
/// </summary>
public class CalendarDataService
{
    private readonly IAppointmentService _appointmentService;
    private readonly IFacilityService _facilityService;
    private readonly AuthenticationStateProvider _authStateProvider;

    public CalendarDataService(
        IAppointmentService appointmentService,
        IFacilityService facilityService,
        AuthenticationStateProvider authStateProvider)
    {
        this._appointmentService = appointmentService;
        this._facilityService = facilityService;
        this._authStateProvider = authStateProvider;
    }

    /// <summary>
    /// 営業時間をロードして状態に反映します。
    /// </summary>
    public async Task LoadBusinessHoursAsync(CalendarState state)
    {
        try
        {
            if (state.CurrentFacilityId.HasValue)
            {
                state.BusinessHours = await this._facilityService.GetBusinessHoursAsync(
                    state.CurrentFacilityId.Value,
                    state.CurrentDate);

                state.StartHour = state.BusinessHours.StartHour;
                state.EndHour = state.BusinessHours.EndHour;
            }
            else
            {
                state.BusinessHours = new BusinessHoursDto();
            }
        }
        catch (Exception)
        {
            state.BusinessHours = new BusinessHoursDto();
        }
    }

    /// <summary>
    /// 休日情報をロードして状態に反映します。
    /// </summary>
    public async Task LoadHolidaysAsync(CalendarState state)
    {
        try
        {
            var startDate = new DateOnly(state.CurrentDate.Year, 1, 1);
            var endDate = new DateOnly(state.CurrentDate.Year, 12, 31);
            var holidays = await this._appointmentService.GetHolidaysAsync(startDate, endDate);

            state.Holidays = holidays.ToDictionary(
                h => h.Date.ToString("yyyy-MM-dd"),
                h => h.Name
            );
        }
        catch (Exception)
        {
            state.Holidays = [];
        }
    }

    /// <summary>
    /// フィルター用の設備オプションを生成して状態に反映します。
    /// EquipmentはAppointmentResourceに統合されました。
    /// </summary>
    public async Task GenerateFilterOptionsAsync(CalendarState state)
    {
        // EquipmentはAppointmentResourceに統合されました
        // このメソッドは将来AppointmentResource用に実装されます
        await Task.CompletedTask;
    }

    /// <summary>
    /// 期間に応じた予約データを取得して状態に反映します。
    /// </summary>
    public async Task LoadAppointmentsAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7)
    {
        try
        {
            var (startDate, endDate) = GetDateRange(viewType, currentDate, weekDays);
            var appointments = await this._appointmentService.GetAppointmentsAsync(startDate, endDate);
            state.Appointments = appointments;
        }
        catch (Exception)
        {
            state.Appointments = [];
        }
    }

    /// <summary>
    /// ビューと日付に基づいて取得期間を計算します。
    /// </summary>
    private static (DateOnly StartDate, DateOnly EndDate) GetDateRange(
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays)
    {
        return viewType switch
        {
            CalendarViewType.Year => (
                new DateOnly(currentDate.Year, 1, 1),
                new DateOnly(currentDate.Year, 12, 31)
            ),
            CalendarViewType.Month => (
                new DateOnly(currentDate.Year, currentDate.Month, 1),
                new DateOnly(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month))
            ),
            CalendarViewType.Week => (
                currentDate.AddDays(-((int)currentDate.DayOfWeek)),
                currentDate.AddDays(-((int)currentDate.DayOfWeek) + weekDays - 1)
            ),
            _ => (currentDate, currentDate)
        };
    }
}
