using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.CoreLib.Util;

public static class DateTimeExtensions
{
    // 特殊パターン
    private static readonly string[] s_formats =
    [
        "yyyyMMdd_HHmmss",
        "yyyyMMdd_HHmm",

        "yyyyMMddHHmmss",
        "yyyyMMddHHmm",

        "yyyyMMdd",
        "yyyyMM",

        "yyyy/MM/dd",
        "yyyy-MM-dd",
        "yyyy.MM.dd",

        "yyyy/MM",
        "yyyy-MM",
        "yyyy.MM",
    ];

    private static readonly string[] s_monthFormats =
    [
        "MMMM",
        "MMM",
        "MMM.",
    ];

    private static readonly CultureInfo s_culture = null!;

    private static readonly DateTimeStyles s_styles =
        // タイムゾーンなしはローカルタイムとみなす
        DateTimeStyles.AssumeLocal
        // 先頭の空白文字を無視
        | DateTimeStyles.AllowLeadingWhite
        // 末尾の空白文字を無視
        | DateTimeStyles.AllowTrailingWhite;

    static DateTimeExtensions()
    {
        if (CultureInfo.CurrentCulture.Clone() is CultureInfo culture)
        {
            // 和暦をパースできるようにしておく
            culture.DateTimeFormat.Calendar = new JapaneseCalendar();
            s_culture = culture;
        }
    }

    private static bool TryParseFiscalMonth(string s, out DateTime date)
    {
        // 1-2桁の数字なら今年とみなす
        if (s.Length <= 2 &&
            Int32.TryParse(s, out var month))
        {
            if (1 <= month && month <= 12)
            {
                // 2024/03 に 12 と入力された場合、2023/12 としたい
                var fiscalYear = DateTime.Today.AddMonths(3).Year;

                // 2024/04 に 03 と入力された場合、2025/03 としたい
                if (month <= 3)
                {
                    fiscalYear++;
                }

                date = new DateTime(fiscalYear, month, 1);
                return true;
            }
        }

        // 月名のパース
        if (DateTime.TryParseExact(s, s_monthFormats, s_culture, s_styles, out var monthDate))
        {
            var today = DateTime.Today;
            date = new DateTime(today.Year, monthDate.Month, today.Day);
            return true;
        }

        date = DateTime.MinValue;
        return false;
    }

    public static DateTime ToDateTimeOrDefault(this string dateString, DateTime defaultDateTime)
    {
        if (String.IsNullOrWhiteSpace(dateString))
        {
            return defaultDateTime;
        }

        // 1-2桁の数字なら今年とみなす
        if (DateTimeExtensions.TryParseFiscalMonth(dateString, out var date))
        {
            return date;
        }

        // まずは特殊フォーマットでパース
        var inv = CultureInfo.InvariantCulture;
        if (DateTime.TryParseExact(dateString, s_formats, inv, s_styles, out date))
        {
            return date;
        }

        // その後通常フォーマットでパース
        if (DateTime.TryParse(dateString, s_culture, s_styles, out date))
        {
            return date;
        }

        return defaultDateTime;
    }

    public static DateTime ToDateTimeOrNow(this string dateString)
    {
        return ToDateTimeOrDefault(dateString, DateTime.Now);
    }

    public static DateTime ToDateOrToday(this string dateString)
    {
        return ToDateTimeOrDefault(dateString, DateTime.Today).Date;
    }

}
