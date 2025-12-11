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
            return new BusinessHoursDto
            {
                StartTime = "09:00",
                EndTime = "18:00",
                LunchStartTime = "12:00",
                LunchEndTime = "13:00"
            };
        }

        // JSONBをパース
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var jsonDoc = JsonDocument.Parse(businessHours.BusinessHours);
            var root = jsonDoc.RootElement;

            var dto = new BusinessHoursDto();

            // start, end を取得
            if (root.TryGetProperty("start", out var startElement))
            {
                dto.StartTime = startElement.GetString() ?? "09:00";
            }

            if (root.TryGetProperty("end", out var endElement))
            {
                dto.EndTime = endElement.GetString() ?? "18:00";
            }

            // lunch オブジェクトを取得
            if (root.TryGetProperty("lunch", out var lunchElement) && lunchElement.ValueKind == JsonValueKind.Object)
            {
                if (lunchElement.TryGetProperty("start", out var lunchStartElement))
                {
                    dto.LunchStartTime = lunchStartElement.GetString() ?? "12:00";
                }

                if (lunchElement.TryGetProperty("end", out var lunchEndElement))
                {
                    dto.LunchEndTime = lunchEndElement.GetString() ?? "13:00";
                }
            }

            return dto;
        }
        catch (JsonException)
        {
            // JSONパースエラーの場合はデフォルト値を返す
            return new BusinessHoursDto
            {
                StartTime = "09:00",
                EndTime = "18:00",
                LunchStartTime = "12:00",
                LunchEndTime = "13:00"
            };
        }
    }
}

