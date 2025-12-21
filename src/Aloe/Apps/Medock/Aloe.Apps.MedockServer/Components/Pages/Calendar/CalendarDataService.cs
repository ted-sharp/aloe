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
using System.Diagnostics;
using System.Text.Json;

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

    public CalendarDataService(
        IAppointmentService appointmentService,
        IFacilityService facilityService,
        IAppointmentStatsRepository appointmentStatsRepository,
        AuthenticationStateProvider authStateProvider,
        IDbContextFactory<MedockDbContext> contextFactory)
    {
        this._appointmentService = appointmentService;
        this._facilityService = facilityService;
        this._appointmentStatsRepository = appointmentStatsRepository;
        this._authStateProvider = authStateProvider;
        this._contextFactory = contextFactory;
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
            Console.WriteLine($"フィルターオプションのロードエラー: {ex.Message}");
            state.AvailableFloors = new List<SearchFilterPanel.FilterItem>();
            state.AvailableResourceGroups = new List<SearchFilterPanel.FilterItem>();
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
            var (startDate, endDate) = GetDateRange(viewType, currentDate, weekDays);
            Console.WriteLine($"[Performance] LoadAppointmentsAsync start: ViewType={viewType}, DateRange={startDate:yyyy-MM-dd}~{endDate:yyyy-MM-dd}");
            
            var querySw = Stopwatch.StartNew();
            var appointments = await this._appointmentService.GetAppointmentsAsync(startDate, endDate);
            
            // フロアフィルターを適用
            if (state.CurrentFilter != null && state.CurrentFilter.SelectedFloorIds.Any())
            {
                appointments = appointments
                    .Where(a => a.FloorId.HasValue && state.CurrentFilter.SelectedFloorIds.Contains(a.FloorId.Value))
                    .ToList();
            }
            
            querySw.Stop();
            Console.WriteLine($"[Performance] LoadAppointmentsAsync query: {querySw.ElapsedMilliseconds}ms, Count={appointments.Count}");
            
            state.Appointments = appointments;
        }
        catch (Exception)
        {
            state.Appointments = [];
        }
        finally
        {
            sw.Stop();
            Console.WriteLine($"[Performance] LoadAppointmentsAsync total: {sw.ElapsedMilliseconds}ms");
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
            Console.WriteLine($"[Performance] LoadMainStatsAsync start: ViewType={viewType}, DateRange={startDate:yyyy-MM-dd}~{endDate:yyyy-MM-dd}");
            
            var querySw = Stopwatch.StartNew();
            List<AppointmentStats> mainStats;
            if (state.CurrentFilter != null && state.CurrentFilter.IsActive)
            {
                // フィルターが有効な場合はフィルター付きメソッドを使用
                mainStats = await this._appointmentStatsRepository.GetMainResourceStatsByDateRangeWithFiltersAsync(
                    startDate,
                    endDate,
                    state.CurrentFilter.SelectedFloorIds.Any() ? state.CurrentFilter.SelectedFloorIds : null,
                    state.CurrentFilter.SelectedResourceGroupIds.Any() ? state.CurrentFilter.SelectedResourceGroupIds : null,
                    state.CurrentFilter.SelectedPlanIds.Any() ? state.CurrentFilter.SelectedPlanIds : null,
                    state.CurrentFilter.SelectedOptionPlanIds.Any() ? state.CurrentFilter.SelectedOptionPlanIds : null);
            }
            else
            {
                mainStats = await this._appointmentStatsRepository.GetMainResourceStatsByDateRangeAsync(startDate, endDate);
            }
            querySw.Stop();
            Console.WriteLine($"[Performance] LoadMainStatsAsync query: {querySw.ElapsedMilliseconds}ms, Count={mainStats.Count}");

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
            Console.WriteLine($"[Performance] LoadMainStatsAsync slotOverrides: {overrideSw.ElapsedMilliseconds}ms, Count={slotOverrides.Count}");

            // 日付ごとにグループ化
            var groupSw = Stopwatch.StartNew();
            var statsByDate = mainStats.GroupBy(s => s.ApptDate).ToDictionary(g => g.Key, g => g.ToList());
            groupSw.Stop();
            Console.WriteLine($"[Performance] LoadMainStatsAsync grouping: {groupSw.ElapsedMilliseconds}ms");

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
                            // 上書きされたスロット定義でApptGraphを再構築
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
            Console.WriteLine($"[Performance] LoadMainStatsAsync initialization: {initSw.ElapsedMilliseconds}ms, Days={endDate.DayNumber - startDate.DayNumber + 1}");
        }
        catch (Exception)
        {
            // エラー時は空のMainStatsを設定
            state.MainStats.Clear();
            state.OriginalMainStats.Clear();
            state.MainStatsGrayedOut.Clear();
        }
        finally
        {
            sw.Stop();
            Console.WriteLine($"[Performance] LoadMainStatsAsync total: {sw.ElapsedMilliseconds}ms");
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
            // 既存のApptGraphをパース
            var existingGraph = JsonSerializer.Deserialize<AppointmentGraphRoot>(stat.ApptGraph);
            if (existingGraph == null)
            {
                return stat;
            }

            // 上書きされたスロット定義を使用して新しいグラフを構築
            var overrideSlots = slotOverride.ApptSlotsData.Slots;
            var newSlots = new List<AppointmentGraphItem>();

            // 上書きされたスロット定義を元に、既存のカウントをマッピング
            foreach (var overrideSlot in overrideSlots)
            {
                // 既存のスロットから対応するカウントを取得
                var matchingSlot = existingGraph.Slots.FirstOrDefault(s =>
                    s.Start == overrideSlot.Start && s.End == overrideSlot.End);

                newSlots.Add(new AppointmentGraphItem
                {
                    Start = overrideSlot.Start,
                    End = overrideSlot.End,
                    Count = matchingSlot?.Count ?? 0,
                    Cap = overrideSlot.Cap,
                    HasOutsideHours = overrideSlot.IsOutsideHours
                });
            }

            // 新しいグラフを作成
            var newGraph = new AppointmentGraphRoot { Slots = newSlots };
            var newGraphJson = JsonSerializer.Serialize(newGraph);

            // 新しいAppointmentStatsを作成
            return new AppointmentStats
            {
                ApptStatId = stat.ApptStatId,
                ApptDate = stat.ApptDate,
                ApptResId = stat.ApptResId,
                ApptGraph = newGraphJson,
                AppointmentResource = stat.AppointmentResource,
                IsDeleted = stat.IsDeleted,
                CreatedAt = stat.CreatedAt,
                CreatedUserId = stat.CreatedUserId,
                CreatedSessionId = stat.CreatedSessionId,
                UpdatedAt = stat.UpdatedAt,
                UpdatedUserId = stat.UpdatedUserId,
                UpdatedSessionId = stat.UpdatedSessionId
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AppointmentSlotOverride適用エラー: {ex.Message}");
            // エラー時は元の統計をそのまま返す
            return stat;
        }
    }
}
