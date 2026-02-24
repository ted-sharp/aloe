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
        return await this.ExecuteQueryAsync(
            () => this.CreateAppointmentQuery()
                .FirstOrDefaultAsync(a => a.ApptId == apptId),
            ex => $"Failed to retrieve appointment {apptId}",
            ex =>
            {
                var (tenantId, facilityId, userId) = this.GetTenantContext();
                LogMessages.AppointmentRetrievalError((ILogger<AppointmentRepository>)this.Logger, apptId, tenantId, facilityId, userId, ex);
            });
    }

    /// <summary>
    /// 日付範囲で予約を取得します。
    /// </summary>
    public async Task<List<Appointment>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await this.ExecuteQueryAsync(
            () => this.CreateAppointmentQuery()
                .Where(a => a.ApptDate.HasValue &&
                            a.ApptDate >= startDate &&
                            a.ApptDate <= endDate)
                .OrderBy(a => a.ApptDate)
                .ThenBy(a => a.ApptStartMin)
                .ToListAsync(),
            ex => $"Failed to retrieve appointments for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            ex =>
            {
                var (tenantId, facilityId, userId) = this.GetTenantContext();
                LogMessages.AppointmentsRetrievalError((ILogger<AppointmentRepository>)this.Logger, startDate, endDate, tenantId, facilityId, userId, ex);
            });
    }

    /// <summary>
    /// 予約を追加します。
    /// </summary>
    public async Task AddAsync(Appointment appointment)
    {
        await this.ExecuteCommandAsync(
            async () =>
            {
                this.Context.Appointments.Add(appointment);
                await this.Context.SaveChangesAsync();
            },
            ex => $"Database error while creating appointment {appointment.ApptId}",
            ex => $"Concurrency error while creating appointment {appointment.ApptId}",
            ex =>
            {
                var (tenantId, facilityId, userId) = this.GetTenantContext();
                LogMessages.AppointmentCreateError((ILogger<AppointmentRepository>)this.Logger, appointment.ApptId, tenantId, facilityId, userId, ex);
            });

        var (txnTenantId, txnFacilityId, txnUserId) = this.GetTenantContext();
        LogMessages.AppointmentCreated((ILogger<AppointmentRepository>)this.Logger, appointment.ApptId, txnTenantId, txnFacilityId, txnUserId);
    }

    /// <summary>
    /// 予約を更新します。
    /// </summary>
    public async Task UpdateAsync(Appointment appointment)
    {
        await this.ExecuteCommandAsync(
            async () =>
            {
                this.Context.Appointments.Update(appointment);
                await this.Context.SaveChangesAsync();
            },
            ex => $"Database error while updating appointment {appointment.ApptId}",
            ex => $"Appointment {appointment.ApptId} was modified by another user",
            ex =>
            {
                var (tenantId, facilityId, userId) = this.GetTenantContext();
                LogMessages.AppointmentUpdateError((ILogger<AppointmentRepository>)this.Logger, appointment.ApptId, tenantId, facilityId, userId, ex);
            });

        var (txnTenantId, txnFacilityId, txnUserId) = this.GetTenantContext();
        LogMessages.AppointmentUpdated((ILogger<AppointmentRepository>)this.Logger, appointment.ApptId, txnTenantId, txnFacilityId, txnUserId);
    }

    /// <summary>
    /// 予約を論理削除します。
    /// </summary>
    public async Task DeleteAsync(Guid apptId)
    {
        var (tenantId, facilityId, userId) = this.GetTenantContext();
        var appointment = await this.Context.Appointments.FindAsync(apptId);
        if (appointment == null)
        {
            LogMessages.AppointmentNotFoundForDeletion((ILogger<AppointmentRepository>)this.Logger, apptId, tenantId, facilityId, userId);
            throw new NotFoundException("Appointment", apptId);
        }

        await this.ExecuteCommandAsync(
            async () =>
            {
                appointment.IsDeleted = true;
                await this.Context.SaveChangesAsync();
            },
            ex => $"Database error while deleting appointment {apptId}",
            ex => $"Concurrency error while deleting appointment {apptId}",
            ex =>
            {
                LogMessages.AppointmentDeleteError((ILogger<AppointmentRepository>)this.Logger, apptId, tenantId, facilityId, userId, ex);
            });

        LogMessages.AppointmentDeleted((ILogger<AppointmentRepository>)this.Logger, apptId, tenantId, facilityId, userId);
    }



    /// <inheritdoc />
    public async Task<Appointment?> FindForUpdateAsync(Guid apptId)
    {
        return await this.ExecuteQueryAsync(
            () => this.Context.Appointments
                .Include(a => a.Floor)
                .Include(a => a.Organization)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.ApptId == apptId && !a.IsDeleted),
            ex => $"Failed to retrieve appointment {apptId} for update",
            ex =>
            {
                var (tenantId, facilityId, userId) = this.GetTenantContext();
                LogMessages.AppointmentRetrievalError((ILogger<AppointmentRepository>)this.Logger, apptId, tenantId, facilityId, userId, ex);
            });
    }

}

