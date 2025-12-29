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
public class HolidayRepository : IHolidayRepository
{
    private readonly MedockDbContext _context;
    private readonly ILogger<HolidayRepository> _logger;
    private readonly IUserContextService _userContextService;

    public HolidayRepository(
        MedockDbContext context,
        ILogger<HolidayRepository> logger,
        IUserContextService userContextService)
    {
        this._context = context;
        this._logger = logger;
        this._userContextService = userContextService;
    }

    /// <inheritdoc />
    public async Task<List<Holiday>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        try
        {
            return await this._context.Holidays
                .AsNoTracking()
                .Where(h => !h.IsDeleted &&
                            h.HolidayDate >= startDate &&
                            h.HolidayDate <= endDate)
                .OrderBy(h => h.HolidayDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = _userContextService.GetTenantContext();
            LogMessages.AppointmentsRetrievalError(_logger, startDate, endDate, tenantId, facilityId, userId, ex);
            throw new DatabaseException($"Failed to retrieve holidays for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}", ex);
        }
    }
}

