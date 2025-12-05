using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockLib.Repositories;

/// <summary>
/// 予約リポジトリ
/// </summary>
public class AppointmentRepository
{
    private readonly MedockDbContext _context;

    public AppointmentRepository(MedockDbContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// IDで予約を取得します。
    /// </summary>
    public async Task<Appointment?> GetByIdAsync(Guid apptId)
    {
        return await this._context.Appointments
            .Include(a => a.Floor)
            .Include(a => a.Organization)
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.ApptId == apptId && !a.IsDeleted);
    }

    /// <summary>
    /// 日付範囲で予約を取得します。
    /// </summary>
    public async Task<List<Appointment>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await this._context.Appointments
            .Include(a => a.Floor)
            .Include(a => a.Organization)
            .Include(a => a.Patient)
            .Where(a => !a.IsDeleted &&
                        a.ApptDate >= startDate &&
                        a.ApptDate <= endDate)
            .OrderBy(a => a.ApptDate)
            .ThenBy(a => a.ApptStartAt)
            .ToListAsync();
    }

    /// <summary>
    /// フロアと日付で予約を取得します。
    /// </summary>
    public async Task<List<Appointment>> GetByFloorAndDateAsync(Guid floorId, DateOnly date)
    {
        return await this._context.Appointments
            .Include(a => a.Floor)
            .Include(a => a.Organization)
            .Include(a => a.Patient)
            .Where(a => !a.IsDeleted &&
                        a.FloorId == floorId &&
                        a.ApptDate == date)
            .OrderBy(a => a.ApptStartAt)
            .ToListAsync();
    }

    /// <summary>
    /// 患者IDで予約を取得します。
    /// </summary>
    public async Task<List<Appointment>> GetByPatientIdAsync(Guid ptId)
    {
        return await this._context.Appointments
            .Include(a => a.Floor)
            .Include(a => a.Organization)
            .Where(a => !a.IsDeleted && a.PtId == ptId)
            .OrderByDescending(a => a.ApptDate)
            .ThenByDescending(a => a.ApptStartAt)
            .ToListAsync();
    }

    /// <summary>
    /// 団体IDで予約を取得します。
    /// </summary>
    public async Task<List<Appointment>> GetByOrganizationIdAsync(Guid orgId)
    {
        return await this._context.Appointments
            .Include(a => a.Floor)
            .Include(a => a.Patient)
            .Where(a => !a.IsDeleted && a.OrgId == orgId)
            .OrderByDescending(a => a.ApptDate)
            .ThenByDescending(a => a.ApptStartAt)
            .ToListAsync();
    }

    /// <summary>
    /// 予約を追加します。
    /// </summary>
    public async Task AddAsync(Appointment appointment)
    {
        this._context.Appointments.Add(appointment);
        await this._context.SaveChangesAsync();
    }

    /// <summary>
    /// 予約を更新します。
    /// </summary>
    public async Task UpdateAsync(Appointment appointment)
    {
        this._context.Appointments.Update(appointment);
        await this._context.SaveChangesAsync();
    }

    /// <summary>
    /// 予約を論理削除します。
    /// </summary>
    public async Task DeleteAsync(Guid apptId)
    {
        var appointment = await this._context.Appointments.FindAsync(apptId);
        if (appointment != null)
        {
            appointment.IsDeleted = true;
            await this._context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 指定日の予約件数を取得します。
    /// </summary>
    public async Task<int> GetCountByDateAsync(DateOnly date)
    {
        return await this._context.Appointments
            .CountAsync(a => !a.IsDeleted && a.ApptDate == date);
    }

    /// <summary>
    /// 指定フロア・日付のステータス別予約件数を取得します。
    /// </summary>
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
}


