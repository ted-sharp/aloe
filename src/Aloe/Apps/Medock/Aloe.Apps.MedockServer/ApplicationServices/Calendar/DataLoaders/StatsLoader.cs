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
