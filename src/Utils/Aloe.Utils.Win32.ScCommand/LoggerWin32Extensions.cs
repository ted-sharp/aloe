// <copyright file="LoggerWin32Extensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace Aloe.Utils.Win32.ScCommand;

/// <summary>
/// Win32 APIのエラーログ出力を拡張するための静的クラス
/// </summary>
public static class LoggerWin32Extensions
{
    /// <summary>
    /// 最後の Win32 エラー情報を付与してログ出力します。
    /// </summary>
    /// <param name="logger">ロガー（null 許容）。</param>
    /// <param name="message">ログメッセージのテンプレート。</param>
    /// <param name="args">テンプレート用の引数。</param>
    [StackTraceHidden]
    public static void LogWin32Error(
        this ILogger? logger,
        string message,
        params object?[] args)
    {
        // ロガーがnullの場合は何もしない
        if (logger is null)
        {
            return;
        }

        // 最後の Win32 エラーコードを取得
        var errorCode = Marshal.GetLastWin32Error();

        // Win32Exception を作成してスタックトレースにエラー情報を残す
        // これにより、エラーの発生箇所と詳細な情報が記録される
        var ex = new Win32Exception(errorCode, message);

        // ログ出力用の引数配列を作成
        // 元の引数に加えて、Win32エラーコードを最後に追加
        var allArgs = new object?[args.Length + 1];
        args.CopyTo(allArgs, 0);
        allArgs[args.Length] = errorCode;

        // 元のメッセージテンプレートにWin32エラーコードの情報を追記
        var fullTemplate = message + " (Win32Error: {ErrorCode})";

        // エラーログを出力
        // 例外情報と共に、拡張されたメッセージテンプレートと引数を使用
        logger.LogError(ex, fullTemplate, allArgs);
    }
}
