using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.Dtos;
using Aloe.Apps.MedockLib.Repositories;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockServer.Components.Calendar;
using Aloe.Apps.MedockServer.Components.FAB;
using Aloe.Apps.MedockServer.Components.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Aloe.Apps.MedockServer.Components.Pages;

/// <summary>
/// カレンダーページのデータロード処理サービス
/// </summary>
public class CalendarDataService
{
    private readonly IAppointmentService _appointmentService;
    private readonly IFacilityService _facilityService;
    private readonly IAppointmentStatsRepository _appointmentStatsRepository;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IDbContextFactory<MedockDbContext> _contextFactory;
    private readonly ILogger<CalendarDataService> _logger;

    public CalendarDataService(
        IAppointmentService appointmentService,
        IFacilityService facilityService,
        IAppointmentStatsRepository appointmentStatsRepository,
        AuthenticationStateProvider authStateProvider,
        IDbContextFactory<MedockDbContext> contextFactory,
        ILogger<CalendarDataService> logger)
    {
        this._appointmentService = appointmentService;
        this._facilityService = facilityService;
        this._appointmentStatsRepository = appointmentStatsRepository;
        this._authStateProvider = authStateProvider;
        this._contextFactory = contextFactory;
        this._logger = logger;
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
                    _logger.LogWarning("Failed to load business hours: {ErrorMessage}", result.ErrorMessage);
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
            _logger.LogError(ex, "Error loading business hours for facility {FacilityId} on date {Date}",
                state.CurrentFacilityId, state.CurrentDate);
            state.BusinessHours = new BusinessHoursDto();
        }
    }

    /// <summary>
    /// 休日情報をロードして状態に反映します。
    /// </summary>
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
                _logger.LogWarning("Failed to load holidays: {ErrorMessage}", result.ErrorMessage);
                state.Holidays.Clear();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading holidays for date range {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}",
                currentDate, currentDate.AddDays(weekDays));
            state.Holidays.Clear();
        }
    }

    /// <summary>
    /// フィルター用のオプション（フロア、リソースグループ、プラン、オプション）をロードして状態に反映します。
    /// </summary>
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
            _logger.LogError(ex, "Error loading filter options");
            state.AvailableFloors = new List<SearchFilterPanel.FilterItem>();
            state.AvailableResourceGroups = new List<SearchFilterPanel.FilterItem>();
            state.AvailableResources = new List<SearchFilterPanel.FilterItem>();
            state.AvailablePlans = new List<SearchFilterPanel.FilterItem>();
            state.AvailableOptions = new List<SearchFilterPanel.FilterItem>();
        }
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
        var sw = Stopwatch.StartNew();
        try
        {
            // 【パフォーマンス対策】Month/Year viewではアポイントメント詳細は不要（mainStats/equipmentStatsで統計表示）
            // Week viewのみアポイントメント詳細が必要（個別予約ブロック表示）
            if (viewType == CalendarViewType.Year || viewType == CalendarViewType.Month)
            {
                _logger.LogInformation("[TRACE] LoadAppointmentsAsync SKIPPED: ViewType={ViewType}, CurrentDate={Date} (appointments not used - mainStats/equipmentStats only)",
                    viewType, currentDate);
                state.Appointments = new List<AppointmentDto>();
                _logger.LogWarning("[TRACE] LoadAppointmentsAsync: {ViewType} ビューのため Appointments をクリアしました", viewType);
                return;
            }

            _logger.LogInformation("[TRACE] LoadAppointmentsAsync: {ViewType} ビューなのでデータをロードします", viewType);

            var (startDate, endDate) = GetDateRange(viewType, currentDate, weekDays);
            _logger.LogInformation("[TRACE] LoadAppointmentsAsync start: ViewType={ViewType}, CurrentDate={CurrentDate}, DateRange={StartDate:yyyy-MM-dd}~{EndDate:yyyy-MM-dd}",
                viewType, currentDate, startDate, endDate);

            var querySw = Stopwatch.StartNew();
            var result = await this._appointmentService.GetAppointmentsAsync(startDate, endDate);
            querySw.Stop();
            _logger.LogInformation("[PERF] LoadAppointmentsAsync - Repository query: {ElapsedMs}ms",
                querySw.ElapsedMilliseconds);

            if (!result.IsSuccess || result.Value == null)
            {
                _logger.LogWarning("Failed to load appointments: {ErrorMessage}", result.ErrorMessage);
                state.Appointments = [];
                return;
            }

            var appointments = result.Value;
            _logger.LogInformation("[PERF] LoadAppointmentsAsync - Retrieved {Count} appointments from service",
                appointments.Count);

            // フロアフィルターを適用
            var filterSw = Stopwatch.StartNew();
            _logger.LogWarning("[TRACE] LoadAppointmentsAsync - CurrentFilter: {Filter}, SelectedFloorIds: {FloorCount}",
                state.CurrentFilter != null ? "set" : "null",
                state.CurrentFilter?.SelectedFloorIds.Count ?? 0);
            if (state.CurrentFilter != null && state.CurrentFilter.SelectedFloorIds.Any())
            {
                _logger.LogWarning("[TRACE] LoadAppointmentsAsync - フロアフィルター適用前: {Count}件", appointments.Count);
                appointments = appointments
                    .Where(a => a.FloorId.HasValue && state.CurrentFilter.SelectedFloorIds.Contains(a.FloorId.Value))
                    .ToList();
                _logger.LogWarning("[TRACE] LoadAppointmentsAsync - フロアフィルター適用後: {Count}件", appointments.Count);
            }
            filterSw.Stop();
            _logger.LogInformation("[PERF] LoadAppointmentsAsync - Floor filtering: {ElapsedMs}ms, Count after filter={Count}",
                filterSw.ElapsedMilliseconds, appointments.Count);

            state.Appointments = appointments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading appointments");
            state.Appointments = [];
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation("[PERF] LoadAppointmentsAsync - Total: {ElapsedMs}ms", sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// MainリソースのStatsを日付ごとにグループ化して状態に反映します。
    /// </summary>
    public async Task LoadMainStatsAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var (startDate, endDate) = GetDateRange(viewType, currentDate, weekDays);
            _logger.LogInformation("[TRACE] LoadMainStatsAsync start: ViewType={ViewType}, CurrentDate={CurrentDate}, DateRange={StartDate:yyyy-MM-dd}~{EndDate:yyyy-MM-dd}",
                viewType, currentDate, startDate, endDate);

            var querySw = Stopwatch.StartNew();
            List<AppointmentStats> mainStats;

            // Mainリソースは常に表示する（Equipmentリソースのフィルター選択に関係なく）
            // SelectedResourceIdsがEquipmentリソースのみの場合、または他のフィルター条件がない場合は
            // フィルターを使わずに全てのMainリソースを取得
            var hasNonResourceFilters = (state.CurrentFilter != null) && (
                state.CurrentFilter.SelectedFloorIds.Any() ||
                state.CurrentFilter.SelectedResourceGroupIds.Any() ||
                state.CurrentFilter.SelectedPlanIds.Any() ||
                state.CurrentFilter.SelectedOptionPlanIds.Any());

            if (hasNonResourceFilters)
            {
                // フロア、リソースグループ、プラン・オプションのフィルターが有効な場合のみフィルター付きメソッドを使用
                // SelectedResourceIdsは渡さない（Mainリソースは常に表示、Equipmentリソースのみフィルタリング）
                mainStats = await this._appointmentStatsRepository.GetMainResourceStatsByDateRangeWithFiltersAsync(
                    startDate,
                    endDate,
                    state.CurrentFilter!.SelectedFloorIds.Any() ? state.CurrentFilter.SelectedFloorIds : null,
                    state.CurrentFilter.SelectedResourceGroupIds.Any() ? state.CurrentFilter.SelectedResourceGroupIds : null,
                    null, // SelectedResourceIdsは渡さない（Mainリソースは常に表示）
                    state.CurrentFilter.SelectedPlanIds.Any() ? state.CurrentFilter.SelectedPlanIds : null,
                    state.CurrentFilter.SelectedOptionPlanIds.Any() ? state.CurrentFilter.SelectedOptionPlanIds : null);
            }
            else
            {
                // フィルターがない、またはEquipmentリソースのみのフィルターの場合は全てのMainリソースを取得
                mainStats = await this._appointmentStatsRepository.GetMainResourceStatsByDateRangeAsync(startDate, endDate);
            }
            querySw.Stop();
            _logger.LogInformation("LoadMainStatsAsync query: {ElapsedMs}ms, Count={Count}",
                querySw.ElapsedMilliseconds, mainStats.Count);

            // AppointmentSlotOverrideを取得
            var overrideSw = Stopwatch.StartNew();
            using var context = this._contextFactory.CreateDbContext();
            var slotOverrides = await context.AppointmentSlotOverrides
                .AsNoTracking()
                .Where(o => !o.IsDeleted &&
                           o.ApptDate >= startDate &&
                           o.ApptDate <= endDate)
                .Include(o => o.AppointmentResource)
                .ToListAsync();
            var overridesByDateAndResource = slotOverrides
                .GroupBy(o => (o.ApptDate, o.ApptResId))
                .ToDictionary(g => g.Key, g => g.First());
            overrideSw.Stop();
            _logger.LogInformation("LoadMainStatsAsync slotOverrides: {ElapsedMs}ms, Count={Count}",
                overrideSw.ElapsedMilliseconds, slotOverrides.Count);

            // 日付ごとにグループ化
            var groupSw = Stopwatch.StartNew();
            var statsByDate = mainStats.GroupBy(s => s.ApptDate).ToDictionary(g => g.Key, g => g.ToList());
            groupSw.Stop();
            _logger.LogInformation("LoadMainStatsAsync grouping: {ElapsedMs}ms", groupSw.ElapsedMilliseconds);

            // 全日付を初期化
            state.MainStats.Clear();
            state.OriginalMainStats.Clear();
            state.MainStatsGrayedOut.Clear();

            var initSw = Stopwatch.StartNew();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dateStr = date.ToString("yyyy-MM-dd");

                // その日のMainリソースStatsがあれば設定、なければ空リスト
                if (statsByDate.TryGetValue(date, out var mainStatsList))
                {
                    // AppointmentSlotOverrideがあれば適用
                    var statsWithOverrides = mainStatsList.Select(stat =>
                    {
                        var key = (date, stat.AppointmentResource.ApptResId);
                        if (overridesByDateAndResource.TryGetValue(key, out var slotOverride))
                        {
                            // 上書きされたスロット定義でAppointmentStatSlotsを再構築
                            return ApplySlotOverride(stat, slotOverride);
                        }
                        return stat;
                    }).ToList();

                    state.MainStats[dateStr] = statsWithOverrides;
                    state.OriginalMainStats[dateStr] = statsWithOverrides.ToList(); // コピーを作成
                }
                else
                {
                    state.MainStats[dateStr] = new List<AppointmentStats>();
                    state.OriginalMainStats[dateStr] = new List<AppointmentStats>();
                }

                state.MainStatsGrayedOut[dateStr] = false;
            }
            initSw.Stop();
            _logger.LogInformation("LoadMainStatsAsync initialization: {ElapsedMs}ms, Days={DayCount}",
                initSw.ElapsedMilliseconds, endDate.DayNumber - startDate.DayNumber + 1);
        }
        catch (Exception ex)
        {
            // エラー時は空のMainStatsを設定
            _logger.LogError(ex, "Error loading main stats");
            state.MainStats.Clear();
            state.OriginalMainStats.Clear();
            state.MainStatsGrayedOut.Clear();
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation("LoadMainStatsAsync total: {ElapsedMs}ms", sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// EquipmentリソースのStatsを日付ごとにグループ化して状態に反映します。
    /// フィルターで選択されたEquipmentリソースのみを取得します。
    /// </summary>
    public async Task LoadEquipmentStatsAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            // フィルターが有効でない場合、またはEquipmentリソースが選択されていない場合は空を設定
            if (state.CurrentFilter == null || !state.CurrentFilter.SelectedResourceIds.Any())
            {
                state.EquipmentStats.Clear();
                state.OriginalEquipmentStats.Clear();
                state.EquipmentStatsOptimized = null;
                var msg = state.CurrentFilter == null ? "CurrentFilter is null" : "SelectedResourceIds is empty";
                _logger.LogInformation("[TRACE] LoadEquipmentStatsAsync: {Message}", msg);
                return;
            }

            _logger.LogDebug("LoadEquipmentStatsAsync: SelectedResourceIds count = {Count}", state.CurrentFilter.SelectedResourceIds.Count);

            var (startDate, endDate) = GetDateRange(viewType, currentDate, weekDays);
            _logger.LogInformation("[TRACE] LoadEquipmentStatsAsync start: ViewType={ViewType}, CurrentDate={CurrentDate}, DateRange={StartDate:yyyy-MM-dd}~{EndDate:yyyy-MM-dd}",
                viewType, currentDate, startDate, endDate);

            // 【最適化版】FromSql + array_agg で SQL側で配列化
            // 既に日付ごと・リソースごとにグループ化されて返される
            var querySw = Stopwatch.StartNew();
            _logger.LogDebug("About to call GetEquipmentResourceSlotsAsArraysByDateAsync with {Count} resource IDs", state.CurrentFilter.SelectedResourceIds.Count);
            var equipmentStatsOptimized = await this._appointmentStatsRepository.GetEquipmentResourceSlotsAsArraysByDateAsync(
                startDate,
                endDate,
                state.CurrentFilter.SelectedResourceIds);
            querySw.Stop();
            _logger.LogDebug("GetEquipmentResourceSlotsAsArraysByDateAsync returned successfully with {DateCount} dates", equipmentStatsOptimized.Count);

            var totalResources = equipmentStatsOptimized.Sum(kvp => kvp.Value.Count);
            _logger.LogInformation("LoadEquipmentStatsAsync query (optimized): {ElapsedMs}ms, Dates={DateCount}, TotalResources={TotalResources}",
                querySw.ElapsedMilliseconds, equipmentStatsOptimized.Count, totalResources);

            // 最適化版データを状態に保存（CalendarCanvasで使用）
            _logger.LogDebug("Setting state.EquipmentStatsOptimized with {DateCount} dates", equipmentStatsOptimized.Count);
            state.EquipmentStatsOptimized = equipmentStatsOptimized;

            // 従来互換性のため空リストを設定
            _logger.LogDebug("Clearing EquipmentStats dictionaries");
            state.EquipmentStats.Clear();
            state.OriginalEquipmentStats.Clear();

            var initSw = Stopwatch.StartNew();
            _logger.LogDebug("Starting to populate EquipmentStats for dates {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}", startDate, endDate);
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dateStr = date.ToString("yyyy-MM-dd");
                // 従来型は空リストを設定（データは EquipmentStatsOptimized に格納済み）
                state.EquipmentStats[dateStr] = new List<AppointmentStats>();
                state.OriginalEquipmentStats[dateStr] = new List<AppointmentStats>();
            }
            initSw.Stop();
            _logger.LogDebug("Finished populating EquipmentStats");
            _logger.LogInformation("LoadEquipmentStatsAsync initialization: {ElapsedMs}ms, Days={DayCount}",
                initSw.ElapsedMilliseconds, endDate.DayNumber - startDate.DayNumber + 1);
        }
        catch (Exception ex)
        {
            // エラー時は空のEquipmentStatsを設定
            state.EquipmentStats.Clear();
            state.OriginalEquipmentStats.Clear();
            _logger.LogError(ex, "Error in LoadEquipmentStatsAsync: ViewType={ViewType}, DateRange={StartDate:yyyy-MM-dd}~{EndDate:yyyy-MM-dd}",
                viewType, currentDate, currentDate);
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation("LoadEquipmentStatsAsync total: {ElapsedMs}ms", sw.ElapsedMilliseconds);
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

    /// <summary>
    /// AppointmentSlotOverrideをAppointmentStatsに適用
    /// </summary>
    private static AppointmentStats ApplySlotOverride(AppointmentStats stat, AppointmentSlotOverride slotOverride)
    {
        if (slotOverride.ApptSlotsData == null || !slotOverride.ApptSlotsData.Slots.Any())
        {
            // 上書きデータが空の場合は、元の統計をそのまま返す
            return stat;
        }

        try
        {
            // 既存のAppointmentStatSlotsを取得
            var existingSlots = stat.AppointmentStatSlots?.Where(s => !s.IsDeleted).ToList() ?? new List<AppointmentStatSlots>();

            // 上書きされたスロット定義を使用して新しいAppointmentStatSlotsを構築
            var overrideSlots = slotOverride.ApptSlotsData.Slots;
            var newStatSlots = new List<AppointmentStatSlots>();

            // 上書きされたスロット定義を元に、既存のカウントをマッピング
            foreach (var overrideSlot in overrideSlots)
            {
                // 既存のスロットから対応するカウントを取得
                var slotStartMinutes = overrideSlot.Start.Hour * 60 + overrideSlot.Start.Minute;
                var slotEndMinutes = overrideSlot.End.Hour * 60 + overrideSlot.End.Minute;

                var matchingSlot = existingSlots.FirstOrDefault(s =>
                    s.SlotStart == slotStartMinutes && s.SlotEnd == slotEndMinutes);

                newStatSlots.Add(new AppointmentStatSlots
                {
                    ApptStatSlotId = Guid.CreateVersion7(),
                    ApptStatId = stat.ApptStatId,
                    ApptDate = stat.ApptDate,
                    ApptResId = stat.ApptResId,
                    SlotStart = slotStartMinutes,
                    SlotEnd = slotEndMinutes,
                    SlotCount = matchingSlot?.SlotCount ?? 0,
                    SlotCap = overrideSlot.Cap,
                    IsDeleted = false,
                    CreatedAt = matchingSlot?.CreatedAt ?? DateTime.UtcNow,
                    CreatedUserId = matchingSlot?.CreatedUserId ?? Guid.Empty,
                    CreatedSessionId = matchingSlot?.CreatedSessionId ?? Guid.Empty,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedUserId = Guid.Empty,
                    UpdatedSessionId = Guid.Empty
                });
            }

            // 新しいAppointmentStatsを作成（AppointmentStatSlotsを含む）
            return new AppointmentStats
            {
                ApptStatId = stat.ApptStatId,
                ApptDate = stat.ApptDate,
                ApptResId = stat.ApptResId,
                ApptCap = stat.ApptCap,
                ApptCount = stat.ApptCount,
                AppointmentResource = stat.AppointmentResource,
                AppointmentStatSlots = newStatSlots,
                IsDeleted = stat.IsDeleted,
                CreatedAt = stat.CreatedAt,
                CreatedUserId = stat.CreatedUserId,
                CreatedSessionId = stat.CreatedSessionId,
                UpdatedAt = stat.UpdatedAt,
                UpdatedUserId = stat.UpdatedUserId,
                UpdatedSessionId = stat.UpdatedSessionId
            };
        }
        catch (Exception)
        {
            // エラー時は元の統計をそのまま返す
            return stat;
        }
    }
}
