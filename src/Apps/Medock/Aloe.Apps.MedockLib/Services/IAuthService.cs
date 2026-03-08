namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 認証サービスインターフェース
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// ログイン処理を行います。
    /// </summary>
    Task<AuthResult> LoginAsync(string userCode, string password, string appName, string ipAddress, string userAgent);

    /// <summary>
    /// クッキー認証を更新します。
    /// </summary>
    Task<AuthResult> RefreshTokenAsync(Guid userId, Guid? facilityId = null);

    /// <summary>
    /// 施設を切り替えて新しいトークンを発行します。
    /// </summary>
    Task<AuthResult> SwitchFacilityAsync(Guid userId, Guid facilityId);

    /// <summary>
    /// ユーザーがアクセス可能な施設一覧を取得します。
    /// </summary>
    Task<List<FacilityInfo>> GetAccessibleFacilitiesAsync(Guid userId);

    /// <summary>
    /// セッションを検証します。
    /// </summary>
    Task<SessionValidationResult> ValidateSessionAsync(Guid sessionId);

    /// <summary>
    /// ログアウト処理を行います。
    /// </summary>
    Task<bool> LogoutAsync(Guid sessionId);
}

