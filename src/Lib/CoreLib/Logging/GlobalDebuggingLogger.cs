using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AloeReservationGrid.Lib.CoreLib.Logging;

// TODO: global using はあんまりはやらなさそうなので削除予定です。
/// <summary>
/// global using を設定することを目的としたデバッグ用ロガーです。
/// <example>
/// global using static AloeReservationGrid.Lib.CoreLib.Logging.Log;
/// </example>
/// </summary>
public static class GlobalDebuggingLogger
{
    private static ILoggerFactory? s_loggerFactory;
    private static ILogger? s_logger;

    /// <summary>
    /// ロガーファクトリを設定します。
    /// </summary>
    /// <param name="host">IHost のインスタンス</param>
    public static IHost ConfigureGlobalDebuggingLogger(this IHost host)
    {
        var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();

        GlobalDebuggingLogger.s_loggerFactory = loggerFactory;
        GlobalDebuggingLogger.s_logger = GlobalDebuggingLogger.s_loggerFactory.CreateLogger("Global");

        return host;
    }

    /// <summary>
    /// 情報ログを出力します。
    /// </summary>
    /// <param name="message">ログメッセージ</param>
    public static void Info(string message)
    {
        GlobalDebuggingLogger.s_logger?.Debug(message);
    }

    /// <summary>
    /// 警告ログを出力します。
    /// </summary>
    /// <param name="message">ログメッセージ</param>
    public static void Warn(string message)
    {
        GlobalDebuggingLogger.s_logger?.Warn(message);
    }

    /// <summary>
    /// エラーログを出力します。
    /// </summary>
    /// <param name="message">ログメッセージ</param>
    public static void Error(string message)
    {
        GlobalDebuggingLogger.s_logger?.Error(message);
    }

    /// <summary>
    /// 例外を含むエラーログを出力します。
    /// </summary>
    /// <param name="ex">例外オブジェクト</param>
    /// <param name="message">ログメッセージ</param>
    public static void Error(Exception ex, string message)
    {
        GlobalDebuggingLogger.s_logger?.Error(ex, message);
    }
}
