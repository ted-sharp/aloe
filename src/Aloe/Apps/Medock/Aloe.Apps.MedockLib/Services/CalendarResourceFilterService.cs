using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// カレンダーリソースフィルタリングサービス実装
/// </summary>
public class CalendarResourceFilterService : ICalendarResourceFilterService
{
    private readonly IDbContextFactory<MedockDbContext> _dbContextFactory;
    private readonly ILogger<CalendarResourceFilterService> _logger;

    public CalendarResourceFilterService(
        IDbContextFactory<MedockDbContext> dbContextFactory,
        ILogger<CalendarResourceFilterService> logger)
    {
        this._dbContextFactory = dbContextFactory;
        this._logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<Guid>> GetRelatedResourceIdsAsync(
        IEnumerable<Guid> floorIds,
        IEnumerable<Guid> planIds)
    {
        var autoResourceIds = new HashSet<Guid>();

        try
        {
            await using var context = await this._dbContextFactory.CreateDbContextAsync();

            var floorIdList = floorIds.ToList();
            var planIdList = planIds.ToList();

            // 選択されているフロアに属するリソース
            if (floorIdList.Any())
            {
                var floorResourceIds = await context.AppointmentResources
                    .AsNoTracking()
                    .Where(r => !r.IsDeleted &&
                               floorIdList.Contains(r.FloorId) &&
                               r.ApptResTypeCode == (int)AppointmentResourceType.Equipment)
                    .Select(r => r.ApptResId)
                    .Distinct()
                    .ToListAsync();

                foreach (var resourceId in floorResourceIds)
                {
                    autoResourceIds.Add(resourceId);
                }
            }

            // 選択されているプランに関連するリソース
            if (planIdList.Any())
            {
                var planResourceIds = await context.PlanResourceRequirements
                    .AsNoTracking()
                    .Where(prr => !prr.IsDeleted &&
                                 planIdList.Contains(prr.PlanId) &&
                                 prr.AppointmentResource.ApptResTypeCode == (int)AppointmentResourceType.Equipment &&
                                 !prr.AppointmentResource.IsDeleted)
                    .Select(prr => prr.ApptResId)
                    .Distinct()
                    .ToListAsync();

                foreach (var resourceId in planResourceIds)
                {
                    autoResourceIds.Add(resourceId);
                }
            }
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting related resource IDs for calendar filtering");
        }

        return autoResourceIds.ToList();
    }
}
