namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// DateTime関連のユーティリティメソッド
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// DateTime値を秒単位で丸めます（マイクロ秒以下を切り捨て）。
    /// 楽観的ロックやタイムスタンプ比較で使用します。
    /// </summary>
    /// <param name="dateTime">丸めるDateTime値</param>
    /// <returns>秒単位で丸められたDateTime値</returns>
    public static DateTime RoundToSeconds(DateTime dateTime)
    {
        return new DateTime(dateTime.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond);
    }
}
