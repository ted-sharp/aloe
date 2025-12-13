using Aloe.Apps.MedockLib.Data;
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
            .CountAsync(a => !a.IsDeleted && a.ApptDate == date);
    }

    /// <inheritdoc />
    public async Task<Dictionary<int, int>> GetStatusCountByFloorAndDateAsync(Guid floorId, DateOnly date)
    {
        return await this._context.Appointments
            .Where(a => !a.IsDeleted &&
                        a.FloorId == floorId &&
                        a.ApptDate == date)
            .GroupBy(a => a.ApptStatusCode)
            .Select(g => new { StatusCode = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.StatusCode, x => x.Count);
    }

    /// <inheritdoc />
    public async Task<List<(DateOnly? ApptDate, TimeOnly? ApptStartTime)>> GetForDayStatsAsync(DateOnly startDate, DateOnly endDate)
    {
        var results = await this._context.Appointments
            .Where(a => !a.IsDeleted &&
                        a.ApptDate.HasValue &&
                        a.ApptDate >= startDate &&
                        a.ApptDate <= endDate)
            .Select(a => new { a.ApptDate, a.ApptStartTime })
            .ToListAsync();

        return results.Select(x => (x.ApptDate, x.ApptStartTime)).ToList();
    }
}
