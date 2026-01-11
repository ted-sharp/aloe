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
    private readonly AppointmentResourceAssignmentService _resourceAssignmentService;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IHolidayRepository holidayRepository,
        IUserContextService userContextService,
        IDateTimeProvider dateTimeProvider,
        IDbContextFactory<MedockDbContext> dbContextFactory,
        ILogger<AppointmentService> logger,
        AppointmentResourceAssignmentService resourceAssignmentService)
    {
        this._appointmentRepository = appointmentRepository;
        this._holidayRepository = holidayRepository;
        this._userContextService = userContextService;
        this._dateTimeProvider = dateTimeProvider;
        this._dbContextFactory = dbContextFactory;
        this._logger = logger;
        this._resourceAssignmentService = resourceAssignmentService;
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
            var dtos = appointments.Select(a => AppointmentMapper.MapToDto(a, this._dateTimeProvider)).ToList();
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

            var dto = AppointmentMapper.MapToDto(appointment, this._dateTimeProvider);

            // 機器リソースを取得
            // 注意: 機器リソースの取得に失敗した場合でも、予約DTOは返す（部分的な結果）
            // EquipmentResourcesは空のリストで初期化されているため、例外が発生しても空のリストのままとなる
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
                // 機器リソースの取得に失敗した場合、ログを出力して処理を継続
                // 予約DTOは返すが、EquipmentResourcesは空のリストのままとなる（部分的な結果）
                var (tenantId, facilityId, userId) = this._userContextService.GetTenantContext();
                this._logger.LogWarning(ex, 
                    "Failed to load equipment resources for appointment {ApptId}. Returning appointment without equipment resources. | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}",
                    apptId, tenantId, facilityId, userId);
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
            await this._resourceAssignmentService.AssignMainResourcesAsync(context, appointment.ApptId, dto.FloorId, now);

            // 選択された機器リソースを割り当て
            if (dto.EquipmentResourceIds?.Any() == true)
            {
                await this._resourceAssignmentService.AssignEquipmentResourcesAsync(context, appointment.ApptId, dto.EquipmentResourceIds, now);
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
            await this._resourceAssignmentService.SoftDeleteAllAssignmentsAsync(context, apptId, appointment.UpdatedAt);

            // Mainリソースを自動的に割り当て
            await this._resourceAssignmentService.AssignMainResourcesAsync(context, apptId, appointment.FloorId, appointment.UpdatedAt);

            // 選択された機器リソースを割り当て
            if (dto.EquipmentResourceIds?.Any() == true)
            {
                await this._resourceAssignmentService.AssignEquipmentResourcesAsync(context, apptId, dto.EquipmentResourceIds, appointment.UpdatedAt);
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


}

