using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockLib.Logging;

/// <summary>
/// LoggerMessage Source Generatorを使用した最適化されたログメッセージ定義
/// </summary>
public static partial class LogMessages
{
    // ========================================
    // Repository層: 予約リポジトリ
    // ========================================

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Error,
        Message = "Error retrieving appointment {AppointmentId} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void AppointmentRetrievalError(
        ILogger logger,
        Guid appointmentId,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Error,
        Message = "Database error while creating appointment {AppointmentId} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void AppointmentCreateError(
        ILogger logger,
        Guid appointmentId,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Concurrency error while updating appointment {AppointmentId} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void AppointmentConcurrencyError(
        ILogger logger,
        Guid appointmentId,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Warning,
        Message = "Appointment not found for deletion: {AppointmentId} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void AppointmentNotFoundForDeletion(
        ILogger logger,
        Guid appointmentId,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Information,
        Message = "Appointment created successfully: {AppointmentId} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void AppointmentCreated(
        ILogger logger,
        Guid appointmentId,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Information,
        Message = "Appointment updated successfully: {AppointmentId} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void AppointmentUpdated(
        ILogger logger,
        Guid appointmentId,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId);

    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Information,
        Message = "Appointment deleted successfully: {AppointmentId} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void AppointmentDeleted(
        ILogger logger,
        Guid appointmentId,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId);

    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Error,
        Message = "Database error while updating appointment {AppointmentId} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void AppointmentUpdateError(
        ILogger logger,
        Guid appointmentId,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);

    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Error,
        Message = "Database error while deleting appointment {AppointmentId} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void AppointmentDeleteError(
        ILogger logger,
        Guid appointmentId,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);

    // ========================================
    // Service層: 予約サービス
    // ========================================

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Error,
        Message = "Error retrieving appointments for date range {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void AppointmentsRetrievalError(
        ILogger logger,
        DateOnly startDate,
        DateOnly endDate,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Warning,
        Message = "Appointment not found: {AppointmentId} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void AppointmentNotFound(
        ILogger logger,
        Guid appointmentId,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Error,
        Message = "Failed to create appointment for patient {PatientId} on date {Date:yyyy-MM-dd} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void AppointmentCreateFailed(
        ILogger logger,
        Guid? patientId,
        DateOnly date,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);

    // ========================================
    // Repository層: 祝日リポジトリ
    // ========================================

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Error,
        Message = "Error retrieving holidays for date range {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void HolidaysRetrievalError(
        ILogger logger,
        DateOnly startDate,
        DateOnly endDate,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);

    // ========================================
    // Repository層: 予約統計リポジトリ
    // ========================================

    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Error,
        Message = "Error retrieving main resource stats for date range {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void MainResourceStatsRetrievalError(
        ILogger logger,
        DateOnly startDate,
        DateOnly endDate,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);

    [LoggerMessage(
        EventId = 1012,
        Level = LogLevel.Error,
        Message = "Error retrieving main resource stats with filters for date range {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void MainResourceStatsWithFiltersRetrievalError(
        ILogger logger,
        DateOnly startDate,
        DateOnly endDate,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);

    [LoggerMessage(
        EventId = 1013,
        Level = LogLevel.Error,
        Message = "Error retrieving main resource stats for date {Date:yyyy-MM-dd} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void MainResourceStatsByDateRetrievalError(
        ILogger logger,
        DateOnly date,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);

    [LoggerMessage(
        EventId = 1014,
        Level = LogLevel.Error,
        Message = "Error retrieving equipment resource stats for date range {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void EquipmentResourceStatsRetrievalError(
        ILogger logger,
        DateOnly startDate,
        DateOnly endDate,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);

    [LoggerMessage(
        EventId = 1015,
        Level = LogLevel.Error,
        Message = "Error retrieving equipment resource slots for date range {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void EquipmentResourceSlotsRetrievalError(
        ILogger logger,
        DateOnly startDate,
        DateOnly endDate,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);

    [LoggerMessage(
        EventId = 1016,
        Level = LogLevel.Error,
        Message = "Error retrieving stat slots for date range {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void StatSlotsRetrievalError(
        ILogger logger,
        DateOnly startDate,
        DateOnly endDate,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);

    // ========================================
    // Service層: 施設サービス
    // ========================================

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Error,
        Message = "Error retrieving business hours for facility {FacilityId} on date {TargetDate:yyyy-MM-dd} | TenantId={TenantId}, UserId={UserId}")]
    public static partial void BusinessHoursRetrievalError(
        ILogger logger,
        Guid facilityId,
        DateOnly? targetDate,
        Guid? tenantId,
        Guid? userId,
        Exception ex);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Debug,
        Message = "No business hours found for facility {FacilityId} on date {Date:yyyy-MM-dd}, using defaults | TenantId={TenantId}, UserId={UserId}")]
    public static partial void BusinessHoursNotFound(
        ILogger logger,
        Guid facilityId,
        DateOnly date,
        Guid? tenantId,
        Guid? userId);

    // ========================================
    // Controller層
    // ========================================

    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Error,
        Message = "Failed to get diff data | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void DiffDataRetrievalError(
        ILogger logger,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);

    // ========================================
    // Blazor Component層
    // ========================================

    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Error,
        Message = "Error moving appointment {AppointmentId} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void AppointmentMoveError(
        ILogger logger,
        Guid appointmentId,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);

    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Error,
        Message = "Error loading appointments | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}")]
    public static partial void AppointmentLoadError(
        ILogger logger,
        Guid? tenantId,
        Guid? facilityId,
        Guid? userId,
        Exception ex);
}
