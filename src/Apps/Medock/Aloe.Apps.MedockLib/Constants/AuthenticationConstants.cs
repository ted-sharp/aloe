namespace Aloe.Apps.MedockLib.Constants;

/// <summary>
/// 認証・セッション・Cookie 関連の定数
/// </summary>
/// <remarks>
/// ログイン、セッション管理、Cookie有効期限などの設定値を一元管理します。
/// </remarks>
public static class AuthenticationConstants
{
    // ========================================
    // Cookie & セッション
    // ========================================

    /// <summary>
    /// Cookie 有効期限（分）- デフォルト値
    /// </summary>
    /// <remarks>
    /// AuthService, ServiceCollectionExtensions, CookieSettings で使用される統一値
    /// </remarks>
    public const int DefaultCookieExpireMinutes = 15;

    /// <summary>
    /// セッション再検証間隔（分）
    /// </summary>
    /// <remarks>
    /// CustomAuthenticationStateProvider で定期的に認証状態を再検証する間隔
    /// 必ず CookieExpireMinutes より小さい値である必要があります
    /// </remarks>
    public const int SessionRevalidationMinutes = 5;

    /// <summary>
    /// Refresh Token 有効期限（日数）
    /// </summary>
    public const int RefreshTokenExpirationDays = 7;

    // ========================================
    // アカウントロック
    // ========================================

    /// <summary>
    /// ログイン失敗時のアカウントロック期間（分）
    /// </summary>
    /// <remarks>
    /// MaxFailedLoginAttempts 回連続で失敗した場合、このロック期間アカウントは使用不可
    /// </remarks>
    public const int AccountLockoutMinutes = 15;

    /// <summary>
    /// ロックアウトまでの最大失敗ログイン試行回数
    /// </summary>
    /// <remarks>
    /// AuthService で検証される。この回数失敗すると AccountLockoutMinutes ロックされます
    /// </remarks>
    public const int MaxFailedLoginAttempts = 5;

    // ========================================
    // パスワード & MFA
    // ========================================

    /// <summary>
    /// パスワード最小長（文字）
    /// </summary>
    public const int MinPasswordLength = 8;

    /// <summary>
    /// パスワード最大長（文字）
    /// </summary>
    public const int MaxPasswordLength = 128;

    /// <summary>
    /// MFA を有効にするかのデフォルト設定
    /// </summary>
    public const bool MfaEnabledDefault = false;
}
