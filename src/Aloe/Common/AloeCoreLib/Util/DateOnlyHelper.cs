using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Common.AloeCoreLib.Util;

public static class DateOnlyHelper
{
    // 特殊パターン
    private static readonly string[] s_formats =
    [
        "yyyyMMdd",
        "yyyyMM",

        "yyyy/MM/dd",
        "yyyy-MM-dd",
        "yyyy.MM.dd",

        "yyyy/M/d",
        "yyyy-M-d",
        "yyyy.M.d",

        "yyyy/MM",
        "yyyy-MM",
        "yyyy.MM",

        "yyyy/M",
        "yyyy-M",
        "yyyy.M",
    ];

    private static bool TryParseMonthName(string monthName, out int month)
    {
        // 小文字で判定、ピリオドは無視
        var normalized = monthName.ToLower().Replace(".", "");

        month = normalized switch
        {
            // 和風月名
            "睦月" or "むつき" or "ムツキ" => 1,
            "如月" or "きさらぎ" or "キサラギ" => 2,
            "弥生" or "やよい" or "ヤヨイ" => 3,
            "卯月" or "うづき" or "ウヅキ" => 4,
            "皐月" or "さつき" or "サツキ" => 5,
            "水無月" or "みなづき" or "ミナヅキ" => 6,
            "文月" or "ふみづき" or "フミヅキ" => 7,
            "葉月" or "はづき" or "ハヅキ" => 8,
            "長月" or "ながつき" or "ナガツキ" => 9,
            "神無月" or "かんなづき" or "カンナヅキ" => 10,
            "霜月" or "しもつき" or "シモツキ" => 11,
            "師走" or "しわす" or "シワス" => 12,

            // 英語月名
            "jan" or "january" => 1,
            "feb" or "february" => 2,
            "mar" or "march" => 3,
            "apr" or "april" => 4,
            "may" => 5,
            "jun" or "june" => 6,
            "jul" or "july" => 7,
            "aug" or "august" => 8,
            "sep" or "sept" or "september" => 9,
            "oct" or "octo" or "october" => 10,
            "nov" or "novem" or "november" => 11,
            "dec" or "decem" or "december" => 12,

            // 数値表記
            "1" or "01" or "一月" or "1月" or "1gatsu" => 1,
            "2" or "02" or "二月" or "2月" or "2gatsu" => 2,
            "3" or "03" or "三月" or "3月" or "3gatsu" => 3,
            "4" or "04" or "四月" or "4月" or "4gatsu" => 4,
            "5" or "05" or "五月" or "5月" or "5gatsu" => 5,
            "6" or "06" or "六月" or "6月" or "6gatsu" => 6,
            "7" or "07" or "七月" or "7月" or "7gatsu" => 7,
            "8" or "08" or "八月" or "8月" or "8gatsu" => 8,
            "9" or "09" or "九月" or "9月" or "9gatsu" => 9,
            "10" or "十月" or "10月" or "10gatsu" => 10,
            "11" or "十一月" or "11月" or "11gatsu" => 11,
            "12" or "十二月" or "12月" or "12gatsu" => 12,

            // 不明な場合
            _ => 0,
        };

        return month != 0;
    }

    private static readonly CultureInfo s_jaCulture = null!;

    private static readonly DateTimeStyles s_styles =
        // タイムゾーンなしはローカルタイムとみなす
        DateTimeStyles.AssumeLocal
        // 先頭の空白文字を無視
        | DateTimeStyles.AllowLeadingWhite
        // 末尾の空白文字を無視
        | DateTimeStyles.AllowTrailingWhite;



    static DateOnlyHelper()
    {
        if (CultureInfo.CurrentCulture.Clone() is CultureInfo culture)
        {
            // 和暦をパースできるようにしておく
            culture.DateTimeFormat.Calendar = new JapaneseCalendar();
            DateOnlyHelper.s_jaCulture = culture;
        }
    }

    private static bool TryParseFiscalMonth(string s, out DateOnly date)
    {
        // 1-2桁の数字なら今年とみなす、月名も解釈する
        if (TryParseMonthName(s, out var month))
        {
            var fiscalYear = DateTime.Today.Year;

            var currentMonth = DateTime.Today.Month;
            //var currentMonth = 2;
            //var currentMonth = 4;

            // 4月以降に 1, 2, 3 が入力された場合は来年とする
            // 2025/4 に 1 が入力された場合は 2026/1 とする
            if (4 <= currentMonth && month <= 3)
            {
                fiscalYear++;
            }

            // 3月以前で今月以前の 1, 2, 3 が入力された場合は来年とする
            // 2025/2 に 1 が入力された場合は 2026/1 とする
            // 2025/2 に 3 が入力された場合は 2025/3 とする
            if (currentMonth <= 3 && month <= 3 && month < currentMonth)
            {
                fiscalYear++;
            }

            date = new DateOnly(fiscalYear, month, 1);
            return true;
        }

        date = DateOnly.MinValue;
        return false;
    }

    public static bool TryParseEx(string dateString, out DateOnly date)
    {
        if (String.IsNullOrWhiteSpace(dateString))
        {
            date = default;
            return false;
        }

        // 1-2桁の数字なら今年とみなす
        if (DateOnlyHelper.TryParseFiscalMonth(dateString, out date))
        {
            return true;
        }

        // まずは想定フォーマットでパース
        var inv = CultureInfo.InvariantCulture;
        if (DateOnly.TryParseExact(dateString, s_formats, inv, s_styles, out date))
        {
            return true;
        }

        // その後和暦フォーマットでパース
        return DateOnly.TryParse(dateString, s_jaCulture, s_styles, out date);
    }

    /// <summary>
    /// 今日を返します。
    /// </summary>
    public static DateOnly GetToday()
    {
        return DateOnly.FromDateTime(DateTime.Today);
    }

    /// <summary>
    /// 月初(1日)を返します。
    /// </summary>
    public static DateOnly GetFirstDate()
    {
        var today = DateTime.Today;
        return new DateOnly(today.Year, today.Month, 1);
    }

    /// <summary>
    /// 月初(1日)を返します。
    /// </summary>
    public static DateTime GetFirstDateTime()
    {
        var today = DateTime.Today;
        return new DateTime(today.Year, today.Month, 1);
    }

    /// <summary>
    /// 月初(1日)を返します。
    /// </summary>
    public static DateOnly GetFirstDate(int year, int month)
    {
        return new DateOnly(year, month, 1);
    }

    /// <summary>
    /// 月初(1日)を返します。
    /// </summary>
    public static DateTime GetFirstDateTime(int year, int month)
    {
        return new DateTime(year, month, 1);
    }

    /// <summary>
    /// 月末(31日など)を返します。
    /// </summary>
    public static DateOnly GetEndDate(DateOnly date)
    {
        return new DateOnly(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
    }

    /// <summary>
    /// 月末(31日など)を返します。
    /// </summary>
    public static DateTime GetEndDateTime(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
    }
}
