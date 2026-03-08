namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// クッキー認証の設定
/// </summary>
public class CookieSettings
{
    /// <summary>設定セクション名</summary>
    public const string SectionName = "Cookie";

    /// <summary>クッキーの有効期限（分）</summary>
    public int? ExpireTimeSpanMinutes { get; set; }

    /// <summary>スライディング有効期限を使用するか</summary>
    public bool? SlidingExpiration { get; set; }
}



