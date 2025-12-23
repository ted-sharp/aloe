using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 施設サービス実装
/// </summary>
public class FacilityService : IFacilityService
{
    private readonly IDbContextFactory<MedockDbContext> _contextFactory;

    public FacilityService(IDbContextFactory<MedockDbContext> contextFactory)
    {
        this._contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public async Task<BusinessHoursDto> GetBusinessHoursAsync(Guid facilityId, DateOnly? targetDate = null)
    {
        using var context = this._contextFactory.CreateDbContext();

        var date = targetDate ?? DateOnly.FromDateTime(DateTime.Today);

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

        if (businessHours == null || businessHours.BusinessHoursData == null)
        {
            // デフォルト値を返す
            return GetDefaultBusinessHoursDto();
        }

        var businessHoursData = businessHours.BusinessHoursData;
        return new BusinessHoursDto
        {
            StartTime = businessHoursData.Start ?? BusinessHoursConstants.DefaultStartTime,
            EndTime = businessHoursData.End ?? BusinessHoursConstants.DefaultEndTime,
            LunchStartTime = businessHoursData.Lunch?.Start ?? BusinessHoursConstants.DefaultLunchStartTime,
            LunchEndTime = businessHoursData.Lunch?.End ?? BusinessHoursConstants.DefaultLunchEndTime
        };
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

