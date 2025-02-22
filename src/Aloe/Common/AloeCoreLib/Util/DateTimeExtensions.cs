
using System.Globalization;

namespace Aloe.Common.AloeCoreLib.Util;

public static class DateTimeExtensions
{
    //// 特殊パターン
    //private static readonly string[] s_formats =
    //[
    //    "yyyyMMdd_HHmmss",
    //    "yyyyMMdd_HHmm",

    //    "yyyyMMddHHmmss",
    //    "yyyyMMddHHmm",

    //    "yyyyMMdd",
    //    "yyyyMM",

    //    "yyyy/MM/dd",
    //    "yyyy-MM-dd",
    //    "yyyy.MM.dd",

    //    "yyyy/M/d",
    //    "yyyy-M-d",
    //    "yyyy.M.d",

    //    "yyyy/MM",
    //    "yyyy-MM",
    //    "yyyy.MM",

    //    "yyyy/M",
    //    "yyyy-M",
    //    "yyyy.M",
    //];

    //private static bool TryParseMonthName(string monthName, out int month)
    //{
    //    // 小文字で判定、ピリオドは無視
    //    var normalized = monthName.ToLower().Replace(".", "");

    //    month = normalized switch
    //    {
    //        // 和風月名
    //        "睦月" or "むつき" or "ムツキ" => 1,
    //        "如月" or "きさらぎ" or "キサラギ" => 2,
    //        "弥生" or "やよい" or "ヤヨイ" => 3,
    //        "卯月" or "うづき" or "ウヅキ" => 4,
    //        "皐月" or "さつき" or "サツキ" => 5,
    //        "水無月" or "みなづき" or "ミナヅキ" => 6,
    //        "文月" or "ふみづき" or "フミヅキ" => 7,
    //        "葉月" or "はづき" or "ハヅキ" => 8,
    //        "長月" or "ながつき" or "ナガツキ" => 9,
    //        "神無月" or "かんなづき" or "カンナヅキ" => 10,
    //        "霜月" or "しもつき" or "シモツキ" => 11,
    //        "師走" or "しわす" or "シワス" => 12,

    //        // 英語月名
    //        "jan" or "january" => 1,
    //        "feb" or "february" => 2,
    //        "mar" or "march" => 3,
    //        "apr" or "april" => 4,
    //        "may" => 5,
    //        "jun" or "june" => 6,
    //        "jul" or "july" => 7,
    //        "aug" or "august" => 8,
    //        "sep" or "sept" or "september" => 9,
    //        "oct" or "octo" or "october" => 10,
    //        "nov" or "novem" or "november" => 11,
    //        "dec" or "decem" or "december" => 12,

    //        // 数値表記
    //        "1" or "01" or "一月" or "1月" or "1gatsu" => 1,
    //        "2" or "02" or "二月" or "2月" or "2gatsu" => 2,
    //        "3" or "03" or "三月" or "3月" or "3gatsu" => 3,
    //        "4" or "04" or "四月" or "4月" or "4gatsu" => 4,
    //        "5" or "05" or "五月" or "5月" or "5gatsu" => 5,
    //        "6" or "06" or "六月" or "6月" or "6gatsu" => 6,
    //        "7" or "07" or "七月" or "7月" or "7gatsu" => 7,
    //        "8" or "08" or "八月" or "8月" or "8gatsu" => 8,
    //        "9" or "09" or "九月" or "9月" or "9gatsu" => 9,
    //        "10" or "十月" or "10月" or "10gatsu" => 10,
    //        "11" or "十一月" or "11月" or "11gatsu" => 11,
    //        "12" or "十二月" or "12月" or "12gatsu" => 12,

    //        // 不明な場合
    //        _ => 0,
    //    };

    //    return month != 0;
    //}

    //private static readonly CultureInfo s_jaCulture = null!;

    //private static readonly DateTimeStyles s_styles =
    //    // タイムゾーンなしはローカルタイムとみなす
    //    DateTimeStyles.AssumeLocal
    //    // 先頭の空白文字を無視
    //    | DateTimeStyles.AllowLeadingWhite
    //    // 末尾の空白文字を無視
    //    | DateTimeStyles.AllowTrailingWhite;

    //static DateTimeExtensions()
    //{
    //    if (CultureInfo.CurrentCulture.Clone() is CultureInfo culture)
    //    {
    //        // 和暦をパースできるようにしておく
    //        culture.DateTimeFormat.Calendar = new JapaneseCalendar();
    //        DateTimeExtensions.s_jaCulture = culture;
    //    }
    //}

    //private static bool TryParseFiscalMonth(string s, out DateTime date)
    //{
    //    // 1-2桁の数字なら今年とみなす
    //    if (s.Length <= 2 && Int32.TryParse(s, out var month) ||
    //        // 月名も解釈する
    //        TryParseMonthName(s, out month))
    //    {
    //        if (1 <= month && month <= 12)
    //        {
    //            // 2024/03 に 12 と入力された場合、2023/12 としたい
    //            var fiscalYear = DateTime.Today.AddMonths(3).Year;

    //            // 2024/04 に 03 と入力された場合、2025/03 としたい
    //            if (month <= 3)
    //            {
    //                fiscalYear++;
    //            }

    //            date = new DateTime(fiscalYear, month, 1);
    //            return true;
    //        }
    //    }

    //    date = DateTime.MinValue;
    //    return false;
    //}

    //public static DateTime ToDateTimeOrDefault(this string dateString, DateTime defaultDateTime)
    //{
    //    if (String.IsNullOrWhiteSpace(dateString))
    //    {
    //        return defaultDateTime;
    //    }

    //    // 1-2桁の数字なら今年とみなす
    //    if (DateTimeExtensions.TryParseFiscalMonth(dateString, out var date))
    //    {
    //        return date;
    //    }

    //    // まずは想定フォーマットでパース
    //    var inv = CultureInfo.InvariantCulture;
    //    if (DateTime.TryParseExact(dateString, s_formats, inv, s_styles, out date))
    //    {
    //        return date;
    //    }

    //    // その後和暦フォーマットでパース
    //    if (DateTime.TryParse(dateString, s_jaCulture, s_styles, out date))
    //    {
    //        return date;
    //    }

    //    return defaultDateTime;
    //}

    //public static DateTime ToDateTimeOrNow(this string dateString)
    //{
    //    return ToDateTimeOrDefault(dateString, DateTime.Now);
    //}

    //public static DateTime ToDateOrToday(this string dateString)
    //{
    //    return ToDateTimeOrDefault(dateString, DateTime.Today).Date;
    //}

    //public static DateTime ToMonthEndDateOrCurrentMonth(this string dateString)
    //{
    //    var today = DateTime.Today;
    //    var date = ToDateTimeOrDefault(dateString, today);
    //    return new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1).Date;
    //}
}
