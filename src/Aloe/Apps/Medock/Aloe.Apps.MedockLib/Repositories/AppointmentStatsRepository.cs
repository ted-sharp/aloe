using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Constants;
using Microsoft.EntityFrameworkCore;

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
}
