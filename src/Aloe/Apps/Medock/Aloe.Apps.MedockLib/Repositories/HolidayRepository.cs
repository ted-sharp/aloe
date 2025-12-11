using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockLib.Repositories;

/// <summary>
/// 祝日リポジトリ
/// </summary>
public class HolidayRepository : IHolidayRepository
{
    private readonly MedockDbContext _context;

    public HolidayRepository(MedockDbContext context)
    {
        this._context = context;
    }

    /// <inheritdoc />
    public async Task<List<Holiday>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await this._context.Holidays
            .Where(h => !h.IsDeleted &&
                        h.HolidayDate >= startDate &&
                        h.HolidayDate <= endDate)
            .OrderBy(h => h.HolidayDate)
            .ToListAsync();
    }
}

