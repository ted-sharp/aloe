using System.Text.Json;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
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

        if (businessHours == null || String.IsNullOrWhiteSpace(businessHours.BusinessHours))
        {
            // デフォルト値を返す
            return GetDefaultBusinessHoursDto();
        }

        // JSONBをパース
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var businessHoursJson = JsonSerializer.Deserialize<BusinessHoursJson>(
                businessHours.BusinessHours,
                options);

            if (businessHoursJson == null)
            {
                return GetDefaultBusinessHoursDto();
            }

            return new BusinessHoursDto
            {
                StartTime = businessHoursJson.Start ?? "09:00",
                EndTime = businessHoursJson.End ?? "18:00",
                LunchStartTime = businessHoursJson.Lunch?.Start ?? "12:00",
                LunchEndTime = businessHoursJson.Lunch?.End ?? "13:00"
            };
        }
        catch (JsonException)
        {
            // JSONパースエラーの場合はデフォルト値を返す
            return GetDefaultBusinessHoursDto();
        }
    }

    /// <summary>
    /// デフォルトの営業時間DTOを取得
    /// </summary>
    private static BusinessHoursDto GetDefaultBusinessHoursDto()
    {
        return new BusinessHoursDto
        {
            StartTime = "09:00",
            EndTime = "18:00",
            LunchStartTime = "12:00",
            LunchEndTime = "13:00"
        };
    }
}

