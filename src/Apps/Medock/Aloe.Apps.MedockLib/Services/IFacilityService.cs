using Aloe.Apps.MedockLib.Common;
using Aloe.Apps.MedockLib.Services.Dtos;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 施設サービスインターフェース
/// </summary>
public interface IFacilityService
{
    /// <summary>
    /// 施設の営業時間を取得します
    /// </summary>
    /// <param name="facilityId">施設ID</param>
    /// <param name="targetDate">対象日付（nullの場合は今日）</param>
    /// <returns>操作結果（成功時は営業時間DTO）</returns>
    Task<Result<BusinessHoursDto>> GetBusinessHoursAsync(Guid facilityId, DateOnly? targetDate = null);
}

