using Aloe.Apps.MedockLib.Common;
using Aloe.Apps.MedockLib.Common.Exceptions;
using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Logging;
using Aloe.Apps.MedockLib.Repositories;
using Aloe.Apps.MedockLib.Services.Dtos;
using Aloe.Apps.MedockLib.Services.Dtos.Appointments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 予約サービス実装
/// </summary>
public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly IUserContextService _userContextService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDbContextFactory<MedockDbContext> _dbContextFactory;
    private readonly ILogger<AppointmentService> _logger;
    private readonly IAppointmentResourceAssignmentService _resourceAssignmentService;
    private readonly IAppointmentStatsUpdateService _appointmentStatsUpdateService;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IHolidayRepository holidayRepository,
        IUserContextService userContextService,
        IDateTimeProvider dateTimeProvider,
        IDbContextFactory<MedockDbContext> dbContextFactory,
        ILogger<AppointmentService> logger,
        IAppointmentResourceAssignmentService resourceAssignmentService,
        IAppointmentStatsUpdateService appointmentStatsUpdateService)
    {
        this._appointmentRepository = appointmentRepository;
        this._holidayRepository = holidayRepository;
        this._userContextService = userContextService;
        this._dateTimeProvider = dateTimeProvider;
        this._dbContextFactory = dbContextFactory;
        this._logger = logger;
        this._resourceAssignmentService = resourceAssignmentService;
        this._appointmentStatsUpdateService = appointmentStatsUpdateService;
    }

    /// <inheritdoc />
    public async Task<Result<List<AppointmentDto>>> GetAppointmentsAsync(DateOnly startDate, DateOnly endDate)
    {
        return await ExecuteAsync(async () =>
        {
            var appointments = await _appointmentRepository.GetByDateRangeAsync(startDate, endDate);
            return appointments.Select(a => AppointmentMapper.MapToDto(a, _dateTimeProvider)).ToList();
        },
        (ex, t, f, u) => LogMessages.AppointmentsRetrievalError(_logger, startDate, endDate, t, f, u, ex),
        $"Failed to retrieve appointments for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
        "APPT_RETRIEVAL_ERROR");
    }

    /// <inheritdoc />
    public async Task<Result<AppointmentDto>> GetAppointmentAsync(Guid apptId)
    {
        return await ExecuteAsync(async () =>
        {
            var appointment = await _appointmentRepository.GetByIdAsync(apptId);
            if (appointment is null)
            {
                var (t, f, u) = _userContextService.GetTenantContext();
                LogMessages.AppointmentNotFound(_logger, apptId, t, f, u);
                throw new NotFoundException("Appointment", apptId);
            }
            return AppointmentMapper.MapToDto(appointment, _dateTimeProvider);
        },
        (ex, t, f, u) => LogMessages.AppointmentRetrievalError(_logger, apptId, t, f, u, ex),
        $"Failed to retrieve appointment {apptId}",
        "APPT_RETRIEVAL_ERROR",
        onNotFound: (ex) => Result<AppointmentDto>.Failure($"Appointment {apptId} not found", "APPT_NOT_FOUND"));
    }

    /// <inheritdoc />
    public async Task<Result<AppointmentDto>> CreateAppointmentAsync(CreateAppointmentDto dto)
    {
        return await ExecuteAsync(async () =>
        {
            var now = _dateTimeProvider.NowRoundedToSeconds;
            var appointment = new Appointment
            {
                ApptId = Guid.CreateVersion7(),
                ApptDate = dto.Date,
                ApptStartMin = dto.StartMin ?? BusinessHoursConstants.DefaultAppointmentStartMin,
                PtId = dto.PatientId,
                OrgId = dto.OrganizationId,
                FloorId = dto.FloorId,
                ApptStatusCode = dto.Status,
                ApptMemo = dto.Memo ?? String.Empty,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _appointmentRepository.AddAsync(appointment);

            await using var context = await _dbContextFactory.CreateDbContextAsync();

            // Mainリソースの割り当て（必ず含まれる）
            // Mainリソースは画面で選択するものではなく、予約のフロアに基づいて自動的に割り当てられる
            var mainResourceIds = await _resourceAssignmentService.AssignMainResourcesAsync(context, appointment.ApptId, dto.FloorId, now);

            // Equipmentリソースの割り当て（選択された場合のみ）
            var equipmentResourceIds = new List<Guid>();
            if (dto.EquipmentResourceIds?.Any() == true)
            {
                equipmentResourceIds = await _resourceAssignmentService.AssignEquipmentResourcesAsync(context, appointment.ApptId, dto.EquipmentResourceIds, now);
            }

            // リソース割り当てを先に保存（LoadAppointmentsAsyncで読み込めるようにするため）
            await context.SaveChangesAsync();

            // Stats再計算
            // Mainリソースは常に含まれるため、mainResourceIdsが空でない限りStats/StatSlotsを更新する
            var affectedResourceIds = mainResourceIds.Union(equipmentResourceIds).ToList();
            if (affectedResourceIds.Any())
            {
                await _appointmentStatsUpdateService.RecalculateStatsAsync(
                    context,
                    new List<DateOnly> { dto.Date },
                    affectedResourceIds);
            }

            // Stats/StatSlotsの保存
            await context.SaveChangesAsync();

            var result = await GetAppointmentAsync(appointment.ApptId);
            if (!result.IsSuccess) throw new Exception(result.ErrorMessage);
            return result.Value!;
        },
        (ex, t, f, u) => LogMessages.AppointmentCreateFailed(_logger, dto.PatientId, dto.Date, t, f, u, ex),
        "Database error while creating appointment",
        "APPT_CREATE_ERROR");
    }

    /// <inheritdoc />
    public async Task<Result<AppointmentDto>> UpdateAppointmentAsync(Guid apptId, UpdateAppointmentDto dto)
    {
        try
        {
            var appointment = await _appointmentRepository.FindForUpdateAsync(apptId);
            if (appointment == null || appointment.IsDeleted)
            {
                var (t, f, u) = _userContextService.GetTenantContext();
                LogMessages.AppointmentNotFound(_logger, apptId, t, f, u);
                return Result<AppointmentDto>.Failure($"Appointment {apptId} not found", "APPT_NOT_FOUND");
            }

            if (dto.ExpectedUpdatedAt.HasValue)
            {
                var expected = _dateTimeProvider.RoundToSeconds(dto.ExpectedUpdatedAt.Value);
                var actual = _dateTimeProvider.RoundToSeconds(appointment.UpdatedAt);
                if (actual != expected)
                {
                    _logger.LogWarning("Concurrency conflict for {ApptId}. Expected: {Expected}, Actual: {Actual}", apptId, expected, actual);
                    return Result<AppointmentDto>.Failure("This appointment was modified by another user.", "APPT_CONCURRENCY_ERROR");
                }
            }

            // 変更前の日付を保存（Stats再計算の為）
            var oldDate = appointment.ApptDate;

            if (dto.Date.HasValue) appointment.ApptDate = dto.Date.Value;
            if (dto.StartMin.HasValue) appointment.ApptStartMin = dto.StartMin.Value;
            if (dto.PatientId.HasValue) appointment.PtId = dto.PatientId.Value;
            if (dto.OrganizationId.HasValue) appointment.OrgId = dto.OrganizationId.Value;
            if (dto.FloorId.HasValue) appointment.FloorId = dto.FloorId.Value;
            if (dto.Status.HasValue) appointment.ApptStatusCode = dto.Status.Value;
            if (dto.Memo != null) appointment.ApptMemo = dto.Memo;

            appointment.UpdatedAt = _dateTimeProvider.NowRoundedToSeconds;
            await _appointmentRepository.UpdateAsync(appointment);

            await using var context = await _dbContextFactory.CreateDbContextAsync();

            // Stats再計算の為、リソース削除前に古いリソースIDを取得
            var oldResourceIds = await GetResourceIdsForAppointmentAsync(context, apptId);

            await _resourceAssignmentService.SoftDeleteAllAssignmentsAsync(context, apptId, appointment.UpdatedAt);
            var newMainResourceIds = await _resourceAssignmentService.AssignMainResourcesAsync(context, apptId, appointment.FloorId, appointment.UpdatedAt);
            var newEquipmentResourceIds = new List<Guid>();
            if (dto.EquipmentResourceIds?.Any() == true)
            {
                newEquipmentResourceIds = await _resourceAssignmentService.AssignEquipmentResourcesAsync(context, apptId, dto.EquipmentResourceIds, appointment.UpdatedAt);
            }

            // Stats再計算の為、新しいリソースIDを取得
            var newResourceIds = newMainResourceIds.Union(newEquipmentResourceIds).ToList();
            var affectedResourceIds = oldResourceIds.Union(newResourceIds).Distinct().ToList();

            // 影響を受ける日付を決定
            var affectedDates = new List<DateOnly>();
            if (appointment.ApptDate.HasValue)
            {
                affectedDates.Add(appointment.ApptDate.Value);
            }
            if (oldDate.HasValue && oldDate != appointment.ApptDate)
            {
                affectedDates.Add(oldDate.Value);
            }

            // Stats再計算
            if (affectedResourceIds.Any())
            {
                await _appointmentStatsUpdateService.RecalculateStatsAsync(
                    context,
                    affectedDates.Distinct().ToList(),
                    affectedResourceIds);
            }

            await context.SaveChangesAsync();

            return await GetAppointmentAsync(apptId);
        }
        catch (ConcurrencyException ex)
        {
            var (t, f, u) = _userContextService.GetTenantContext();
            LogMessages.AppointmentConcurrencyError(_logger, apptId, t, f, u, ex);
            return Result<AppointmentDto>.Failure("This appointment was modified by another user.", "APPT_CONCURRENCY_ERROR");
        }
        catch (Exception ex)
        {
            var (t, f, u) = _userContextService.GetTenantContext();
            LogMessages.AppointmentUpdateError(_logger, apptId, t, f, u, ex);
            return Result<AppointmentDto>.Failure(ex is DatabaseException ? "Database error while updating appointment" : "An unexpected error occurred", "APPT_UPDATE_ERROR");
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAppointmentAsync(Guid apptId)
    {
        return await ExecuteAsync(async () =>
        {
            // 削除前にAppointmentを読み込み（リソースIDを含む）
            var appointment = await _appointmentRepository.FindForUpdateAsync(apptId);
            if (appointment == null || appointment.IsDeleted)
            {
                var (t, f, u) = _userContextService.GetTenantContext();
                LogMessages.AppointmentNotFound(_logger, apptId, t, f, u);
                throw new NotFoundException("Appointment", apptId);
            }

            // Stats再計算の為、リソースIDを取得
            var affectedDate = appointment.ApptDate;
            var affectedResourceIds = appointment.AppointmentResourceAssignments
                .Where(a => !a.IsDeleted)
                .Select(a => a.ApptResId)
                .ToList();

            // 予約を削除
            await _appointmentRepository.DeleteAsync(apptId);

            // Stats再計算
            if (affectedDate.HasValue && affectedResourceIds.Any())
            {
                await using var context = await _dbContextFactory.CreateDbContextAsync();

                await _appointmentStatsUpdateService.RecalculateStatsAsync(
                    context,
                    new List<DateOnly> { affectedDate.Value },
                    affectedResourceIds);

                await context.SaveChangesAsync();
            }
        },
        (ex, t, f, u) => LogMessages.AppointmentDeleteError(_logger, apptId, t, f, u, ex),
        "Database error while deleting appointment",
        "APPT_DELETE_ERROR",
        onNotFound: (ex) => Result.Failure($"Appointment {apptId} not found", "APPT_NOT_FOUND"));
    }

    /// <inheritdoc />
    public async Task<Result<List<HolidayDto>>> GetHolidaysAsync(DateOnly startDate, DateOnly endDate)
    {
        return await ExecuteAsync(async () =>
        {
            var holidays = await _holidayRepository.GetByDateRangeAsync(startDate, endDate);
            return holidays.Select(h => new HolidayDto { Date = h.HolidayDate, Name = h.HolidayName }).ToList();
        },
        (ex, t, f, u) => LogMessages.AppointmentsRetrievalError(_logger, startDate, endDate, t, f, u, ex),
        "Failed to retrieve holidays",
        "HOLIDAY_RETRIEVAL_ERROR");
    }

    #region Helpers

    /// <summary>
    /// 予約のリソースIDリストを取得
    /// </summary>
    private async Task<List<Guid>> GetResourceIdsForAppointmentAsync(MedockDbContext context, Guid apptId)
    {
        return await context.AppointmentResourceAssignments
            .Where(a => a.ApptId == apptId && !a.IsDeleted)
            .Select(a => a.ApptResId)
            .ToListAsync();
    }

    private async Task<Result<T>> ExecuteAsync<T>(
        Func<Task<T>> operation,
        Action<Exception, Guid?, Guid?, Guid?> logAction,
        string errorMessage,
        string errorCode,
        Func<Exception, Result<T>>? onNotFound = null)
    {
        try
        {
            return Result<T>.Success(await operation());
        }
        catch (NotFoundException ex) when (onNotFound != null)
        {
            return onNotFound(ex);
        }
        catch (Exception ex)
        {
            var (t, f, u) = _userContextService.GetTenantContext();
            logAction(ex, t, f, u);
            return Result<T>.Failure(ex is DatabaseException ? errorMessage : "An unexpected error occurred", errorCode);
        }
    }

    private async Task<Result> ExecuteAsync(
        Func<Task> operation,
        Action<Exception, Guid?, Guid?, Guid?> logAction,
        string errorMessage,
        string errorCode,
        Func<Exception, Result>? onNotFound = null)
    {
        try
        {
            await operation();
            return Result.Success();
        }
        catch (NotFoundException ex) when (onNotFound != null)
        {
            return onNotFound(ex);
        }
        catch (Exception ex)
        {
            var (t, f, u) = _userContextService.GetTenantContext();
            logAction(ex, t, f, u);
            return Result.Failure(ex is DatabaseException ? errorMessage : "An unexpected error occurred", errorCode);
        }
    }

    #endregion
}

