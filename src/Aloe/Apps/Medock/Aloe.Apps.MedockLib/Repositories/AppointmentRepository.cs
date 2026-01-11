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
public class AppointmentRepository : RepositoryBase, IAppointmentRepository
{
    public AppointmentRepository(
        MedockDbContext context,
        ILogger<AppointmentRepository> logger,
        IUserContextService userContextService,
        IDateTimeProvider dateTimeProvider)
        : base(context, logger, userContextService, dateTimeProvider)
    {
    }

    /// <summary>
    /// IDで予約を取得します（読み取り用、変更追跡なし）。
    /// </summary>
    public async Task<Appointment?> GetByIdAsync(Guid apptId)
    {
        try
        {
            return await this.Context.Appointments
                .AsNoTracking()
                .Include(a => a.Floor)
                .Include(a => a.Organization)
                .Include(a => a.Patient)
                .Include(a => a.AppointmentResourceAssignments)
                    .ThenInclude(r => r.AppointmentResource)
                .FirstOrDefaultAsync(a => a.ApptId == apptId && !a.IsDeleted);
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this.GetTenantContext();
            LogMessages.AppointmentRetrievalError((ILogger<AppointmentRepository>)this.Logger, apptId, tenantId, facilityId, userId, ex);
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
            return await this.Context.Appointments
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
            var (tenantId, facilityId, userId) = this.GetTenantContext();
            LogMessages.AppointmentsRetrievalError((ILogger<AppointmentRepository>)this.Logger, startDate, endDate, tenantId, facilityId, userId, ex);
            throw new DatabaseException($"Failed to retrieve appointments for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}", ex);
        }
    }

    /// <summary>
    /// 予約を追加します。
    /// </summary>
    public async Task AddAsync(Appointment appointment)
    {
        try
        {
            this.Context.Appointments.Add(appointment);
            await this.Context.SaveChangesAsync();

            var (tenantId, facilityId, userId) = this.GetTenantContext();
            LogMessages.AppointmentCreated((ILogger<AppointmentRepository>)this.Logger, appointment.ApptId, tenantId, facilityId, userId);
        }
        catch (DbUpdateException ex)
        {
            var (tenantId, facilityId, userId) = this.GetTenantContext();
            LogMessages.AppointmentCreateError((ILogger<AppointmentRepository>)this.Logger, appointment.ApptId, tenantId, facilityId, userId, ex);
            throw new DatabaseException($"Database error while creating appointment {appointment.ApptId}", ex);
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this.GetTenantContext();
            LogMessages.AppointmentCreateError((ILogger<AppointmentRepository>)this.Logger, appointment.ApptId, tenantId, facilityId, userId, ex);
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
            this.Context.Appointments.Update(appointment);
            await this.Context.SaveChangesAsync();

            var (tenantId, facilityId, userId) = this.GetTenantContext();
            LogMessages.AppointmentUpdated((ILogger<AppointmentRepository>)this.Logger, appointment.ApptId, tenantId, facilityId, userId);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var (tenantId, facilityId, userId) = this.GetTenantContext();
            LogMessages.AppointmentConcurrencyError((ILogger<AppointmentRepository>)this.Logger, appointment.ApptId, tenantId, facilityId, userId, ex);
            throw new ConcurrencyException($"Appointment {appointment.ApptId} was modified by another user", ex);
        }
        catch (DbUpdateException ex)
        {
            var (tenantId, facilityId, userId) = this.GetTenantContext();
            LogMessages.AppointmentUpdateError((ILogger<AppointmentRepository>)this.Logger, appointment.ApptId, tenantId, facilityId, userId, ex);
            throw new DatabaseException($"Database error while updating appointment {appointment.ApptId}", ex);
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this.GetTenantContext();
            LogMessages.AppointmentUpdateError((ILogger<AppointmentRepository>)this.Logger, appointment.ApptId, tenantId, facilityId, userId, ex);
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
            var (tenantId, facilityId, userId) = this.GetTenantContext();
            var appointment = await this.Context.Appointments.FindAsync(apptId);
            if (appointment == null)
            {
                LogMessages.AppointmentNotFoundForDeletion((ILogger<AppointmentRepository>)this.Logger, apptId, tenantId, facilityId, userId);
                throw new NotFoundException("Appointment", apptId);
            }

            appointment.IsDeleted = true;
            await this.Context.SaveChangesAsync();

            LogMessages.AppointmentDeleted((ILogger<AppointmentRepository>)this.Logger, apptId, tenantId, facilityId, userId);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            var (tenantId, facilityId, userId) = this.GetTenantContext();
            LogMessages.AppointmentDeleteError((ILogger<AppointmentRepository>)this.Logger, apptId, tenantId, facilityId, userId, ex);
            throw new DatabaseException($"Database error while deleting appointment {apptId}", ex);
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this.GetTenantContext();
            LogMessages.AppointmentDeleteError((ILogger<AppointmentRepository>)this.Logger, apptId, tenantId, facilityId, userId, ex);
            throw;
        }
    }



    /// <inheritdoc />
    public async Task<Appointment?> FindForUpdateAsync(Guid apptId)
    {
        try
        {
            return await this.Context.Appointments
                .Include(a => a.Floor)
                .Include(a => a.Organization)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.ApptId == apptId && !a.IsDeleted);
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this.GetTenantContext();
            LogMessages.AppointmentRetrievalError((ILogger<AppointmentRepository>)this.Logger, apptId, tenantId, facilityId, userId, ex);
            throw new DatabaseException($"Failed to retrieve appointment {apptId} for update", ex);
        }
    }

}

