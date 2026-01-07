using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.Dtos;
using Aloe.Apps.MedockLib.Services.Dtos.Appointments;
using Aloe.Apps.MedockServer.Components.Pages;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Aloe.Apps.MedockServer.ApplicationServices.Calendar.DataLoaders;

/// <summary>
/// 予約データおよび営業時間のロード処理を担当するサービス
/// </summary>
public class AppointmentLoader : IAppointmentLoader
{
    private readonly IAppointmentService _appointmentService;
    private readonly IFacilityService _facilityService;
    private readonly ILogger<AppointmentLoader> _logger;

    public AppointmentLoader(
        IAppointmentService appointmentService,
        IFacilityService facilityService,
        ILogger<AppointmentLoader> logger)
    {
        this._appointmentService = appointmentService;
        this._facilityService = facilityService;
        this._logger = logger;
    }

    /// <inheritdoc />
    public async Task LoadBusinessHoursAsync(CalendarState state)
    {
        try
        {
            if (state.CurrentFacilityId.HasValue)
            {
                var result = await this._facilityService.GetBusinessHoursAsync(
                    state.CurrentFacilityId.Value,
                    state.CurrentDate);

                if (result.IsSuccess && result.Value != null)
                {
                    state.BusinessHours = result.Value;
                    state.StartHour = state.BusinessHours.StartHour;
                    state.EndHour = state.BusinessHours.EndHour;
                }
                else
                {
                    this._logger.LogWarning("Failed to load business hours: {ErrorMessage}", result.ErrorMessage);
                    state.BusinessHours = new BusinessHoursDto();
                }
            }
            else
            {
                state.BusinessHours = new BusinessHoursDto();
            }
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error loading business hours for facility {FacilityId} on date {Date}",
                state.CurrentFacilityId, state.CurrentDate);
            state.BusinessHours = new BusinessHoursDto();
        }
    }

    /// <inheritdoc />
    public async Task LoadAppointmentsAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            // 【パフォーマンス対策】Month/Year viewではアポイントメント詳細は不要（mainStats/equipmentStatsで統計表示）
            // Week viewのみアポイントメント詳細が必要（個別予約ブロック表示）
            if (viewType == CalendarViewType.Year || viewType == CalendarViewType.Month)
            {
                this._logger.LogInformation("[TRACE] LoadAppointmentsAsync SKIPPED: ViewType={ViewType}, CurrentDate={Date} (appointments not used - mainStats/equipmentStats only)",
                    viewType, currentDate);
                state.Appointments = new List<AppointmentDto>();
                this._logger.LogWarning("[TRACE] LoadAppointmentsAsync: {ViewType} ビューのため Appointments をクリアしました", viewType);
                return;
            }

            this._logger.LogInformation("[TRACE] LoadAppointmentsAsync: {ViewType} ビューなのでデータをロードします", viewType);

            var (startDate, endDate) = GetDateRange(viewType, currentDate, weekDays);
            this._logger.LogInformation("[TRACE] LoadAppointmentsAsync start: ViewType={ViewType}, CurrentDate={CurrentDate}, DateRange={StartDate:yyyy-MM-dd}~{EndDate:yyyy-MM-dd}",
                viewType, currentDate, startDate, endDate);

            var querySw = Stopwatch.StartNew();
            var result = await this._appointmentService.GetAppointmentsAsync(startDate, endDate);
            querySw.Stop();
            this._logger.LogInformation("[PERF] LoadAppointmentsAsync - Repository query: {ElapsedMs}ms",
                querySw.ElapsedMilliseconds);

            if (!result.IsSuccess || result.Value == null)
            {
                this._logger.LogWarning("Failed to load appointments: {ErrorMessage}", result.ErrorMessage);
                state.Appointments = [];
                return;
            }

            var appointments = result.Value;
            this._logger.LogInformation("[PERF] LoadAppointmentsAsync - Retrieved {Count} appointments from service",
                appointments.Count);

            // フロアフィルターを適用
            var filterSw = Stopwatch.StartNew();
            this._logger.LogWarning("[TRACE] LoadAppointmentsAsync - CurrentFilter: {Filter}, SelectedFloorIds: {FloorCount}",
                state.CurrentFilter != null ? "set" : "null",
                state.CurrentFilter?.SelectedFloorIds.Count ?? 0);
            if (state.CurrentFilter != null && state.CurrentFilter.SelectedFloorIds.Any())
            {
                this._logger.LogWarning("[TRACE] LoadAppointmentsAsync - フロアフィルター適用前: {Count}件", appointments.Count);
                appointments = appointments
                    .Where(a => a.FloorId.HasValue && state.CurrentFilter.SelectedFloorIds.Contains(a.FloorId.Value))
                    .ToList();
                this._logger.LogWarning("[TRACE] LoadAppointmentsAsync - フロアフィルター適用後: {Count}件", appointments.Count);
            }
            filterSw.Stop();
            this._logger.LogInformation("[PERF] LoadAppointmentsAsync - Floor filtering: {ElapsedMs}ms, Count after filter={Count}",
                filterSw.ElapsedMilliseconds, appointments.Count);

            state.Appointments = appointments;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error loading appointments");
            state.Appointments = [];
        }
        finally
        {
            sw.Stop();
            this._logger.LogInformation("[PERF] LoadAppointmentsAsync - Total: {ElapsedMs}ms", sw.ElapsedMilliseconds);
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
                currentDate,
                currentDate.AddDays(weekDays - 1)
            ),
            _ => (currentDate, currentDate)
        };
    }
}
