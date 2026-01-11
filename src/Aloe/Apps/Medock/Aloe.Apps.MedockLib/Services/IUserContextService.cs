using System.Security.Claims;
using Aloe.Apps.MedockLib.Common;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// ユーザーコンテキストサービスインターフェース
/// </summary>
public interface IUserContextService
{
    /// <summary>
    /// 現在のユーザー情報
    /// </summary>
    UserContextInfo? CurrentUser { get; }

    /// <summary>
    /// 現在のセッションID
    /// </summary>
    Guid? CurrentSessionId { get; }

    /// <summary>
    /// 認証クレームからユーザー情報を初期化します。
    /// </summary>
    void InitializeFromClaims(ClaimsPrincipal principal);

    /// <summary>
    /// 認証クレームからユーザー情報を初期化します（非同期版）。
    /// FacilityIdがクレームにない場合、アクセス可能な施設からデフォルトを取得します。
    /// </summary>
    /// <remarks>
    /// ログインスキップ（前回のセッションが維持されている場合）でも正しく動作するよう、
    /// Cookieにfacility_idが含まれていない場合は、DBからアクセス可能な施設を取得してフォールバックします。
    /// </remarks>
    /// <returns>初期化が成功した場合はtrue、再ログインが必要な場合はfalse</returns>
    Task<bool> InitializeFromClaimsAsync(ClaimsPrincipal principal);

    /// <summary>
    /// セッションIDを設定します（ログイン時に呼び出し）。
    /// </summary>
    void SetSessionId(Guid sessionId);

    /// <summary>
    /// 切替可能な施設一覧を取得します（DBアクセス）。
    /// </summary>
    Task<List<FacilityInfo>> GetAccessibleFacilitiesAsync();

    /// <summary>
    /// ユーザーが複数の施設にアクセス可能かどうかを取得します。
    /// </summary>
    Task<bool> HasMultipleFacilitiesAsync();

    /// <summary>
    /// 現在のユーザーのテナント、施設、ユーザーIDを取得します。
    /// </summary>
    /// <returns>(TenantId, FacilityId, UserId) のタプル</returns>
    (Guid? TenantId, Guid? FacilityId, Guid? UserId) GetTenantContext();

    /// <summary>
    /// 現在のユーザーのテナント、施設、ユーザーID を値オブジェクトで取得します。
    /// </summary>
    /// <returns>TenantContext 値オブジェクト</returns>
    TenantContext GetTenantContextValue();
}

