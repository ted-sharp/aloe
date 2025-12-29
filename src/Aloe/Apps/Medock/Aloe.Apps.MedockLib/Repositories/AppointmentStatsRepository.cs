using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Services.Dtos;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace Aloe.Apps.MedockLib.Repositories;

/// <summary>
/// 予約統計リポジトリ
/// </summary>
public class AppointmentStatsRepository : IAppointmentStatsRepository
{
    private readonly MedockDbContext _context;

    public AppointmentStatsRepository(MedockDbContext context)
    {
        this._context = context;
    }

    /// <inheritdoc />
    public async Task<int> GetCountByDateAsync(DateOnly date)
    {
        return await this._context.Appointments
            .AsNoTracking()
            .CountAsync(a => !a.IsDeleted && a.ApptDate == date);
    }

    /// <inheritdoc />
    public async Task<Dictionary<int, int>> GetStatusCountByFloorAndDateAsync(Guid floorId, DateOnly date)
    {
        return await this._context.Appointments
            .AsNoTracking()
            .Where(a => !a.IsDeleted &&
                        a.FloorId == floorId &&
                        a.ApptDate == date)
            .GroupBy(a => a.ApptStatusCode)
            .Select(g => new { StatusCode = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.StatusCode, x => x.Count);
    }

    /// <inheritdoc />
    public async Task<List<(DateOnly? ApptDate, TimeOnly? ApptStartTime)>> GetForMainStatsAsync(DateOnly startDate, DateOnly endDate)
    {
        var results = await this._context.Appointments
            .AsNoTracking()
            .Where(a => !a.IsDeleted &&
                        a.ApptDate.HasValue &&
                        a.ApptDate >= startDate &&
                        a.ApptDate <= endDate)
            .Select(a => new { a.ApptDate, a.ApptStartTime })
            .ToListAsync();

        return results.Select(x => (x.ApptDate, x.ApptStartTime)).ToList();
    }

    /// <inheritdoc />
    public async Task<List<Data.Entities.AppointmentStats>> GetMainResourceStatsByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await this._context.AppointmentStats
            .AsNoTracking()
            .Include(s => s.AppointmentResource)
            .Include(s => s.AppointmentStatSlots)
            .Where(s => !s.IsDeleted &&
                        !s.AppointmentResource.IsDeleted &&
                        s.AppointmentResource.ApptResTypeCode == (int)AppointmentResourceType.Main &&
                        s.ApptDate >= startDate &&
                        s.ApptDate <= endDate)
            .ToListAsync();
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
        var query = this._context.AppointmentStats
            .AsNoTracking()
            .Include(s => s.AppointmentResource)
                .ThenInclude(r => r.Floor)
            .Include(s => s.AppointmentResource)
                .ThenInclude(r => r.AppointmentResourceGroupMembers)
                    .ThenInclude(m => m.AppointmentResourceGroup)
            .Include(s => s.AppointmentStatSlots)
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

        // リソースグループフィルター
        if (resourceGroupIds != null && resourceGroupIds.Any())
        {
            query = query.Where(s => s.AppointmentResource.AppointmentResourceGroupMembers
                .Any(m => !m.IsDeleted && resourceGroupIds.Contains(m.AppointmentResourceGroup.ApptResGroupId)));
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
                var planResourceIds = await this._context.PlanResourceRequirements
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
                var optionResourceIds = await this._context.PlanResourceRequirements
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
    }

    /// <inheritdoc />
    public async Task<List<Data.Entities.AppointmentStats>> GetMainResourceStatsByDateAndResourcesAsync(
        DateOnly date,
        List<Guid> resourceIds)
    {
        return await this._context.AppointmentStats
            .AsNoTracking()
            .Include(s => s.AppointmentResource)
            .Include(s => s.AppointmentStatSlots)
            .Where(s => !s.IsDeleted &&
                        !s.AppointmentResource.IsDeleted &&
                        s.AppointmentResource.ApptResTypeCode == (int)AppointmentResourceType.Main &&
                        s.ApptDate == date &&
                        resourceIds.Contains(s.ApptResId))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<Data.Entities.AppointmentStats>> GetEquipmentResourceStatsByDateRangeAsync(
        DateOnly startDate,
        DateOnly endDate,
        List<Guid> equipmentResourceIds)
    {
        var query = this._context.AppointmentStats
            .AsNoTracking()
            .Include(s => s.AppointmentResource)
            .Include(s => s.AppointmentStatSlots)
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
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, List<EquipmentResourceStatsDto>>> GetEquipmentResourceSlotsAsArraysByDateAsync(
        DateOnly startDate,
        DateOnly endDate,
        List<Guid>? equipmentResourceIds)
    {
        // equipmentResourceIds が null または空の場合は空の辞書を返す
        if (equipmentResourceIds == null || !equipmentResourceIds.Any())
        {
            Console.WriteLine($"[Debug] GetEquipmentResourceSlotsAsArraysByDateAsync: equipmentResourceIds is null or empty, returning empty dict");
            return new Dictionary<string, List<EquipmentResourceStatsDto>>();
        }

        Console.WriteLine($"[Debug] GetEquipmentResourceSlotsAsArraysByDateAsync: DateRange={startDate:yyyy-MM-dd}~{endDate:yyyy-MM-dd}, IDs count={equipmentResourceIds.Count}");

        // PostgreSQL の array_agg で SQL側で配列化
        // 注意: SqlQueryRaw では複雑な投影ができないため、raw queryで取得後にクライアント側で処理
        var sql = @"
            SELECT
                s.appt_date::text as ""ApptDate"",
                s.appt_res_id::text as ""ResourceId"",
                ar.appt_res_name as ""ResourceName"",
                COALESCE(SUM(s.appt_cap), 0)::int as ""TotalCapacity"",
                COALESCE(SUM(s.appt_available), 0)::int as ""TotalAvailable"",
                array_agg(ss.slot_start ORDER BY ss.slot_start)::int[] as ""SlotStartMinutes"",
                array_agg(ss.slot_end ORDER BY ss.slot_start)::int[] as ""SlotEndMinutes"",
                array_agg(ss.slot_available ORDER BY ss.slot_start)::int[] as ""SlotAvailables""
            FROM appointment_stats s
            INNER JOIN appointment_resources ar ON s.appt_res_id = ar.appt_res_id
            INNER JOIN appointment_stat_slots ss ON s.appt_stat_id = ss.appt_stat_id
            WHERE s.is_deleted = false
                AND ar.is_deleted = false
                AND ss.is_deleted = false
                AND ar.appt_res_type_code = @equipmentTypeCode
                AND s.appt_date >= @startDate
                AND s.appt_date <= @endDate
                AND s.appt_res_id = ANY(@equipmentIds::uuid[])
            GROUP BY s.appt_date, s.appt_res_id, ar.appt_res_name
            ORDER BY s.appt_date, ar.appt_res_name";

        var equipmentTypeCode = new NpgsqlParameter("@equipmentTypeCode", (int)AppointmentResourceType.Equipment);
        var startDateParam = new NpgsqlParameter("@startDate", startDate);
        var endDateParam = new NpgsqlParameter("@endDate", endDate);
        var equipmentIdsParam = new NpgsqlParameter("@equipmentIds", NpgsqlDbType.Array | NpgsqlDbType.Uuid) { Value = equipmentResourceIds.ToArray() };

        var parameters = new object[] { equipmentTypeCode, startDateParam, endDateParam, equipmentIdsParam };

        // 中間DTO で日付を含める
        try
        {
            var results = await this._context
                .Database
                .SqlQueryRaw<EquipmentStatsWithDateDto>(sql, parameters)
                .ToListAsync();

            Console.WriteLine($"[Debug] GetEquipmentResourceSlotsAsArraysByDateAsync: SQL returned {results.Count} rows");

            // 日付ごとにグループ化
            var groupedByDate = new Dictionary<string, List<EquipmentResourceStatsDto>>();
            foreach (var item in results)
            {
                if (!groupedByDate.ContainsKey(item.ApptDate))
                {
                    groupedByDate[item.ApptDate] = new List<EquipmentResourceStatsDto>();
                }
                groupedByDate[item.ApptDate].Add(new EquipmentResourceStatsDto
                {
                    ResourceId = item.ResourceId ?? string.Empty,
                    ResourceName = item.ResourceName ?? string.Empty,
                    TotalCapacity = item.TotalCapacity,
                    TotalAvailable = item.TotalAvailable,
                    SlotStartMinutes = item.SlotStartMinutes ?? Array.Empty<int>(),
                    SlotEndMinutes = item.SlotEndMinutes ?? Array.Empty<int>(),
                    SlotAvailables = item.SlotAvailables ?? Array.Empty<int>(),
                    SlotFlags = null
                });
            }

            return groupedByDate;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] GetEquipmentResourceSlotsAsArraysByDateAsync: {ex.Message}");
            Console.WriteLine($"[Error] Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[Error] Inner Exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    // FromSql用の中間DTO
    private class EquipmentStatsWithDateDto
    {
        public string ApptDate { get; set; } = string.Empty;
        public string ResourceId { get; set; } = string.Empty;
        public string ResourceName { get; set; } = string.Empty;
        public int TotalCapacity { get; set; }
        public int TotalAvailable { get; set; }
        public int[]? SlotStartMinutes { get; set; }
        public int[]? SlotEndMinutes { get; set; }
        public int[]? SlotAvailables { get; set; }
    }
}
