using Aloe.Apps.MedockLib.Common.Exceptions;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Logging;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockLib.Repositories;

/// <summary>
/// 予約リポジトリ
/// </summary>
public class AppointmentRepository : IAppointmentRepository
{
    private readonly MedockDbContext _context;
    private readonly ILogger<AppointmentRepository> _logger;
    private readonly IUserContextService _userContextService;

    public AppointmentRepository(
        MedockDbContext context,
        ILogger<AppointmentRepository> logger,
        IUserContextService userContextService)
    {
        this._context = context;
        this._logger = logger;
        this._userContextService = userContextService;
    }

    /// <summary>
    /// IDで予約を取得します（読み取り用、変更追跡なし）。
    /// </summary>
    public async Task<Appointment?> GetByIdAsync(Guid apptId)
    {
        try
        {
            return await this._context.Appointments
                .AsNoTracking()
                .Include(a => a.Floor)
                .Include(a => a.Organization)
                .Include(a => a.Patient)
                .Include(a => a.AppointmentResourceReservations)
                    .ThenInclude(r => r.AppointmentResource)
                .FirstOrDefaultAsync(a => a.ApptId == apptId && !a.IsDeleted);
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentRetrievalError(this._logger, apptId, tenantId, facilityId, userId, ex);
            throw new DatabaseException($"Failed to retrieve appointment {apptId}", ex);
        }
    }

    /// <summary>
    /// 日付範囲で予約を取得します。
    /// </summary>
    public async Task<List<Appointment>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        try
        {
            return await this._context.Appointments
                .AsNoTracking()
                .Include(a => a.Floor)
                .Include(a => a.Organization)
                .Include(a => a.Patient)
                .Where(a => !a.IsDeleted &&
                            a.ApptDate.HasValue &&
                            a.ApptDate >= startDate &&
                            a.ApptDate <= endDate)
                .OrderBy(a => a.ApptDate)
                .ThenBy(a => a.ApptStartMin)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentsRetrievalError(this._logger, startDate, endDate, tenantId, facilityId, userId, ex);
            throw new DatabaseException($"Failed to retrieve appointments for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}", ex);
        }
    }

    /// <summary>
    /// フロアと日付で予約を取得します。
    /// </summary>
    public async Task<List<Appointment>> GetByFloorAndDateAsync(Guid floorId, DateOnly date)
    {
        try
        {
            return await this._context.Appointments
                .AsNoTracking()
                .Include(a => a.Floor)
                .Include(a => a.Organization)
                .Include(a => a.Patient)
                .Where(a => !a.IsDeleted &&
                            a.FloorId == floorId &&
                            a.ApptDate == date)
                .OrderBy(a => a.ApptStartMin)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentsRetrievalError(this._logger, date, date, tenantId, facilityId, userId, ex);
            throw new DatabaseException($"Failed to retrieve appointments for floor {floorId} on date {date:yyyy-MM-dd}", ex);
        }
    }

    /// <summary>
    /// 患者IDで予約を取得します。
    /// </summary>
    public async Task<List<Appointment>> GetByPatientIdAsync(Guid ptId)
    {
        try
        {
            return await this._context.Appointments
                .AsNoTracking()
                .Include(a => a.Floor)
                .Include(a => a.Organization)
                .Where(a => !a.IsDeleted && a.PtId == ptId)
                .OrderByDescending(a => a.ApptDate)
                .ThenByDescending(a => a.ApptStartMin)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentCreateFailed(this._logger, ptId, DateOnly.FromDateTime(DateTime.Now), tenantId, facilityId, userId, ex);
            throw new DatabaseException($"Failed to retrieve appointments for patient {ptId}", ex);
        }
    }

    /// <summary>
    /// 団体IDで予約を取得します。
    /// </summary>
    public async Task<List<Appointment>> GetByOrganizationIdAsync(Guid orgId)
    {
        try
        {
            return await this._context.Appointments
                .AsNoTracking()
                .Include(a => a.Floor)
                .Include(a => a.Patient)
                .Where(a => !a.IsDeleted && a.OrgId == orgId)
                .OrderByDescending(a => a.ApptDate)
                .ThenByDescending(a => a.ApptStartMin)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentCreateFailed(this._logger, null, DateOnly.FromDateTime(DateTime.Now), tenantId, facilityId, userId, ex);
            throw new DatabaseException($"Failed to retrieve appointments for organization {orgId}", ex);
        }
    }

    /// <summary>
    /// 予約を追加します。
    /// </summary>
    public async Task AddAsync(Appointment appointment)
    {
        try
        {
            this._context.Appointments.Add(appointment);
            await this._context.SaveChangesAsync();

            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentCreated(this._logger, appointment.ApptId, tenantId, facilityId, userId);
        }
        catch (DbUpdateException ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentCreateError(this._logger, appointment.ApptId, tenantId, facilityId, userId, ex);
            throw new DatabaseException($"Database error while creating appointment {appointment.ApptId}", ex);
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentCreateError(this._logger, appointment.ApptId, tenantId, facilityId, userId, ex);
            throw;
        }
    }

    /// <summary>
    /// 予約を更新します。
    /// </summary>
    public async Task UpdateAsync(Appointment appointment)
    {
        try
        {
            this._context.Appointments.Update(appointment);
            await this._context.SaveChangesAsync();

            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentUpdated(this._logger, appointment.ApptId, tenantId, facilityId, userId);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentConcurrencyError(this._logger, appointment.ApptId, tenantId, facilityId, userId, ex);
            throw new ConcurrencyException($"Appointment {appointment.ApptId} was modified by another user", ex);
        }
        catch (DbUpdateException ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentCreateError(this._logger, appointment.ApptId, tenantId, facilityId, userId, ex);
            throw new DatabaseException($"Database error while updating appointment {appointment.ApptId}", ex);
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentCreateError(this._logger, appointment.ApptId, tenantId, facilityId, userId, ex);
            throw;
        }
    }

    /// <summary>
    /// 予約を論理削除します。
    /// </summary>
    public async Task DeleteAsync(Guid apptId)
    {
        try
        {
            var appointment = await this._context.Appointments.FindAsync(apptId);
            if (appointment == null)
            {
                var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
                LogMessages.AppointmentNotFoundForDeletion(this._logger, apptId, tenantId, facilityId, userId);
                throw new NotFoundException("Appointment", apptId);
            }

            appointment.IsDeleted = true;
            await this._context.SaveChangesAsync();

            var (tenantId2, facilityId2, userId2) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentDeleted(this._logger, apptId, tenantId2, facilityId2, userId2);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentCreateError(this._logger, apptId, tenantId, facilityId, userId, ex);
            throw new DatabaseException($"Database error while deleting appointment {apptId}", ex);
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentCreateError(this._logger, apptId, tenantId, facilityId, userId, ex);
            throw;
        }
    }



    /// <inheritdoc />
    public async Task<Appointment?> FindForUpdateAsync(Guid apptId)
    {
        try
        {
            return await this._context.Appointments
                .Include(a => a.Floor)
                .Include(a => a.Organization)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.ApptId == apptId && !a.IsDeleted);
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentRetrievalError(this._logger, apptId, tenantId, facilityId, userId, ex);
            throw new DatabaseException($"Failed to retrieve appointment {apptId} for update", ex);
        }
    }
}





