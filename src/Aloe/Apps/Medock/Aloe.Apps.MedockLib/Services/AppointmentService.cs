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

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IHolidayRepository holidayRepository,
        IUserContextService userContextService,
        IDateTimeProvider dateTimeProvider,
        IDbContextFactory<MedockDbContext> dbContextFactory,
        ILogger<AppointmentService> logger)
    {
        this._appointmentRepository = appointmentRepository;
        this._holidayRepository = holidayRepository;
        this._userContextService = userContextService;
        this._dateTimeProvider = dateTimeProvider;
        this._dbContextFactory = dbContextFactory;
        this._logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<AppointmentDto>>> GetAppointmentsAsync(DateOnly startDate, DateOnly endDate)
    {
        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var querySw = System.Diagnostics.Stopwatch.StartNew();
            var appointments = await this._appointmentRepository.GetByDateRangeAsync(startDate, endDate);
            querySw.Stop();
            this._logger.LogInformation("[PERF] AppointmentService - Repository GetByDateRangeAsync: {ElapsedMs}ms, Count={Count}",
                querySw.ElapsedMilliseconds, appointments.Count);

            var mapSw = System.Diagnostics.Stopwatch.StartNew();
            var dtos = appointments.Select(a => this.MapToDto(a)).ToList();
            mapSw.Stop();
            this._logger.LogInformation("[PERF] AppointmentService - MapToDto: {ElapsedMs}ms",
                mapSw.ElapsedMilliseconds);

            sw.Stop();
            this._logger.LogInformation("[PERF] AppointmentService - GetAppointmentsAsync total: {ElapsedMs}ms",
                sw.ElapsedMilliseconds);

            return Result<List<AppointmentDto>>.Success(dtos);
        }
        catch (DatabaseException ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentsRetrievalError(this._logger, startDate, endDate, tenantId, facilityId, userId, ex);
            return Result<List<AppointmentDto>>.Failure(
                $"Failed to retrieve appointments for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                "APPT_RETRIEVAL_ERROR");
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentsRetrievalError(this._logger, startDate, endDate, tenantId, facilityId, userId, ex);
            return Result<List<AppointmentDto>>.Failure(
                "An unexpected error occurred while retrieving appointments",
                "APPT_RETRIEVAL_ERROR");
        }
    }

    /// <inheritdoc />
    public async Task<Result<AppointmentDto>> GetAppointmentAsync(Guid apptId)
    {
        try
        {
            var appointment = await this._appointmentRepository.GetByIdAsync(apptId);
            if (appointment is null)
            {
                var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
                LogMessages.AppointmentNotFound(this._logger, apptId, tenantId, facilityId, userId);
                return Result<AppointmentDto>.Failure($"Appointment {apptId} not found", "APPT_NOT_FOUND");
            }

            var dto = this.MapToDto(appointment);

            // 機器リソースを取得
            try
            {
                await using var context = await this._dbContextFactory.CreateDbContextAsync();

                var equipmentResources = await context.AppointmentResourceAssignments
                    .AsNoTracking()
                    .Where(a => a.ApptId == apptId && !a.IsDeleted)
                    .Include(a => a.AppointmentResource)
                    .Where(a => a.AppointmentResource.ApptResTypeCode == (int)AppointmentResourceType.Equipment)
                    .Select(a => new EquipmentResourceDto
                    {
                        Id = a.ApptResId,
                        Name = a.AppointmentResource.ApptResName
                    })
                    .ToListAsync();

                dto.EquipmentResources = equipmentResources;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error loading equipment resources for appointment {ApptId}", apptId);
                // エラーが発生しても DTO は返す（リソース情報なし）
            }

            return Result<AppointmentDto>.Success(dto);
        }
        catch (DatabaseException ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentRetrievalError(this._logger, apptId, tenantId, facilityId, userId, ex.InnerException ?? ex);
            return Result<AppointmentDto>.Failure($"Failed to retrieve appointment {apptId}", "APPT_RETRIEVAL_ERROR");
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentRetrievalError(this._logger, apptId, tenantId, facilityId, userId, ex);
            return Result<AppointmentDto>.Failure("An unexpected error occurred", "APPT_RETRIEVAL_ERROR");
        }
    }

    /// <inheritdoc />
    public async Task<Result<AppointmentDto>> CreateAppointmentAsync(CreateAppointmentDto dto)
    {
        try
        {
            var now = this._dateTimeProvider.NowRoundedToSeconds;
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

            await this._appointmentRepository.AddAsync(appointment);

            // リソース割り当てを作成
            await using var context = await this._dbContextFactory.CreateDbContextAsync();

            // Mainリソースを自動的に割り当て
            await this.AssignMainResourcesAsync(context, appointment.ApptId, dto.FloorId, now);

            // 選択された機器リソースを割り当て
            if (dto.EquipmentResourceIds?.Any() == true)
            {
                await this.AssignEquipmentResourcesAsync(context, appointment.ApptId, dto.EquipmentResourceIds, now);
            }

            await context.SaveChangesAsync();

            // 関連データを読み込んで返す
            var result = await this.GetAppointmentAsync(appointment.ApptId);
            if (!result.IsSuccess)
            {
                var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
                LogMessages.AppointmentCreateFailed(this._logger, dto.PatientId, dto.Date, tenantId, facilityId, userId, new Exception(result.ErrorMessage));
                return Result<AppointmentDto>.Failure("Failed to create appointment", "APPT_CREATE_ERROR");
            }
            return result;
        }
        catch (DatabaseException ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentCreateFailed(this._logger, dto.PatientId, dto.Date, tenantId, facilityId, userId, ex);
            return Result<AppointmentDto>.Failure($"Database error while creating appointment", "APPT_CREATE_ERROR");
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentCreateFailed(this._logger, dto.PatientId, dto.Date, tenantId, facilityId, userId, ex);
            return Result<AppointmentDto>.Failure("An unexpected error occurred while creating appointment", "APPT_CREATE_ERROR");
        }
    }

    /// <inheritdoc />
    public async Task<Result<AppointmentDto>> UpdateAppointmentAsync(Guid apptId, UpdateAppointmentDto dto)
    {
        try
        {
            var appointment = await this._appointmentRepository.FindForUpdateAsync(apptId);
            if (appointment == null || appointment.IsDeleted)
            {
                var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
                LogMessages.AppointmentNotFound(this._logger, apptId, tenantId, facilityId, userId);
                return Result<AppointmentDto>.Failure($"Appointment {apptId} not found", "APPT_NOT_FOUND");
            }

            // 楽観的ロック：他のユーザーによる更新を検出
            if (dto.ExpectedUpdatedAt.HasValue)
            {
                // 秒単位で比較（マイクロ秒の差を無視）
                var expectedSeconds = this._dateTimeProvider.RoundToSeconds(dto.ExpectedUpdatedAt.Value);
                var actualSeconds = this._dateTimeProvider.RoundToSeconds(appointment.UpdatedAt);

                if (actualSeconds != expectedSeconds)
                {
                    var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
                    this._logger.LogWarning(
                        "Concurrency conflict detected for appointment {ApptId}. Expected UpdatedAt: {Expected}, Actual: {Actual}",
                        apptId, expectedSeconds, actualSeconds);
                    return Result<AppointmentDto>.Failure(
                        "This appointment was modified by another user. Please refresh and try again.",
                        "APPT_CONCURRENCY_ERROR");
                }
            }

            if (dto.Date.HasValue) appointment.ApptDate = dto.Date.Value;
            if (dto.StartMin.HasValue)
                appointment.ApptStartMin = dto.StartMin.Value;
            if (dto.PatientId.HasValue) appointment.PtId = dto.PatientId.Value;
            if (dto.OrganizationId.HasValue) appointment.OrgId = dto.OrganizationId.Value;
            if (dto.FloorId.HasValue) appointment.FloorId = dto.FloorId.Value;
            if (dto.Status.HasValue) appointment.ApptStatusCode = dto.Status.Value;
            if (dto.Memo != null) appointment.ApptMemo = dto.Memo;

            // 秒単位で丸める（マイクロ秒の差を排除）
            appointment.UpdatedAt = this._dateTimeProvider.NowRoundedToSeconds;

            await this._appointmentRepository.UpdateAsync(appointment);

            // リソース割り当てを同期
            await using var context = await this._dbContextFactory.CreateDbContextAsync();

            // 既存の割り当てをすべてソフト削除
            var existingAssignments = await context.AppointmentResourceAssignments
                .Where(a => a.ApptId == apptId && !a.IsDeleted)
                .ToListAsync();

            foreach (var assignment in existingAssignments)
            {
                assignment.IsDeleted = true;
                assignment.UpdatedAt = appointment.UpdatedAt;
            }

            // Mainリソースを自動的に割り当て
            await this.AssignMainResourcesAsync(context, apptId, appointment.FloorId, appointment.UpdatedAt);

            // 選択された機器リソースを割り当て
            if (dto.EquipmentResourceIds?.Any() == true)
            {
                await this.AssignEquipmentResourcesAsync(context, apptId, dto.EquipmentResourceIds, appointment.UpdatedAt);
            }

            await context.SaveChangesAsync();

            return await this.GetAppointmentAsync(apptId);
        }
        catch (ConcurrencyException ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentConcurrencyError(this._logger, apptId, tenantId, facilityId, userId, ex);
            return Result<AppointmentDto>.Failure(
                $"Appointment {apptId} was modified by another user. Please refresh and try again.",
                "APPT_CONCURRENCY_ERROR");
        }
        catch (DatabaseException ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentUpdateError(this._logger, apptId, tenantId, facilityId, userId, ex);
            return Result<AppointmentDto>.Failure("Database error while updating appointment", "APPT_UPDATE_ERROR");
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentUpdateError(this._logger, apptId, tenantId, facilityId, userId, ex);
            return Result<AppointmentDto>.Failure("An unexpected error occurred while updating appointment", "APPT_UPDATE_ERROR");
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAppointmentAsync(Guid apptId)
    {
        try
        {
            var appointment = await this._appointmentRepository.FindForUpdateAsync(apptId);
            if (appointment == null || appointment.IsDeleted)
            {
                var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
                LogMessages.AppointmentNotFound(this._logger, apptId, tenantId, facilityId, userId);
                return Result.Failure($"Appointment {apptId} not found", "APPT_NOT_FOUND");
            }

            await this._appointmentRepository.DeleteAsync(apptId);
            return Result.Success();
        }
        catch (NotFoundException ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentNotFound(this._logger, apptId, tenantId, facilityId, userId);
            return Result.Failure(ex.Message, "APPT_NOT_FOUND");
        }
        catch (DatabaseException ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentDeleteError(this._logger, apptId, tenantId, facilityId, userId, ex);
            return Result.Failure("Database error while deleting appointment", "APPT_DELETE_ERROR");
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentDeleteError(this._logger, apptId, tenantId, facilityId, userId, ex);
            return Result.Failure("An unexpected error occurred while deleting appointment", "APPT_DELETE_ERROR");
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<HolidayDto>>> GetHolidaysAsync(DateOnly startDate, DateOnly endDate)
    {
        try
        {
            var holidays = await this._holidayRepository.GetByDateRangeAsync(startDate, endDate);
            var dtos = holidays.Select(h => new HolidayDto
            {
                Date = h.HolidayDate,
                Name = h.HolidayName
            }).ToList();
            return Result<List<HolidayDto>>.Success(dtos);
        }
        catch (DatabaseException ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentsRetrievalError(this._logger, startDate, endDate, tenantId, facilityId, userId, ex);
            return Result<List<HolidayDto>>.Failure("Failed to retrieve holidays", "HOLIDAY_RETRIEVAL_ERROR");
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
            LogMessages.AppointmentsRetrievalError(this._logger, startDate, endDate, tenantId, facilityId, userId, ex);
            return Result<List<HolidayDto>>.Failure("An unexpected error occurred while retrieving holidays", "HOLIDAY_RETRIEVAL_ERROR");
        }
    }

    /// <summary>
    /// Mainリソースを予約に自動的に割り当てます
    /// </summary>
    private async Task AssignMainResourcesAsync(MedockDbContext context, Guid apptId, Guid floorId, DateTime timestamp)
    {
        var mainResources = await context.AppointmentResources
            .AsNoTracking()
            .Where(r => r.FloorId == floorId &&
                       !r.IsDeleted &&
                       r.ApptResTypeCode == (int)AppointmentResourceType.Main)
            .OrderBy(r => r.ApptResSeq)
            .ToListAsync();

        foreach (var mainResource in mainResources)
        {
            var assignment = new AppointmentResourceAssignment
            {
                ApptResAssignId = Guid.CreateVersion7(),
                ApptId = apptId,
                ApptResId = mainResource.ApptResId,
                IsDeleted = false,
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            };

            await context.AppointmentResourceAssignments.AddAsync(assignment);
        }

        this._logger.LogDebug("Created {Count} main resource assignments for appointment {ApptId}",
            mainResources.Count, apptId);
    }

    /// <summary>
    /// 機器リソースを予約に割り当てます
    /// </summary>
    private async Task AssignEquipmentResourcesAsync(MedockDbContext context, Guid apptId, IEnumerable<Guid> equipmentResourceIds, DateTime timestamp)
    {
        foreach (var resourceId in equipmentResourceIds)
        {
            var assignment = new AppointmentResourceAssignment
            {
                ApptResAssignId = Guid.CreateVersion7(),
                ApptId = apptId,
                ApptResId = resourceId,
                IsDeleted = false,
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            };

            await context.AppointmentResourceAssignments.AddAsync(assignment);
        }

        this._logger.LogDebug("Created {Count} equipment resource assignments for appointment {ApptId}",
            equipmentResourceIds.Count(), apptId);
    }

    private AppointmentDto MapToDto(Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.ApptId,
            Date = appointment.ApptDate ?? DateOnly.FromDateTime(this._dateTimeProvider.Today),
            StartMin = appointment.ApptStartMin,
            PatientId = appointment.PtId,
            PatientName = appointment.Patient?.PtName,
            OrganizationId = appointment.OrgId,
            OrganizationName = appointment.Organization?.OrgName,
            FloorId = appointment.FloorId,
            FloorName = appointment.Floor?.FloorName,
            Status = appointment.ApptStatusCode,
            Memo = appointment.ApptMemo,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt
        };
    }

}

