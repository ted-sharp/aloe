using Aloe.Apps.MedockLib.Common.Exceptions;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Logging;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Aloe.Apps.MedockLib.Repositories;

/// <summary>
/// 予約統計リポジトリ
/// </summary>
public class AppointmentStatsRepository : RepositoryBase, IAppointmentStatsRepository
{
    public AppointmentStatsRepository(
        MedockDbContext context,
        ILogger<AppointmentStatsRepository> logger,
        IUserContextService userContextService,
        IDateTimeProvider dateTimeProvider)
        : base(context, logger, userContextService, dateTimeProvider)
    {
    }

    /// <inheritdoc />
    public async Task<List<Data.Entities.AppointmentStats>> GetMainResourceStatsByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await this.ExecuteQueryAsync(
            () => this.Context.AppointmentStats
                .AsNoTracking()
                .Include(s => s.AppointmentResource)
                .Where(s => !s.IsDeleted &&
                            !s.AppointmentResource.IsDeleted &&
                            s.AppointmentResource.ApptResTypeCode == (int)AppointmentResourceType.Main &&
                            s.ApptDate >= startDate &&
                            s.ApptDate <= endDate)
                .ToListAsync(),
            ex => $"Failed to retrieve main resource stats for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            ex =>
            {
                var (tenantId, facilityId, userId) = this.GetTenantContext();
                LogMessages.MainResourceStatsRetrievalError((ILogger<AppointmentStatsRepository>)this.Logger, startDate, endDate, tenantId, facilityId, userId, ex);
            });
    }

    /// <inheritdoc />
    public async Task<List<Data.Entities.AppointmentStats>> GetMainResourceStatsByDateRangeWithFiltersAsync(
        DateOnly startDate,
        DateOnly endDate,
        List<Guid>? floorIds = null,
        List<Guid>? resourceGroupIds = null,
        List<Guid>? resourceIds = null,
        List<Guid>? planIds = null,
        List<Guid>? optionPlanIds = null)
    {
        return await this.ExecuteQueryAsync(
            async () =>
            {
                var query = this.Context.AppointmentStats
                    .AsNoTracking()
                    .Include(s => s.AppointmentResource)
                        .ThenInclude(r => r.Floor)
                    .Where(s => !s.IsDeleted &&
                                !s.AppointmentResource.IsDeleted &&
                                s.AppointmentResource.ApptResTypeCode == (int)AppointmentResourceType.Main &&
                                s.ApptDate >= startDate &&
                                s.ApptDate <= endDate);

                // フロアフィルター
                if (floorIds != null && floorIds.Any())
                {
                    query = query.Where(s => floorIds.Contains(s.AppointmentResource.FloorId));
                }

                // リソースフィルター
                if (resourceIds != null && resourceIds.Any())
                {
                    query = query.Where(s => resourceIds.Contains(s.AppointmentResource.ApptResId));
                }

                // プラン・オプションフィルター（PlanResourceRequirementを介してリソースを絞り込み）
                if (planIds != null && planIds.Any() || optionPlanIds != null && optionPlanIds.Any())
                {
                    var resourceIdsFromPlans = new HashSet<Guid>();

                    // プランからリソースを取得
                    if (planIds != null && planIds.Any())
                    {
                        var planResourceIds = await this.Context.PlanResourceRequirements
                            .AsNoTracking()
                            .Where(prr => !prr.IsDeleted && planIds.Contains(prr.PlanId))
                            .Select(prr => prr.ApptResId)
                            .Distinct()
                            .ToListAsync();
                        foreach (var id in planResourceIds)
                        {
                            resourceIdsFromPlans.Add(id);
                        }
                    }

                    // オプションからリソースを取得（PlanOptionのOptionPlanIdに対応するPlanResourceRequirementを検索）
                    if (optionPlanIds != null && optionPlanIds.Any())
                    {
                        // オプションのプランIDから、そのプランのリソース要件を取得
                        var optionResourceIds = await this.Context.PlanResourceRequirements
                            .AsNoTracking()
                            .Where(prr => !prr.IsDeleted && optionPlanIds.Contains(prr.PlanId))
                            .Select(prr => prr.ApptResId)
                            .Distinct()
                            .ToListAsync();
                        foreach (var id in optionResourceIds)
                        {
                            resourceIdsFromPlans.Add(id);
                        }
                    }

                    if (resourceIdsFromPlans.Any())
                    {
                        query = query.Where(s => resourceIdsFromPlans.Contains(s.AppointmentResource.ApptResId));
                    }
                    else
                    {
                        // プラン・オプションが選択されているが、該当リソースがない場合は空の結果を返す
                        return new List<Data.Entities.AppointmentStats>();
                    }
                }

                return await query.ToListAsync();
            },
            ex => $"Failed to retrieve main resource stats with filters for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            ex =>
            {
                var (tenantId, facilityId, userId) = this.GetTenantContext();
                LogMessages.MainResourceStatsWithFiltersRetrievalError((ILogger<AppointmentStatsRepository>)this.Logger, startDate, endDate, tenantId, facilityId, userId, ex);
            });
    }

    /// <inheritdoc />
    public async Task<List<Data.Entities.AppointmentStats>> GetMainResourceStatsByDateAndResourcesAsync(
        DateOnly date,
        List<Guid> resourceIds)
    {
        return await this.ExecuteQueryAsync(
            () => this.Context.AppointmentStats
                .AsNoTracking()
                .Include(s => s.AppointmentResource)
                .Where(s => !s.IsDeleted &&
                            !s.AppointmentResource.IsDeleted &&
                            s.AppointmentResource.ApptResTypeCode == (int)AppointmentResourceType.Main &&
                            s.ApptDate == date &&
                            resourceIds.Contains(s.ApptResId))
                .ToListAsync(),
            ex => $"Failed to retrieve main resource stats for date {date:yyyy-MM-dd}",
            ex =>
            {
                var (tenantId, facilityId, userId) = this.GetTenantContext();
                LogMessages.MainResourceStatsByDateRetrievalError((ILogger<AppointmentStatsRepository>)this.Logger, date, tenantId, facilityId, userId, ex);
            });
    }

    /// <inheritdoc />
    public async Task<List<Data.Entities.AppointmentStats>> GetEquipmentResourceStatsByDateRangeAsync(
        DateOnly startDate,
        DateOnly endDate,
        List<Guid> equipmentResourceIds)
    {
        return await this.ExecuteQueryAsync(
            async () =>
            {
                var query = this.Context.AppointmentStats
                    .AsNoTracking()
                    .Include(s => s.AppointmentResource)
                    .Where(s => !s.IsDeleted &&
                                !s.AppointmentResource.IsDeleted &&
                                s.AppointmentResource.ApptResTypeCode == (int)AppointmentResourceType.Equipment &&
                                s.ApptDate >= startDate &&
                                s.ApptDate <= endDate);

                // equipmentResourceIdsが指定されている場合のみフィルタリング
                if (equipmentResourceIds != null && equipmentResourceIds.Any())
                {
                    query = query.Where(s => equipmentResourceIds.Contains(s.AppointmentResource.ApptResId));
                }

                return await query.ToListAsync();
            },
            ex => $"Failed to retrieve equipment resource stats for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            ex =>
            {
                var (tenantId, facilityId, userId) = this.GetTenantContext();
                LogMessages.EquipmentResourceStatsRetrievalError((ILogger<AppointmentStatsRepository>)this.Logger, startDate, endDate, tenantId, facilityId, userId, ex);
            });
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, List<ResourceStatSlotsDto>>> GetEquipmentResourceSlotsAsArraysByDateAsync(
        DateOnly startDate,
        DateOnly endDate,
        List<Guid>? equipmentResourceIds)
    {
        // equipmentResourceIds が null または空の場合は空の辞書を返す
        if (equipmentResourceIds == null || !equipmentResourceIds.Any())
        {
            ((ILogger<AppointmentStatsRepository>)this.Logger).LogDebug("GetEquipmentResourceSlotsAsArraysByDateAsync: equipmentResourceIds is null or empty, returning empty dict");
            return new Dictionary<string, List<ResourceStatSlotsDto>>();
        }

        ((ILogger<AppointmentStatsRepository>)this.Logger).LogDebug("GetEquipmentResourceSlotsAsArraysByDateAsync: DateRange={StartDate:yyyy-MM-dd}~{EndDate:yyyy-MM-dd}, IDs count={Count}",
            startDate, endDate, equipmentResourceIds.Count);

        // PostgreSQL の array_agg で SQL側で配列化
        // 注意: SqlQueryRaw では複雑な投影ができないため、raw queryで取得後にクライアント側で処理
        // appointment_stat_slots テーブルは appt_date と appt_res_id で appointment_stats と関連付けられている
        var sql = @"
            SELECT
                ss.appt_date::text as ""ApptDate"",
                ss.appt_res_id::text as ""ResourceId"",
                ar.appt_res_name as ""ResourceName"",
                ar.appt_res_type_code::int as ""ResourceTypeCode"",
                COALESCE(SUM(ss.slot_cap), 0)::int as ""TotalCapacity"",
                COALESCE(SUM(ss.slot_available), 0)::int as ""TotalAvailable"",
                array_agg(ss.slot_start_min ORDER BY ss.slot_start_min)::int[] as ""SlotStartMinutes"",
                array_agg(ss.slot_end_min ORDER BY ss.slot_start_min)::int[] as ""SlotEndMinutes"",
                array_agg(ss.slot_cap ORDER BY ss.slot_start_min)::int[] as ""SlotCaps"",
                array_agg(ss.slot_available ORDER BY ss.slot_start_min)::int[] as ""SlotAvailables""
            FROM appointment_stat_slots ss
            INNER JOIN appointment_resources ar ON ss.appt_res_id = ar.appt_res_id
            WHERE ss.is_deleted = false
                AND ar.is_deleted = false
                AND ar.appt_res_type_code = @equipmentTypeCode
                AND ss.appt_date >= @startDate
                AND ss.appt_date <= @endDate
                AND ss.appt_res_id = ANY(@equipmentIds::uuid[])
            GROUP BY ss.appt_date, ss.appt_res_id, ar.appt_res_name, ar.appt_res_type_code
            ORDER BY ss.appt_date, ar.appt_res_name";

        var equipmentTypeCode = new NpgsqlParameter("@equipmentTypeCode", (int)AppointmentResourceType.Equipment);
        var startDateParam = new NpgsqlParameter("@startDate", startDate);
        var endDateParam = new NpgsqlParameter("@endDate", endDate);
        var equipmentIdsParam = new NpgsqlParameter("@equipmentIds", NpgsqlDbType.Array | NpgsqlDbType.Uuid) { Value = equipmentResourceIds.ToArray() };

        var parameters = new object[] { equipmentTypeCode, startDateParam, endDateParam, equipmentIdsParam };

        // 中間DTO で日付を含める
        return await this.ExecuteQueryAsync(
            async () =>
            {
                var results = await this.Context
                    .Database
                    .SqlQueryRaw<EquipmentStatsWithDateDto>(sql, parameters)
                    .ToListAsync();

                ((ILogger<AppointmentStatsRepository>)this.Logger).LogDebug("GetEquipmentResourceSlotsAsArraysByDateAsync: SQL returned {RowCount} rows", results.Count);

                // 日付ごとにグループ化
                var groupedByDate = new Dictionary<string, List<ResourceStatSlotsDto>>();
                foreach (var item in results)
                {
                    if (!groupedByDate.ContainsKey(item.ApptDate))
                    {
                        groupedByDate[item.ApptDate] = new List<ResourceStatSlotsDto>();
                    }

                    // 分数はそのままint配列として使用
                    var slotStartMinutes = item.SlotStartMinutes ?? Array.Empty<int>();
                    var slotEndMinutes = item.SlotEndMinutes ?? Array.Empty<int>();

                    groupedByDate[item.ApptDate].Add(new ResourceStatSlotsDto
                    {
                        ResourceId = item.ResourceId ?? String.Empty,
                        ResourceName = item.ResourceName ?? String.Empty,
                        TotalCapacity = item.TotalCapacity,
                        TotalAvailable = item.TotalAvailable,
                        SlotStartMins = slotStartMinutes,
                        SlotEndMins = slotEndMinutes,
                        SlotCounts = Array.Empty<int>(), // Equipmentでは使用しない
                        SlotCaps = item.SlotCaps ?? Array.Empty<int>(), // 空き率計算用
                        SlotAvailables = item.SlotAvailables ?? Array.Empty<int>(),
                        SlotFlags = null, // 将来的にIsOutsideHoursなどを設定
                        SlotFilteredCounts = null, // 現時点では使用しない
                        IsDayGrayedOut = false, // Equipmentでは使用しない
                        ResourceTypeCode = item.ResourceTypeCode,
                        PlanTypeCode = null // プランタイプは現時点では未対応
                    });
                }

                return groupedByDate;
            },
            ex => $"Failed to retrieve equipment resource slots for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            ex =>
            {
                var (tenantId, facilityId, userId) = this.GetTenantContext();
                LogMessages.EquipmentResourceSlotsRetrievalError((ILogger<AppointmentStatsRepository>)this.Logger, startDate, endDate, tenantId, facilityId, userId, ex);
            });
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, List<ResourceStatSlotsDto>>> GetEquipmentResourceSlotsAsArraysByDateWithOrGroupsAsync(
        DateOnly startDate,
        DateOnly endDate,
        List<Guid>? or1ResourceIds,
        List<Guid>? or2ResourceIds)
    {
        // 両方のORグループが空の場合は空の辞書を返す
        if ((or1ResourceIds == null || !or1ResourceIds.Any()) &&
            (or2ResourceIds == null || !or2ResourceIds.Any()))
        {
            ((ILogger<AppointmentStatsRepository>)this.Logger).LogDebug("GetEquipmentResourceSlotsAsArraysByDateWithOrGroupsAsync: both OR groups are empty, returning empty dict");
            return new Dictionary<string, List<ResourceStatSlotsDto>>();
        }

        ((ILogger<AppointmentStatsRepository>)this.Logger).LogDebug("GetEquipmentResourceSlotsAsArraysByDateWithOrGroupsAsync: DateRange={StartDate:yyyy-MM-dd}~{EndDate:yyyy-MM-dd}, OR1 count={Or1Count}, OR2 count={Or2Count}",
            startDate, endDate, or1ResourceIds?.Count ?? 0, or2ResourceIds?.Count ?? 0);

        // ORグループ条件を構築
        // 注意: appointment_stat_slotsは統計データなので、実際にはリソースIDのフィルタリングのみ
        // AND(OR1, OR2)の条件は、予約リソース要件テーブルとのJOINが必要だが、
        // 統計データのフィルタリングとしては、OR1またはOR2のいずれかに一致するリソースを返す
        var allResourceIds = new List<Guid>();
        if (or1ResourceIds != null && or1ResourceIds.Any())
        {
            allResourceIds.AddRange(or1ResourceIds);
        }
        if (or2ResourceIds != null && or2ResourceIds.Any())
        {
            allResourceIds.AddRange(or2ResourceIds);
        }

        // 重複を除去
        allResourceIds = allResourceIds.Distinct().ToList();

        // 既存のメソッドを呼び出して結果を取得
        return await this.GetEquipmentResourceSlotsAsArraysByDateAsync(startDate, endDate, allResourceIds);
    }

    /// <summary>
    /// 指定された日付範囲のStatスロットを取得します。
    /// AppointmentStatsの削除された navigation property に代わるメソッド
    /// </summary>
    public async Task<Dictionary<(DateOnly ApptDate, Guid ApptResId), List<AppointmentStatSlots>>> GetStatSlotsByDateRangeAsync(
        DateOnly startDate,
        DateOnly endDate,
        List<Guid>? resourceIds = null)
    {
        return await this.ExecuteQueryAsync(
            async () =>
            {
                var query = this.Context.AppointmentStatSlots
                    .AsNoTracking()
                    .Where(s => !s.IsDeleted &&
                                s.ApptDate >= startDate &&
                                s.ApptDate <= endDate);

                if (resourceIds != null && resourceIds.Any())
                {
                    query = query.Where(s => resourceIds.Contains(s.ApptResId));
                }

                var slots = await query.OrderBy(s => s.ApptDate).ThenBy(s => s.ApptResId).ThenBy(s => s.SlotStartMin).ToListAsync();

                // Group by (ApptDate, ApptResId) for easy lookup
                var result = new Dictionary<(DateOnly, Guid), List<AppointmentStatSlots>>();
                foreach (var slot in slots)
                {
                    var key = (slot.ApptDate, slot.ApptResId);
                    if (!result.ContainsKey(key))
                    {
                        result[key] = new List<AppointmentStatSlots>();
                    }
                    result[key].Add(slot);
                }

                return result;
            },
            ex => $"Failed to retrieve stat slots for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            ex =>
            {
                var (tenantId, facilityId, userId) = this.GetTenantContext();
                LogMessages.StatSlotsRetrievalError((ILogger<AppointmentStatsRepository>)this.Logger, startDate, endDate, tenantId, facilityId, userId, ex);
            });
    }

    /// <inheritdoc />
    public async Task UpsertStatsAndSlotsAsync(
        Data.MedockDbContext context,
        List<DateOnly> dates,
        List<Guid> resourceIds,
        List<Data.Entities.AppointmentStats> newStats,
        List<Data.Entities.AppointmentStatSlots> newSlots)
    {
        // 既存Stats/StatSlotsをソフト削除
        var existingStats = await context.AppointmentStats
            .Where(s => dates.Contains(s.ApptDate) &&
                       resourceIds.Contains(s.ApptResId) &&
                       !s.IsDeleted)
            .ToListAsync();

        foreach (var stat in existingStats)
        {
            stat.IsDeleted = true;
            stat.UpdatedAt = this.DateTimeProvider.NowRoundedToSeconds;
        }

        var existingSlots = await context.AppointmentStatSlots
            .Where(s => dates.Contains(s.ApptDate) &&
                       resourceIds.Contains(s.ApptResId) &&
                       !s.IsDeleted)
            .ToListAsync();

        foreach (var slot in existingSlots)
        {
            slot.IsDeleted = true;
            slot.UpdatedAt = this.DateTimeProvider.NowRoundedToSeconds;
        }

        // 新規レコード挿入
        await context.AppointmentStats.AddRangeAsync(newStats);
        await context.AppointmentStatSlots.AddRangeAsync(newSlots);

        // Note: SaveChanges は呼び出さない（呼び出し側がトランザクション管理）
    }

    // FromSql用の中間DTO
    private class EquipmentStatsWithDateDto
    {
        public string ApptDate { get; set; } = String.Empty;
        public string ResourceId { get; set; } = String.Empty;
        public string ResourceName { get; set; } = String.Empty;
        public int ResourceTypeCode { get; set; }
        public int TotalCapacity { get; set; }
        public int TotalAvailable { get; set; }
        public int[]? SlotStartMinutes { get; set; }
        public int[]? SlotEndMinutes { get; set; }
        public int[]? SlotCaps { get; set; }
        public int[]? SlotAvailables { get; set; }
    }

}
