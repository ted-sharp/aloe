using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AloeReservationGrid.Lib.CoreLib.Logging;

// TODO: ちょっとくらい短くできてもしょうがないので、削除予定です

// CA2254 ログ メッセージ テンプレートは、呼び出しによって異なるべきではありません。
// Serilog などのテンプレートを使用するので、ここではメッセージを抑制します。
#pragma warning disable CA2254

/// <summary>
/// ILogger の拡張メソッドです。
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// 情報ログを出力します。
    /// </summary>
    /// <param name="logger">ロガー</param>
    /// <param name="message">ログメッセージ</param>
    public static ILogger Info(this ILogger logger, string message)
    {
        logger.LogInformation(message);
        return logger;
    }

    /// <summary>
    /// 警告ログを出力します。
    /// </summary>
    /// <param name="logger">ロガー</param>
    /// <param name="message">ログメッセージ</param>
    public static ILogger Warn(this ILogger logger, string message)
    {
        logger.LogWarning(message);
        return logger;
    }

    /// <summary>
    /// 警告ログを出力します。
    /// </summary>
    /// <param name="logger">ロガー</param>
    /// <param name="message">ログメッセージ</param>
    public static ILogger Debug(this ILogger logger, string message)
    {
        logger.LogDebug(message);
        return logger;
    }

    /// <summary>
    /// エラーログを出力します。
    /// </summary>
    /// <param name="logger">ロガー</param>
    /// <param name="message">ログメッセージ</param>
    public static ILogger Error(this ILogger logger, string message)
    {
        logger.LogError(message);
        return logger;
    }

    /// <summary>
    /// 例外を含むエラーログを出力します。
    /// </summary>
    /// <param name="logger">ロガー</param>
    /// <param name="ex">例外オブジェクト</param>
    /// <param name="message">ログメッセージ</param>
    public static ILogger Error(this ILogger logger, Exception ex, string message)
    {
        logger.LogError(ex, message);
        return logger;
    }
}
