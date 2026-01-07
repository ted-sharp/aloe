using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Repositories;
using Aloe.Apps.MedockServer.Components.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Aloe.Apps.MedockServer.ApplicationServices.Calendar.DataLoaders;

/// <summary>
/// 統計データ（MainStats, EquipmentStats）のロード処理を担当するサービス
/// </summary>
public class StatsLoader : IStatsLoader
{
    private readonly IAppointmentStatsRepository _appointmentStatsRepository;
    private readonly IDbContextFactory<MedockDbContext> _contextFactory;
    private readonly ILogger<StatsLoader> _logger;

    public StatsLoader(
        IAppointmentStatsRepository appointmentStatsRepository,
        IDbContextFactory<MedockDbContext> contextFactory,
        ILogger<StatsLoader> logger)
    {
        this._appointmentStatsRepository = appointmentStatsRepository;
        this._contextFactory = contextFactory;
        this._logger = logger;
    }

    /// <inheritdoc />
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
            this._logger.LogInformation("[TRACE] LoadMainStatsAsync start: ViewType={ViewType}, CurrentDate={CurrentDate}, DateRange={StartDate:yyyy-MM-dd}~{EndDate:yyyy-MM-dd}",
                viewType, currentDate, startDate, endDate);

            var querySw = Stopwatch.StartNew();
            List<AppointmentStats> mainStats;

            // Mainリソースは常に表示する（Equipmentリソースのフィルター選択に関係なく）
            // SelectedResourceIdsがEquipmentリソースのみの場合、または他のフィルター条件がない場合は
            // フィルターを使わずに全てのMainリソースを取得
            var hasNonResourceFilters = (state.CurrentFilter != null) && (
                state.CurrentFilter.SelectedFloorIds.Any() ||
                state.CurrentFilter.SelectedPlanIds.Any());

            if (hasNonResourceFilters)
            {
                // フロア、プランのフィルターが有効な場合のみフィルター付きメソッドを使用
                // SelectedResourceIdsは渡さない（Mainリソースは常に表示、Equipmentリソースのみフィルタリング）
                mainStats = await this._appointmentStatsRepository.GetMainResourceStatsByDateRangeWithFiltersAsync(
                    startDate,
                    endDate,
                    state.CurrentFilter!.SelectedFloorIds.Any() ? state.CurrentFilter.SelectedFloorIds : null,
                    null, // resourceGroupIds - 削除済み
                    null, // SelectedResourceIdsは渡さない（Mainリソースは常に表示）
                    state.CurrentFilter.SelectedPlanIds.Any() ? state.CurrentFilter.SelectedPlanIds : null,
                    null); // optionPlanIds - 削除済み
            }
            else
            {
                // フィルターがない、またはEquipmentリソースのみのフィルターの場合は全てのMainリソースを取得
                mainStats = await this._appointmentStatsRepository.GetMainResourceStatsByDateRangeAsync(startDate, endDate);
            }
            querySw.Stop();
            this._logger.LogInformation("LoadMainStatsAsync query: {ElapsedMs}ms, Count={Count}",
                querySw.ElapsedMilliseconds, mainStats.Count);

            // NOTE: AppointmentSlotOverrides have been replaced with AppointmentScheduleOverrides
            // Stats are now pre-calculated during seeding, so runtime override application is no longer needed
            var overridesByDateAndResource = new Dictionary<(DateOnly, Guid), AppointmentScheduleOverride>();

            // 日付ごとにグループ化
            var groupSw = Stopwatch.StartNew();
            var statsByDate = mainStats.GroupBy(s => s.ApptDate).ToDictionary(g => g.Key, g => g.ToList());
            groupSw.Stop();
            this._logger.LogInformation("LoadMainStatsAsync grouping: {ElapsedMs}ms", groupSw.ElapsedMilliseconds);

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
                    // NOTE: Slot overrides are now pre-calculated during seeding
                    // Stats are already in their final form from the database
                    state.MainStats[dateStr] = mainStatsList;
                    state.OriginalMainStats[dateStr] = mainStatsList.ToList();
                }
                else
                {
                    state.MainStats[dateStr] = new List<AppointmentStats>();
                    state.OriginalMainStats[dateStr] = new List<AppointmentStats>();
                }

                state.MainStatsGrayedOut[dateStr] = false;
            }
            initSw.Stop();
            this._logger.LogInformation("LoadMainStatsAsync initialization: {ElapsedMs}ms, Days={DayCount}",
                initSw.ElapsedMilliseconds, endDate.DayNumber - startDate.DayNumber + 1);
        }
        catch (Exception ex)
        {
            // エラー時は空のMainStatsを設定
            this._logger.LogError(ex, "Error loading main stats");
            state.MainStats.Clear();
            state.OriginalMainStats.Clear();
            state.MainStatsGrayedOut.Clear();
        }
        finally
        {
            sw.Stop();
            this._logger.LogInformation("LoadMainStatsAsync total: {ElapsedMs}ms", sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Mainリソースのスロット単位統計データをロードして状態に反映します。
    /// </summary>
    public async Task LoadMainStatsSlotsAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var (startDate, endDate) = GetDateRange(viewType, currentDate, weekDays);
            this._logger.LogInformation("[TRACE] LoadMainStatsSlotsAsync start: ViewType={ViewType}, CurrentDate={CurrentDate}, DateRange={StartDate:yyyy-MM-dd}~{EndDate:yyyy-MM-dd}",
                viewType, currentDate, startDate, endDate);

            // MainStats から Main リソース ID を抽出（LoadMainStatsAsync で既にロード済み）
            var mainResourceIds = state.MainStats
                .SelectMany(kvp => kvp.Value)
                .Select(s => s.ApptResId)
                .Distinct()
                .ToList();

            this._logger.LogInformation("LoadMainStatsSlotsAsync - Main resource IDs: {Count}", mainResourceIds.Count);

            var querySw = Stopwatch.StartNew();
            // Main リソースのスロットのみを取得
            var statSlots = await this._appointmentStatsRepository.GetStatSlotsByDateRangeAsync(startDate, endDate, mainResourceIds);
            querySw.Stop();

            var totalSlotCount = statSlots.Sum(kvp => kvp.Value.Count);
            this._logger.LogInformation("LoadMainStatsSlotsAsync query: {ElapsedMs}ms, DateCount={DateCount}, TotalSlots={TotalSlots}",
                querySw.ElapsedMilliseconds, statSlots.Count, totalSlotCount);

            // スロットデータを状態に保存
            state.MainStatsSlots = statSlots;
            this._logger.LogInformation("LoadMainStatsSlotsAsync - State updated with {DateCount} dates", statSlots.Count);
        }
        catch (Exception ex)
        {
            // エラー時は null を設定
            state.MainStatsSlots = null;
            this._logger.LogError(ex, "Error loading main stats slots");
        }
        finally
        {
            sw.Stop();
            this._logger.LogInformation("LoadMainStatsSlotsAsync total: {ElapsedMs}ms", sw.ElapsedMilliseconds);
        }
    }

    /// <inheritdoc />
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
                state.EquipmentStatsOptimized = null;
                var msg = state.CurrentFilter == null ? "CurrentFilter is null" : "SelectedResourceIds is empty";
                this._logger.LogInformation("[TRACE] LoadEquipmentStatsAsync: {Message}", msg);
                return;
            }

            this._logger.LogDebug("LoadEquipmentStatsAsync: SelectedResourceIds count = {Count}", state.CurrentFilter.SelectedResourceIds.Count);

            var (startDate, endDate) = GetDateRange(viewType, currentDate, weekDays);
            this._logger.LogInformation("[TRACE] LoadEquipmentStatsAsync start: ViewType={ViewType}, CurrentDate={CurrentDate}, DateRange={StartDate:yyyy-MM-dd}~{EndDate:yyyy-MM-dd}",
                viewType, currentDate, startDate, endDate);

            // 【最適化版】FromSql + array_agg で SQL側で配列化
            // 既に日付ごと・リソースごとにグループ化されて返される
            var querySw = Stopwatch.StartNew();
            this._logger.LogDebug("About to call GetEquipmentResourceSlotsAsArraysByDateAsync with {Count} resource IDs", state.CurrentFilter.SelectedResourceIds.Count);
            var equipmentStatsOptimized = await this._appointmentStatsRepository.GetEquipmentResourceSlotsAsArraysByDateAsync(
                startDate,
                endDate,
                state.CurrentFilter.SelectedResourceIds);
            querySw.Stop();
            this._logger.LogDebug("GetEquipmentResourceSlotsAsArraysByDateAsync returned successfully with {DateCount} dates", equipmentStatsOptimized.Count);

            var totalResources = equipmentStatsOptimized.Sum(kvp => kvp.Value.Count);
            this._logger.LogInformation("LoadEquipmentStatsAsync query (optimized): {ElapsedMs}ms, Dates={DateCount}, TotalResources={TotalResources}",
                querySw.ElapsedMilliseconds, equipmentStatsOptimized.Count, totalResources);

            // 最適化版データを状態に保存
            this._logger.LogDebug("Setting state.EquipmentStatsOptimized with {DateCount} dates", equipmentStatsOptimized.Count);
            state.EquipmentStatsOptimized = equipmentStatsOptimized;
        }
        catch (Exception ex)
        {
            // エラー時は null を設定
            state.EquipmentStatsOptimized = null;
            this._logger.LogError(ex, "Error in LoadEquipmentStatsAsync: ViewType={ViewType}, DateRange={StartDate:yyyy-MM-dd}~{EndDate:yyyy-MM-dd}",
                viewType, currentDate, currentDate);
        }
        finally
        {
            sw.Stop();
            this._logger.LogInformation("LoadEquipmentStatsAsync total: {ElapsedMs}ms", sw.ElapsedMilliseconds);
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
