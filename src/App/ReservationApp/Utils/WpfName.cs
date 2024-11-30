using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.App.ReservationApp.Utils;

public static class WpfName
{
    /// <summary>
    /// WPF の識別子名として使用できない文字を返します。
    /// </summary>
    public static string GetInvalidNameChars()
    {
        return "()[]{}.,:;@-";
    }

    public static string GetBracketChars()
    {
        return "([{";
    }

    /// <summary>
    /// 無効な名前の文字を削除または置換します。
    /// </summary>
    /// <param name="s">対象の文字列。</param>
    /// <param name="replacement">置き換え文字。既定では空文字（削除）。</param>
    /// <returns>無効な文字が削除または置換された文字列。</returns>
    public static string ReplaceInvalidNameChars(this string s, string replacement = "_")
    {
        if (String.IsNullOrWhiteSpace(s))
        {
            return s;
        }

        var invalidChars = GetInvalidNameChars();
        return String.Concat(s.Select(c => invalidChars.Contains(c) ? replacement : c.ToString()));
    }

    public static string TrimAfterBrackets(this string s)
    {
        if (String.IsNullOrWhiteSpace(s))
        {
            return s;
        }

        var invalidChars = GetBracketChars().ToCharArray();
        var index = s.IndexOfAny(invalidChars);
        return index >= 0
            ? s.Substring(0, index).TrimEnd()
            : s;
    }
}
