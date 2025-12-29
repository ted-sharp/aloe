using Aloe.Apps.MedockLib.Services;

namespace Aloe.Apps.MedockLib.Logging;

/// <summary>
/// IUserContextServiceと連携したロギング拡張メソッド
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// UserContextServiceから現在のテナント情報を取得してタプルで返します
    /// </summary>
    public static (Guid? TenantId, Guid? FacilityId, Guid? UserId) GetTenantContext(this IUserContextService? userContextService)
    {
        if (userContextService?.CurrentUser == null)
        {
            return (null, null, null);
        }

        return (
            userContextService.CurrentUser.TenantId,
            userContextService.CurrentUser.FacilityId,
            userContextService.CurrentUser.UserId
        );
    }
}
