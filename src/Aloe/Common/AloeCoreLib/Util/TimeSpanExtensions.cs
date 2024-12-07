using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Common.AloeCoreLib.Util;

public static class TimeSpanExtensions
{
    /// <summary>
    /// 日、時、分、秒を表す文字列に変換します。
    /// 存在しない場合は省略されます。
    /// </summary>
    /// <returns>
    /// 「99日23時59分59秒」 というフォーマットになります。
    /// </returns>
    public static string ToJaString(this TimeSpan span)
    {
        var parts = new List<string>();

        // 日、時、分、秒をそれぞれチェックし、必要な部分のみ表示
        if (span.Days > 0)
        {
            parts.Add($"{span.Days}日");
        }

        if (span.Hours > 0)
        {
            parts.Add($"{span.Hours}時間");
        }

        if (span.Minutes > 0)
        {
            parts.Add($"{span.Minutes}分");
        }

        if (span.Seconds > 0)
        {
            parts.Add($"{span.Seconds}秒");
        }

        return String.Join(" ", parts);
    }

    /// <summary>
    /// 日、時、分、秒のいずれかの最大単位で約XXの形式で文字列を返します。
    /// 次の単位が半分を超える場合は四捨五入します。
    /// </summary>
    /// <returns>
    /// 「約99日」「約23時間」「約59分」など、四捨五入して最も大きな単位で表されます。
    /// 秒の場合は「59秒」とそのまま表されます。
    /// </returns>
    public static string ToApproximateJaString(this TimeSpan span)
    {
        if (span.Days > 0)
        {
            // 時が12時間以上なら日数を繰り上げ
            var days = span.Days + (span.Hours >= 12 ? 1 : 0);
            return $"約{days}日";
        }

        if (span.Hours > 0)
        {
            // 分が30分以上なら時間を繰り上げ
            var hours = span.Hours + (span.Minutes >= 30 ? 1 : 0);
            return $"約{hours}時間";
        }

        if (span.Minutes > 0)
        {
            // 秒が30秒以上なら分を繰り上げ
            var minutes = span.Minutes + (span.Seconds >= 30 ? 1 : 0);
            return $"約{minutes}分";
        }

        return $"{span.Seconds}秒";
    }
}
