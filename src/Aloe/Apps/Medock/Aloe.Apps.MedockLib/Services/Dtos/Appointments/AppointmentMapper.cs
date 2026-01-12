using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;

namespace Aloe.Apps.MedockLib.Services.Dtos.Appointments;

/// <summary>
/// 予約エンティティとDTOのマッピングを行うクラス
/// </summary>
public static class AppointmentMapper
{
    /// <summary>
    /// AppointmentエンティティをAppointmentDtoにマッピングします
    /// </summary>
    /// <param name="appointment">予約エンティティ</param>
    /// <param name="dateTimeProvider">日時プロバイダー（今日の日付取得用）</param>
    /// <returns>予約DTO</returns>
    public static AppointmentDto MapToDto(Appointment appointment, IDateTimeProvider dateTimeProvider)
    {
        return new AppointmentDto
        {
            Id = appointment.ApptId,
            Date = appointment.ApptDate ?? DateOnly.FromDateTime(dateTimeProvider.Today),
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
            UpdatedAt = appointment.UpdatedAt,
            EquipmentResources = appointment.AppointmentResourceAssignments?
                .Where(a => !a.IsDeleted && a.AppointmentResource?.ApptResTypeCode == (int)AppointmentResourceType.Equipment)
                .Select(a => new EquipmentResourceDto
                {
                    Id = a.ApptResId,
                    Name = a.AppointmentResource?.ApptResName ?? string.Empty
                }).ToList() ?? new List<EquipmentResourceDto>()
        };
    }
}
