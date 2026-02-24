// <copyright file="LoggerDumpExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;

namespace Aloe.Utils.Logging.Dump;

/// <summary>
/// ログ出力を拡張するための静的クラスです。
/// </summary>
public static class LoggerDumpExtensions
{
    /// <summary>
    /// バイト配列をダンプしてログ出力します。
    /// </summary>
    /// <param name="logger">ログ出力先の <see cref="ILogger"/> インスタンス。</param>
    /// <param name="bytes">ダンプするバイト配列。</param>
    /// <exception cref="ArgumentNullException">loggerまたはbytesがnullの場合。</exception>
    public static void DumpDebug(
        this ILogger logger,
        byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(bytes);
        
        if (!logger.IsEnabled(LogLevel.Debug) || bytes.Length == 0)
        {
            return;
        }

        const string h1 = "00000000 | 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F | 0123456789ABCDEF";
        const string h2 = "-------- | -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- | ----------------";

        // 1) 初期バッファはスタック上に確保 (512文字)
        var vsb = new ValueStringBuilder(stackalloc char[512]);

        // 2) ヘッダー部分
        vsb.AppendLine();
        vsb.AppendLine(h1);
        vsb.AppendLine(h2);

        // 3) 各 16 バイトを16進ダンプ
        for (var i = 0; i < bytes.Length; i += 16)
        {
            // オフセットアドレスを表示
            vsb.Append($"{i:X8} | ");

            var count = Math.Min(16, bytes.Length - i);
            for (var j = 0; j < count; j++)
            {
                var v = bytes[i + j];

                // 上位4ビット
                var hi = v >> 4;
                vsb.Append((char)(hi < 10 ? '0' + hi : 'A' + hi - 10));

                // 下位4ビット
                var lo = v & 0xF;
                vsb.Append((char)(lo < 10 ? '0' + lo : 'A' + lo - 10));
                if (j < count - 1)
                {
                    vsb.Append(' ');
                }
            }

            // 16バイト未満の場合の空白埋め
            if (count < 16)
            {
                vsb.Append(new string(' ', (16 - count) * 3));
            }

            // ASCII表現を表示
            vsb.Append(" | ");
            for (var j = 0; j < count; j++)
            {
                var v = bytes[i + j];
                var c = (v is >= 32 and <= 126) ? (char)v : '.';
                vsb.Append(c);
            }

            vsb.AppendLine();
        }

        // 4) 完成した文字列を取得してログ出力
        //    内部でプール返却も行われます
        var dump = vsb.ToString();
        logger.LogDebug(dump);
    }
}
