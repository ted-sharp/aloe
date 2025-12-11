using System.Security.Claims;

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
    /// JWTクレームからユーザー情報を初期化します。
    /// </summary>
    void InitializeFromClaims(ClaimsPrincipal principal);

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
}

