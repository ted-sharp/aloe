using Aloe.Apps.MedockLib.Common;
using Aloe.Apps.MedockLib.Common.Exceptions;
using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Logging;
using Aloe.Apps.MedockLib.Services.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 施設サービス実装
/// </summary>
public class FacilityService : IFacilityService
{
    private readonly IDbContextFactory<MedockDbContext> _contextFactory;
    private readonly ILogger<FacilityService> _logger;
    private readonly IUserContextService _userContextService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public FacilityService(IDbContextFactory<MedockDbContext> contextFactory, ILogger<FacilityService> logger, IUserContextService userContextService, IDateTimeProvider dateTimeProvider)
    {
        this._contextFactory = contextFactory;
        this._logger = logger;
        this._userContextService = userContextService;
        this._dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc />
    public async Task<Result<BusinessHoursDto>> GetBusinessHoursAsync(Guid facilityId, DateOnly? targetDate = null)
    {
        try
        {
            using var context = this._contextFactory.CreateDbContext();

            var date = targetDate ?? this._dateTimeProvider.TodayDateOnly;

            // 該当日付で有効な営業時間レコードを取得
            // active_from <= date <= active_to かつ is_active = true かつ is_deleted = false
            var businessHours = await context.FacilityBusinessHours
                .Where(fbh => fbh.FacilityId == facilityId
                    && fbh.IsActive
                    && !fbh.IsDeleted
                    && fbh.ActiveFrom <= date
                    && fbh.ActiveTo >= date)
                .OrderByDescending(fbh => fbh.ActiveFrom) // 最新の設定を優先
                .FirstOrDefaultAsync();

            if (businessHours == null)
            {
                // No business hours found, return default values - this is a normal condition, not an error
                this._logger.LogDebug("No business hours found for facility {FacilityId} on date {Date:yyyy-MM-dd}, using defaults",
                    facilityId, date);
                return Result<BusinessHoursDto>.Success(GetDefaultBusinessHoursDto());
            }

            var dto = new BusinessHoursDto
            {
                StartTime = FacilityBusinessHours.MinutesToTimeString(businessHours.WorkStartMin),
                EndTime = FacilityBusinessHours.MinutesToTimeString(businessHours.WorkEndMin),
                LunchStartTime = FacilityBusinessHours.MinutesToTimeString(businessHours.LunchStartMin),
                LunchEndTime = FacilityBusinessHours.MinutesToTimeString(businessHours.LunchEndMin)
            };
            return Result<BusinessHoursDto>.Success(dto);
        }
        catch (Exception ex)
        {
            var (tenantId, _, userId) = this._userContextService.GetTenantContext();
            LogMessages.BusinessHoursRetrievalError(this._logger, facilityId, targetDate ?? this._dateTimeProvider.TodayDateOnly, tenantId, userId, ex);
            // Return default hours on error to prevent system failure
            return Result<BusinessHoursDto>.Success(GetDefaultBusinessHoursDto());
        }
    }

    /// <summary>
    /// デフォルトの営業時間DTOを取得
    /// </summary>
    private static BusinessHoursDto GetDefaultBusinessHoursDto()
    {
        return new BusinessHoursDto
        {
            StartTime = BusinessHoursConstants.DefaultStartTime,
            EndTime = BusinessHoursConstants.DefaultEndTime,
            LunchStartTime = BusinessHoursConstants.DefaultLunchStartTime,
            LunchEndTime = BusinessHoursConstants.DefaultLunchEndTime
        };
    }
}

