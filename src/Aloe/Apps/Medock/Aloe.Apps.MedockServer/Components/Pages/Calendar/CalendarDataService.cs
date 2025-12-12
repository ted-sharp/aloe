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
    private readonly IEquipmentService _equipmentService;
    private readonly IFacilityService _facilityService;
    private readonly AuthenticationStateProvider _authStateProvider;

    public CalendarDataService(
        IAppointmentService appointmentService,
        IEquipmentService equipmentService,
        IFacilityService facilityService,
        AuthenticationStateProvider authStateProvider)
    {
        this._appointmentService = appointmentService;
        this._equipmentService = equipmentService;
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
    /// </summary>
    public async Task GenerateFilterOptionsAsync(CalendarState state)
    {
        try
        {
            var authState = await this._authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? false)
            {
                state.AvailableEquipments = [];
                return;
            }

            var tenantIdClaim = user.FindFirst("tenant_id")?.Value;
            if (String.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                state.AvailableEquipments = [];
                return;
            }

            var equipments = await this._equipmentService.GetEquipmentsByTenantAsync(tenantId);
            state.AvailableEquipments = equipments.Select(e => new SearchFilterPanel.FilterItem
            {
                Id = e.EquipId,
                Name = e.EquipName
            }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"設備データ取得エラー: {ex.Message}");
            state.AvailableEquipments = [];
        }
    }
}
