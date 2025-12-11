using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Repositories;

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

    // AM/PM の時間境界
    private const int AmStartHour = 8;
    private const int AmEndHour = 12;
    private const int PmStartHour = 13;
    private const int PmEndHour = 18;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IHolidayRepository holidayRepository,
        IUserContextService userContextService,
        IDateTimeProvider dateTimeProvider)
    {
        this._appointmentRepository = appointmentRepository;
        this._holidayRepository = holidayRepository;
        this._userContextService = userContextService;
        this._dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, DayStatsDto>> GetDayStatsAsync(DateOnly startDate, DateOnly endDate)
    {
        var appointments = await this._appointmentRepository.GetForDayStatsAsync(startDate, endDate);

        var result = new Dictionary<string, DayStatsDto>([]);

        // 指定期間の全日付を初期化
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            result[dateStr] = new DayStatsDto
            {
                AmCount = 0,
                PmCount = 0,
                AmMax = 10, // TODO: フロア/施設の設定から取得
                PmMax = 10
            };
        }

        // 予約を集計
        foreach (var (apptDate, apptStartAt) in appointments)
        {
            if (!apptDate.HasValue) continue;

            var dateStr = apptDate.Value.ToString("yyyy-MM-dd");
            if (!result.TryGetValue(dateStr, out var stats)) continue;

            // 時間から AM/PM を判定
            var hour = apptStartAt?.Hour ?? AmStartHour;
            if (hour >= AmStartHour && hour < AmEndHour)
            {
                stats.AmCount++;
            }
            else if (hour >= PmStartHour && hour < PmEndHour)
            {
                stats.PmCount++;
            }
            else if (hour < PmStartHour)
            {
                // 12-13時は昼休み、AMにカウント
                stats.AmCount++;
            }
            else
            {
                // 18時以降はPMにカウント
                stats.PmCount++;
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<List<AppointmentDto>> GetAppointmentsAsync(DateOnly startDate, DateOnly endDate)
    {
        var appointments = await this._appointmentRepository.GetByDateRangeAsync(startDate, endDate);
        return appointments.Select(a => this.MapToDto(a)).ToList();
    }

    /// <inheritdoc />
    public async Task<AppointmentDto?> GetAppointmentAsync(Guid apptId)
    {
        var appointment = await this._appointmentRepository.GetByIdAsync(apptId);
        return appointment is not null ? this.MapToDto(appointment) : null;
    }

    /// <inheritdoc />
    public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto dto)
    {
        // 監査情報を設定
        var userId = this._userContextService.CurrentUser?.UserId ?? Guid.Empty;
        var sessionId = this._userContextService.CurrentSessionId ?? Guid.Empty;
        this._appointmentRepository.SetAuditInfo(userId, sessionId);

        var appointment = new Appointment
        {
            ApptId = Guid.NewGuid(),
            ApptDate = dto.Date,
            ApptStartAt = dto.StartTime.HasValue
                ? dto.Date.ToDateTime(dto.StartTime.Value)
                : null,
            ApptEndAt = dto.EndTime.HasValue
                ? dto.Date.ToDateTime(dto.EndTime.Value)
                : null,
            PtId = dto.PatientId,
            OrgId = dto.OrganizationId,
            FloorId = dto.FloorId,
            ApptStatusCode = dto.Status,
            IsDeleted = false,
            CreatedAt = this._dateTimeProvider.Now,
            UpdatedAt = this._dateTimeProvider.Now
        };

        await this._appointmentRepository.AddAsync(appointment);

        // 関連データを読み込んで返す
        return await this.GetAppointmentAsync(appointment.ApptId)
               ?? throw new InvalidOperationException("Failed to create appointment");
    }

    /// <inheritdoc />
    public async Task<AppointmentDto?> UpdateAppointmentAsync(Guid apptId, UpdateAppointmentDto dto)
    {
        // 監査情報を設定
        var userId = this._userContextService.CurrentUser?.UserId ?? Guid.Empty;
        var sessionId = this._userContextService.CurrentSessionId ?? Guid.Empty;
        this._appointmentRepository.SetAuditInfo(userId, sessionId);

        var appointment = await this._appointmentRepository.FindByIdAsync(apptId);
        if (appointment == null || appointment.IsDeleted)
        {
            return null;
        }

        if (dto.Date.HasValue)
        {
            appointment.ApptDate = dto.Date.Value;
        }

        if (dto.StartTime.HasValue && appointment.ApptDate.HasValue)
        {
            appointment.ApptStartAt = appointment.ApptDate.Value.ToDateTime(dto.StartTime.Value);
        }

        if (dto.EndTime.HasValue && appointment.ApptDate.HasValue)
        {
            appointment.ApptEndAt = appointment.ApptDate.Value.ToDateTime(dto.EndTime.Value);
        }

        if (dto.PatientId.HasValue)
        {
            appointment.PtId = dto.PatientId.Value;
        }

        if (dto.OrganizationId.HasValue)
        {
            appointment.OrgId = dto.OrganizationId.Value;
        }

        if (dto.FloorId.HasValue)
        {
            appointment.FloorId = dto.FloorId.Value;
        }

        if (dto.Status.HasValue)
        {
            appointment.ApptStatusCode = dto.Status.Value;
        }

        appointment.UpdatedAt = this._dateTimeProvider.Now;

        await this._appointmentRepository.UpdateAsync(appointment);

        return await this.GetAppointmentAsync(apptId);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAppointmentAsync(Guid apptId)
    {
        // 監査情報を設定
        var userId = this._userContextService.CurrentUser?.UserId ?? Guid.Empty;
        var sessionId = this._userContextService.CurrentSessionId ?? Guid.Empty;
        this._appointmentRepository.SetAuditInfo(userId, sessionId);

        var appointment = await this._appointmentRepository.FindByIdAsync(apptId);
        if (appointment == null || appointment.IsDeleted)
        {
            return false;
        }

        await this._appointmentRepository.DeleteAsync(apptId);
        return true;
    }

    /// <inheritdoc />
    public async Task<List<HolidayDto>> GetHolidaysAsync(DateOnly startDate, DateOnly endDate)
    {
        var holidays = await this._holidayRepository.GetByDateRangeAsync(startDate, endDate);
        return holidays.Select(h => new HolidayDto
        {
            Date = h.HolidayDate,
            Name = h.HolidayName
        }).ToList();
    }

    private AppointmentDto MapToDto(Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.ApptId,
            Date = appointment.ApptDate ?? DateOnly.FromDateTime(this._dateTimeProvider.Today),
            StartTime = appointment.ApptStartAt.HasValue
                ? TimeOnly.FromDateTime(appointment.ApptStartAt.Value)
                : null,
            EndTime = appointment.ApptEndAt.HasValue
                ? TimeOnly.FromDateTime(appointment.ApptEndAt.Value)
                : null,
            PatientId = appointment.PtId,
            PatientName = appointment.Patient?.PtName,
            OrganizationId = appointment.OrgId,
            OrganizationName = appointment.Organization?.OrgName,
            FloorId = appointment.FloorId,
            FloorName = appointment.Floor?.FloorName,
            Status = appointment.ApptStatusCode,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt
        };
    }
}

