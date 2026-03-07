using System.Text.RegularExpressions;

namespace Aloe.Apps.MedockServer.ApplicationServices.Mobile;

/// <summary>
/// UserAgent 文字列からモバイル端末かどうかを判定するヘルパー
/// </summary>
public static class MobileDetectionHelper
{
    private static readonly Regex Pattern =
        new(@"iPhone|Android|Mobile", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool IsMobile(string userAgent)
        => !string.IsNullOrEmpty(userAgent) && Pattern.IsMatch(userAgent);
}
