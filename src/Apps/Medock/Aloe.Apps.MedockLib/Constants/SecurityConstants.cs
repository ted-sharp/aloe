namespace Aloe.Apps.MedockLib.Constants;

/// <summary>
/// セキュリティ・暗号化関連の定数
/// </summary>
/// <remarks>
/// パスワードハッシング、ソルト、その他セキュリティ関連の設定値を一元管理します。
/// </remarks>
public static class SecurityConstants
{
    // ========================================
    // パスワードハッシング (PBKDF2-SHA256)
    // ========================================

    /// <summary>
    /// PBKDF2 イテレーション数
    /// </summary>
    /// <remarks>
    /// NIST の推奨値：最小 1,000, 実運用では 10,000 以上を推奨
    /// PasswordHasher で使用される。値が大きいほど安全性向上、CPU使用量増加
    /// </remarks>
    public const int PasswordHashingIterations = 10000;

    /// <summary>
    /// ソルトサイズ（バイト）
    /// </summary>
    /// <remarks>
    /// パスワードハッシング時に使用されるランダムソルトのサイズ
    /// 最小 16 バイト（128 ビット）推奨
    /// </remarks>
    public const int SaltSizeBytes = 16;

    /// <summary>
    /// ハッシュサイズ（バイト）
    /// </summary>
    /// <remarks>
    /// PBKDF2-SHA256 で生成されるハッシュサイズ
    /// SHA256 = 32 バイト
    /// </remarks>
    public const int HashSizeBytes = 32;

    // ========================================
    // トークン・暗号化
    // ========================================

    /// <summary>
    /// JWT トークン最小有効期限（秒）
    /// </summary>
    public const int TokenMinExpirationSeconds = 60;

    /// <summary>
    /// JWT トークン最大有効期限（秒） - 1時間
    /// </summary>
    public const int TokenMaxExpirationSeconds = 3600;

    // ========================================
    // セキュリティ検証
    // ========================================

    /// <summary>
    /// メールアドレスの最大長
    /// </summary>
    public const int MaxEmailLength = 254;

    /// <summary>
    /// ユーザーコードの最大長
    /// </summary>
    public const int MaxUserCodeLength = 100;
}
