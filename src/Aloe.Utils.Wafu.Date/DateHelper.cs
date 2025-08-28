// <copyright file="DateHelper.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

// ReSharper disable ArrangeStaticMemberQualifier
namespace Aloe.Utils.Wafu.Date;

/// <summary>
/// 日付操作を支援するユーティリティクラスです。
/// </summary>
public static class DateHelper
{
    /// <summary>
    /// 日付文字列を解析して DateOnly 型に変換します。
    /// </summary>
    /// <param name="dateString">解析する日付文字列</param>
    /// <param name="date">解析結果の DateOnly 値</param>
    /// <returns>解析が成功した場合は true、それ以外の場合は false</returns>
    /// <exception cref="ArgumentNullException">dateString が null の場合にスローされます。</exception>
    public static bool TryParseEx(string dateString, out DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(dateString);

        if (String.IsNullOrWhiteSpace(dateString))
        {
            date = default;
            return false;
        }

        if (TryParseFiscalMonth(dateString, out date))
        {
            return true;
        }

        if (DateTime.TryParseExact(
            dateString,
            DateFormatConstants.Formats,
            CultureInfo.InvariantCulture,
            DateFormatConstants.Styles,
            out var dateTime))
        {
            date = DateOnly.FromDateTime(dateTime);
            return true;
        }

        var normalized = dateString.Normalize(NormalizationForm.FormKC);

        if (normalized.IndexOfAny(s_kanjiNumbers) >= 0)
        {
            normalized = ReplaceComplexKanjiNumbers(normalized);
        }

        if (DateTime.TryParse(normalized, s_jaCulture.Value, DateFormatConstants.Styles, out dateTime))
        {
            date = DateOnly.FromDateTime(dateTime);
            return true;
        }

        date = DateOnly.MinValue;
        return false;
    }

    /// <summary>
    /// 今日の日付を DateOnly 型で取得します。
    /// </summary>
    /// <returns>今日の日付</returns>
    public static DateOnly GetToday() =>
        DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// 今月の初日（1日）を DateOnly 型で取得します。
    /// </summary>
    /// <returns>今月の初日</returns>
    public static DateOnly GetFirstDate()
    {
        var today = DateTime.Today;
        return new DateOnly(today.Year, today.Month, 1);
    }

    /// <summary>
    /// 今月の初日（1日）を DateTime 型で取得します。
    /// </summary>
    /// <returns>今月の初日</returns>
    public static DateTime GetFirstDateTime()
    {
        var today = DateTime.Today;
        return new DateTime(today.Year, today.Month, 1);
    }

    /// <summary>
    /// 指定された年月の初日（1日）を DateOnly 型で取得します。
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <returns>指定された年月の初日</returns>
    public static DateOnly GetFirstDate(int year, int month) => new DateOnly(year, month, 1);

    /// <summary>
    /// 指定された年月の初日（1日）を DateTime 型で取得します。
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <returns>指定された年月の初日</returns>
    public static DateTime GetFirstDateTime(int year, int month) => new DateTime(year, month, 1);

    /// <summary>
    /// 指定された日付の月末日を DateOnly 型で取得します。
    /// </summary>
    /// <param name="date">基準となる日付</param>
    /// <returns>指定された日付の月末日</returns>
    public static DateOnly GetEndDate(DateOnly date) =>
        new DateOnly(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);

    /// <summary>
    /// 指定された日付の月末日を DateTime 型で取得します。
    /// </summary>
    /// <param name="date">基準となる日付</param>
    /// <returns>指定された日付の月末日</returns>
    public static DateTime GetEndDateTime(DateTime date) =>
        new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);

    // ---------------------- private fields and helpers (must come after public members) ----------------------

    /// <summary>
    /// 和暦カルチャー情報を遅延初期化で保持します。
    /// </summary>
    private static readonly Lazy<CultureInfo> s_jaCulture = new(() =>
    {
        var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        culture.DateTimeFormat.Calendar = new JapaneseCalendar();
        return culture;
    });

    /// <summary>
    /// 月名とその数値表現のマッピングを保持するディクショナリです。
    /// </summary>
    private static readonly Dictionary<string, int> s_monthNameMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["1"] = 1, ["01"] = 1, ["1月"] = 1, ["一月"] = 1, ["jan"] = 1, ["january"] = 1, ["睦月"] = 1, ["むつき"] = 1, ["ムツキ"] = 1,
        ["2"] = 2, ["02"] = 2, ["2月"] = 2, ["二月"] = 2, ["feb"] = 2, ["february"] = 2, ["如月"] = 2, ["きさらぎ"] = 2, ["キサラギ"] = 2,
        ["3"] = 3, ["03"] = 3, ["3月"] = 3, ["三月"] = 3, ["mar"] = 3, ["march"] = 3, ["弥生"] = 3, ["やよい"] = 3, ["ヤヨイ"] = 3,
        ["4"] = 4, ["04"] = 4, ["4月"] = 4, ["四月"] = 4, ["apr"] = 4, ["april"] = 4, ["卯月"] = 4, ["うづき"] = 4, ["ウヅキ"] = 4,
        ["5"] = 5, ["05"] = 5, ["5月"] = 5, ["五月"] = 5, ["may"] = 5, ["皐月"] = 5, ["さつき"] = 5, ["サツキ"] = 5,
        ["6"] = 6, ["06"] = 6, ["6月"] = 6, ["六月"] = 6, ["jun"] = 6, ["june"] = 6, ["水無月"] = 6, ["みなづき"] = 6, ["ミナヅキ"] = 6,
        ["7"] = 7, ["07"] = 7, ["7月"] = 7, ["七月"] = 7, ["jul"] = 7, ["july"] = 7, ["文月"] = 7, ["ふみづき"] = 7, ["フミヅキ"] = 7,
        ["8"] = 8, ["08"] = 8, ["8月"] = 8, ["八月"] = 8, ["aug"] = 8, ["august"] = 8, ["葉月"] = 8, ["はづき"] = 8, ["ハヅキ"] = 8,
        ["9"] = 9, ["09"] = 9, ["9月"] = 9, ["九月"] = 9, ["sep"] = 9, ["sept"] = 9, ["september"] = 9, ["長月"] = 9, ["ながつき"] = 9, ["ナガツキ"] = 9,
        ["10"] = 10, ["10月"] = 10, ["十月"] = 10, ["oct"] = 10, ["octo"] = 10, ["october"] = 10, ["神無月"] = 10, ["かんなづき"] = 10, ["カンナヅキ"] = 10,
        ["11"] = 11, ["11月"] = 11, ["十一月"] = 11, ["nov"] = 11, ["novem"] = 11, ["november"] = 11, ["霜月"] = 11, ["しもつき"] = 11, ["シモツキ"] = 11,
        ["12"] = 12, ["12月"] = 12, ["十二月"] = 12, ["dec"] = 12, ["decem"] = 12, ["december"] = 12, ["師走"] = 12, ["しわす"] = 12, ["シワス"] = 12,
    };

    /// <summary>
    /// 漢数字を数値に変換するためのマッピングです。
    /// </summary>
    private static readonly Dictionary<char, int> s_kanjiNumericMap = new()
    {
        // 通常漢数字
        ['〇'] = 0, ['一'] = 1, ['二'] = 2, ['三'] = 3, ['四'] = 4, ['亖'] = 4,
        ['五'] = 5, ['六'] = 6, ['七'] = 7, ['八'] = 8, ['九'] = 9,

        // 大字（正式表記）
        ['零'] = 0, ['壱'] = 1, ['壹'] = 1, ['弌'] = 1,
        ['弐'] = 2, ['貳'] = 2, ['貮'] = 2, ['弍'] = 2,
        ['参'] = 3, ['參'] = 3, ['弎'] = 3,
        ['肆'] = 4, ['伍'] = 5, ['陸'] = 6,
        ['漆'] = 7, ['柒'] = 7, ['質'] = 7,
        ['捌'] = 8, ['玖'] = 9,
    };

    /// <summary>
    /// 漢数字の単位を数値に変換するためのマッピングです。
    /// </summary>
    private static readonly Dictionary<char, int> s_kanjiUnitMap = new()
    {
        ['十'] = 10, ['拾'] = 10, ['什'] = 10,
        ['廿'] = 20, ['卄'] = 20,
        ['丗'] = 30, ['卅'] = 30,
        ['卌'] = 40,
        ['百'] = 100, ['佰'] = 100, ['陌'] = 100,
        ['千'] = 1000, ['仟'] = 1000, ['阡'] = 1000,
        ['万'] = 10000, ['萬'] = 10000,
    };

    /// <summary>
    /// 漢数字の文字配列です。
    /// </summary>
    private static readonly char[] s_kanjiNumbers = s_kanjiNumericMap.Keys
        .Concat(s_kanjiUnitMap.Keys)
        .ToArray();

    /// <summary>
    /// 漢数字を検出するための正規表現です。
    /// </summary>
    private static readonly Regex s_kanjiNumberRegex = new(
        $"[{String.Concat(s_kanjiNumbers.Select(c => Regex.Escape(c.ToString())))}]+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// 入力文字列を正規化します。
    /// </summary>
    /// <param name="input">正規化する入力文字列</param>
    /// <returns>正規化された文字列</returns>
    private static string NormalizeInput(string input)
    {
        Span<char> buffer = stackalloc char[input.Length];
        var len = 0;

        foreach (var ch in input)
        {
            if (ch == '.')
            {
                continue;
            }

            buffer[len++] = ch switch
            {
                >= 'Ａ' and <= 'Ｚ' => (char)(ch - 'Ａ' + 'a'),
                >= '０' and <= '９' => (char)(ch - '０' + '0'),
                >= 'A' and <= 'Z' => (char)(ch + 32),
                _ => ch,
            };
        }

        return new string(buffer[..len]);
    }

    /// <summary>
    /// 1-2桁の数字や月名が指定された場合は月数(1-12)にパースします。
    /// </summary>
    /// <param name="input">解析する月名または数値</param>
    /// <param name="month">解析結果の月数</param>
    /// <returns>解析が成功した場合は true、それ以外の場合は false</returns>
    private static bool TryParseMonthName(string input, out int month)
    {
        if (String.IsNullOrEmpty(input))
        {
            month = 0;
            return false;
        }

        var normalized = NormalizeInput(input);
        return s_monthNameMap.TryGetValue(normalized, out month);
    }

    /// <summary>
    /// 漢数字の文字列を数値に変換します。
    /// </summary>
    /// <param name="input">変換する漢数字の文字列</param>
    /// <returns>変換された数値。変換に失敗した場合は -1</returns>
    private static int ParseKanjiNumber(string input)
    {
        if (String.IsNullOrEmpty(input))
        {
            return -1;
        }

        var total = 0;
        var current = 0;
        var hasUnit = false;

        foreach (var ch in input)
        {
            if (s_kanjiNumericMap.TryGetValue(ch, out var digit))
            {
                current = digit;
            }
            else if (s_kanjiUnitMap.TryGetValue(ch, out var unit))
            {
                if (unit >= 20 && current == 0)
                {
                    total += unit;
                }
                else
                {
                    if (current == 0)
                    {
                        current = 1;
                    }

                    total += current * unit;
                }

                current = 0;
                hasUnit = true;
            }
            else
            {
                return -1;
            }
        }

        return hasUnit ? total + current : -1;
    }

    /// <summary>
    /// 文字列内の複合漢数字を数値に置換します。
    /// </summary>
    /// <param name="input">置換対象の文字列</param>
    /// <returns>漢数字が数値に置換された文字列</returns>
    private static string ReplaceComplexKanjiNumbers(string input)
    {
        return s_kanjiNumberRegex.Replace(input, match =>
        {
            var value = ParseKanjiNumber(match.Value);
            return value >= 0 ? value.ToString() : match.Value;
        });
    }

    /// <summary>
    /// 会計年度の月を解析して日付に変換します。
    /// </summary>
    /// <param name="s">解析する月名または数値</param>
    /// <param name="date">解析結果の日付</param>
    /// <returns>解析が成功した場合は true、それ以外の場合は false</returns>
    private static bool TryParseFiscalMonth(string s, out DateOnly date)
    {
        if (TryParseMonthName(s, out var month))
        {
            var fiscalYear = DateTime.Today.Year;
            var currentMonth = DateTime.Today.Month;

            if ((4 <= currentMonth && month <= 3) ||
                (currentMonth <= 3 && month <= 3 && month < currentMonth))
            {
                fiscalYear++;
            }

            date = new DateOnly(fiscalYear, month, 1);
            return true;
        }

        date = DateOnly.MinValue;
        return false;
    }

    // ---------------------- private nested types (should follow fields) ----------------------

    /// <summary>
    /// 指定された年月の初日（1日）を DateOnly 型で取得します。
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <returns>指定された年月の初日</returns>
    public static DateOnly GetFirstDate(int year, int month) => new DateOnly(year, month, 1);

    /// <summary>
    /// 指定された年月の初日（1日）を DateTime 型で取得します。
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <returns>指定された年月の初日</returns>
    public static DateTime GetFirstDateTime(int year, int month) => new DateTime(year, month, 1);

    /// <summary>
    /// 指定された日付の月末日を DateOnly 型で取得します。
    /// </summary>
    /// <param name="date">基準となる日付</param>
    /// <returns>指定された日付の月末日</returns>
    public static DateOnly GetEndDate(DateOnly date) =>
        new DateOnly(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);

    /// <summary>
    /// 指定された日付の月末日を DateTime 型で取得します。
    /// </summary>
    /// <param name="date">基準となる日付</param>
    /// <returns>指定された日付の月末日</returns>
    public static DateTime GetEndDateTime(DateTime date) =>
        new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);

    /// <summary>
    /// 日付フォーマットの定数を保持する内部クラスです。
    /// </summary>
    private static class DateFormatConstants
    {
        /// <summary>
        /// サポートされる日付フォーマットの配列です。
        /// </summary>
        public static readonly string[] Formats =
        [
            "yyyyMMdd",
            "yyyyMM",
            "yyyy年MM月dd日",
            "yyyy/MM/dd",
            "yyyy-MM-dd",
            "yyyy.MM.dd",
            "yyyy年M月d日",
            "yyyy/M/d",
            "yyyy-M-d",
            "yyyy.M.d",
            "yyyy年MM月",
            "yyyy/MM",
            "yyyy-MM",
            "yyyy.MM",
            "yyyy年M月",
            "yyyy/M",
            "yyyy-M",
            "yyyy.M",
        ];

        /// <summary>
        /// 日付解析時に使用するスタイル設定です。
        /// </summary>
        public static readonly DateTimeStyles Styles =
            DateTimeStyles.AssumeLocal |
            DateTimeStyles.AllowLeadingWhite |
            DateTimeStyles.AllowTrailingWhite;
    }
}
