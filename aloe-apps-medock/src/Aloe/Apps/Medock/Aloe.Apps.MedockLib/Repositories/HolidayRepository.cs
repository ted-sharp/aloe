using Aloe.Apps.MedockLib.Common.Exceptions;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Logging;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockLib.Repositories;

/// <summary>
/// 祝日リポジトリ
/// </summary>
public class HolidayRepository : RepositoryBase, IHolidayRepository
{
    public HolidayRepository(
        MedockDbContext context,
        ILogger<HolidayRepository> logger,
        IUserContextService userContextService,
        IDateTimeProvider dateTimeProvider)
        : base(context, logger, userContextService, dateTimeProvider)
    {
    }

    /// <inheritdoc />
    public async Task<List<Holiday>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await this.ExecuteQueryAsync(
            () => this.Context.Holidays
                .AsNoTracking()
                .Where(h => !h.IsDeleted &&
                            h.HolidayDate >= startDate &&
                            h.HolidayDate <= endDate)
                .OrderBy(h => h.HolidayDate)
                .ToListAsync(),
            ex => $"Failed to retrieve holidays for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            ex =>
            {
                var (tenantId, facilityId, userId) = this.GetTenantContext();
                LogMessages.HolidaysRetrievalError((ILogger<HolidayRepository>)this.Logger, startDate, endDate, tenantId, facilityId, userId, ex);
            });
    }
}

