using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockServer.Components.Pages;
using Aloe.Apps.MedockServer.Components.Calendar;
using Aloe.Apps.MedockServer.Components.FAB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockServer.ApplicationServices.Calendar.DataLoaders;

/// <summary>
/// カレンダーメタデータ（休日、フィルターオプション）のロード処理を担当するサービス
/// </summary>
public class MetadataLoader : IMetadataLoader
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDbContextFactory<MedockDbContext> _contextFactory;
    private readonly ILogger<MetadataLoader> _logger;

    public MetadataLoader(
        IAppointmentService appointmentService,
        IDbContextFactory<MedockDbContext> contextFactory,
        ILogger<MetadataLoader> logger)
    {
        this._appointmentService = appointmentService;
        this._contextFactory = contextFactory;
        this._logger = logger;
    }

    /// <inheritdoc />
    public async Task LoadHolidaysAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7)
    {
        try
        {
            // 表示範囲に基づいて祝日を取得
            var (startDate, endDate) = GetDateRange(viewType, currentDate, weekDays);
            var result = await this._appointmentService.GetHolidaysAsync(startDate, endDate);

            // 既存の祝日をクリアしてから新しいデータを設定
            state.Holidays.Clear();

            if (result.IsSuccess && result.Value != null)
            {
                state.Holidays = result.Value.ToDictionary(
                    h => h.Date.ToString("yyyy-MM-dd"),
                    h => h.Name
                );
            }
            else
            {
                this._logger.LogWarning("Failed to load holidays: {ErrorMessage}", result.ErrorMessage);
                state.Holidays.Clear();
            }
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error loading holidays for date range {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}",
                currentDate, currentDate.AddDays(weekDays));
            state.Holidays.Clear();
        }
    }

    /// <inheritdoc />
    public async Task LoadFilterOptionsAsync(CalendarState state)
    {
        try
        {
            if (!state.CurrentFacilityId.HasValue)
            {
                return;
            }

            using var context = this._contextFactory.CreateDbContext();
            var facilityId = state.CurrentFacilityId.Value;

            // フロアをロード
            var floors = await context.Floors
                .AsNoTracking()
                .Where(f => f.FacilityId == facilityId && !f.IsDeleted)
                .OrderBy(f => f.FloorSeq)
                .ThenBy(f => f.FloorCode)
                .Select(f => new SearchFilterPanel.FilterItem
                {
                    Id = f.FloorId,
                    Name = f.FloorName
                })
                .ToListAsync();
            state.AvailableFloors = floors;

            // リソースグループをロード
            var resourceGroups = await context.AppointmentResourceGroups
                .AsNoTracking()
                .Where(rg => rg.FacilityId == facilityId && !rg.IsDeleted)
                .OrderBy(rg => rg.ResGroupSeq)
                .ThenBy(rg => rg.ResGroupCode)
                .Select(rg => new SearchFilterPanel.FilterItem
                {
                    Id = rg.ApptResGroupId,
                    Name = rg.ResGroupName
                })
                .ToListAsync();
            state.AvailableResourceGroups = resourceGroups;

            // リソースをロード（Equipmentリソースのみ）
            var resources = await context.AppointmentResources
                .AsNoTracking()
                .Where(r => r.Floor.FacilityId == facilityId &&
                           !r.IsDeleted &&
                           r.ApptResTypeCode == (int)Aloe.Apps.MedockLib.Constants.AppointmentResourceType.Equipment)
                .OrderBy(r => r.ApptResSeq)
                .ThenBy(r => r.ApptResName)
                .Select(r => new SearchFilterPanel.FilterItem
                {
                    Id = r.ApptResId,
                    Name = r.ApptResName
                })
                .ToListAsync();
            state.AvailableResources = resources;

            // プランをロード（有効なもののみ）
            var today = DateOnly.FromDateTime(DateTime.Today);
            var plans = await context.Plans
                .AsNoTracking()
                .Where(p => p.FacilityId == facilityId &&
                           !p.IsDeleted &&
                           p.IsActive &&
                           p.ActiveFrom <= today &&
                           p.ActiveTo >= today)
                .OrderBy(p => p.PlanCode)
                .Select(p => new SearchFilterPanel.FilterItem
                {
                    Id = p.PlanId,
                    Name = p.PlanName
                })
                .ToListAsync();
            state.AvailablePlans = plans;

            // オプション（PlanOptionのOptionPlanIdに基づくプラン）をロード
            var optionPlanIds = await context.PlanOptions
                .AsNoTracking()
                .Where(po => !po.IsDeleted)
                .Select(po => po.OptionPlanId)
                .Distinct()
                .ToListAsync();

            var options = await context.Plans
                .AsNoTracking()
                .Where(p => optionPlanIds.Contains(p.PlanId) &&
                           p.FacilityId == facilityId &&
                           !p.IsDeleted &&
                           p.IsActive &&
                           p.ActiveFrom <= today &&
                           p.ActiveTo >= today)
                .OrderBy(p => p.PlanCode)
                .Select(p => new SearchFilterPanel.FilterItem
                {
                    Id = p.PlanId,
                    Name = p.PlanName
                })
                .ToListAsync();
            state.AvailableOptions = options;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error loading filter options");
            state.AvailableFloors = new List<SearchFilterPanel.FilterItem>();
            state.AvailableResourceGroups = new List<SearchFilterPanel.FilterItem>();
            state.AvailableResources = new List<SearchFilterPanel.FilterItem>();
            state.AvailablePlans = new List<SearchFilterPanel.FilterItem>();
            state.AvailableOptions = new List<SearchFilterPanel.FilterItem>();
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
