using Aloe.Apps.MedockLib.Data.Entities;

namespace Aloe.Apps.MedockLib.Services.Auth;

/// <summary>
/// 認証・認可に関するヘルパークラス
/// </summary>
/// <remarks>
/// FUTURE FEATURE: Role-Based Access Control (RBAC) 実装用に予約されています。
/// 現在はアクティブに使用されていません。実装計画については CLAUDE.md を参照してください。
/// </remarks>
public static class AuthorizationHelper
{
    /// <summary>
    /// ユーザーの施設に関連するロールを取得します
    /// </summary>
    /// <param name="user">ユーザー</param>
    /// <param name="facilityId">施設ID（nullの場合は全施設のロールを取得）</param>
    /// <returns>ロールコードの配列</returns>
    public static string[] GetUserRoles(User user, Guid? facilityId = null)
    {
        return user.FacilityUsers
            .Where(fu => !fu.IsDeleted && (facilityId == null || fu.FacilityId == facilityId))
            .SelectMany(fu => fu.FacilityUserRoles)
            .Where(fur => !fur.IsDeleted)
            .Select(fur => fur.RoleCode)
            .Distinct()
            .ToArray();
    }

    /// <summary>
    /// ユーザーが指定施設の管理者かどうかを確認します
    /// </summary>
    /// <param name="user">ユーザー</param>
    /// <param name="facilityId">施設ID</param>
    /// <returns>施設管理者の場合はtrue</returns>
    public static bool IsFacilityAdmin(User user, Guid facilityId)
    {
        return user.FacilityUsers
            .Any(fu => fu.FacilityId == facilityId && fu.IsFacilityAdmin && !fu.IsDeleted);
    }

    /// <summary>
    /// 施設の表示名を取得します
    /// </summary>
    /// <param name="facility">施設</param>
    /// <returns>表示名（FacilityNameDisplay優先、なければFacilityName）</returns>
    public static string GetFacilityDisplayName(Facility? facility)
    {
        if (facility == null) return "";
        return !String.IsNullOrEmpty(facility.FacilityNameDisplay)
            ? facility.FacilityNameDisplay
            : facility.FacilityName;
    }
}
