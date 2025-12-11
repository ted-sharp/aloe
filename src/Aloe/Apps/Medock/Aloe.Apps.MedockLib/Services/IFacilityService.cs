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
    /// <returns>営業時間DTO。取得できない場合はデフォルト値（9:00-18:00、昼休み12:00-13:00）を返します</returns>
    Task<BusinessHoursDto> GetBusinessHoursAsync(Guid facilityId, DateOnly? targetDate = null);
}

