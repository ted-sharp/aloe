using System.Text.RegularExpressions;

namespace Aloe.Apps.MedockLib.Services.VoiceCommand;

/// <summary>
/// 日本語の日時表現を解析するパーサー
/// </summary>
public static partial class JapaneseDateParser
{
    /// <summary>
    /// 日本語テキストから日付を抽出する
    /// </summary>
    public static DateOnly? ParseDate(string text, DateOnly today)
    {
        // 相対日付（長い表現を先にチェック）
        if (text.Contains("一昨日"))
            return today.AddDays(-2);

        if (text.Contains("三日後"))
            return today.AddDays(3);

        if (text.Contains("明後日"))
            return today.AddDays(2);

        if (text.Contains("昨日"))
            return today.AddDays(-1);

        if (text.Contains("今日"))
            return today;

        if (text.Contains("明日"))
            return today.AddDays(1);

        // 曜日指定: 「来週の月曜日」「今週金曜」「再来週水曜日」
        var weekdayDate = ParseWeekday(text, today);
        if (weekdayDate.HasValue)
            return weekdayDate;

        // 「来月の5日」「先月10日」「再来月1日」
        var relativeMonthDate = ParseRelativeMonth(text, today);
        if (relativeMonthDate.HasValue)
            return relativeMonthDate;

        // 絶対日付: 「2026年3月5日」「3月5日」
        var absoluteDate = ParseAbsoluteDate(text, today);
        if (absoluteDate.HasValue)
            return absoluteDate;

        // 月末パターン（来月末、先月末、再来月末、今月末、月末）
        var monthEndDate = ParseMonthEnd(text, today);
        if (monthEndDate.HasValue)
            return monthEndDate;

        // 「来月」「先月」（日付指定なし → その月の1日）
        if (text.Contains("再来月"))
            return new DateOnly(today.Year, today.Month, 1).AddMonths(2);

        if (text.Contains("来月"))
            return new DateOnly(today.Year, today.Month, 1).AddMonths(1);

        if (text.Contains("先月"))
            return new DateOnly(today.Year, today.Month, 1).AddMonths(-1);

        return null;
    }

    private static DateOnly? ParseMonthEnd(string text, DateOnly today)
    {
        DateOnly baseDate;
        if (text.Contains("再来月末"))
            baseDate = new DateOnly(today.Year, today.Month, 1).AddMonths(2);
        else if (text.Contains("来月末"))
            baseDate = new DateOnly(today.Year, today.Month, 1).AddMonths(1);
        else if (text.Contains("先月末"))
            baseDate = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);
        else if (text.Contains("今月末") || text.Contains("月末"))
            baseDate = new DateOnly(today.Year, today.Month, 1);
        else
            return null;

        var lastDay = DateTime.DaysInMonth(baseDate.Year, baseDate.Month);
        return new DateOnly(baseDate.Year, baseDate.Month, lastDay);
    }

    /// <summary>
    /// 日本語テキストから時刻（0:00からの分数）を抽出する
    /// </summary>
    public static int? ParseTime(string text)
    {
        // 「午後3時30分」「午後3時半」「午後3時」
        var pmMatch = PmTimeRegex().Match(text);
        if (pmMatch.Success)
        {
            var hour = Int32.Parse(pmMatch.Groups["hour"].Value);
            if (hour < 12) hour += 12;
            var min = ParseMinutePart(pmMatch);
            return hour * 60 + min;
        }

        // 「午前10時30分」「午前10時半」「午前10時」
        var amMatch = AmTimeRegex().Match(text);
        if (amMatch.Success)
        {
            var hour = Int32.Parse(amMatch.Groups["hour"].Value);
            var min = ParseMinutePart(amMatch);
            return hour * 60 + min;
        }

        // 「10時30分」「10時半」「10時」
        var timeMatch = TimeRegex().Match(text);
        if (timeMatch.Success)
        {
            var hour = Int32.Parse(timeMatch.Groups["hour"].Value);
            var min = ParseMinutePart(timeMatch);

            // 午前/午後の指定なしで 1〜7時 → 業務時間外なので午後と推定
            if (hour >= 1 && hour <= 7)
                hour += 12;

            return hour * 60 + min;
        }

        return null;
    }

    private static int ParseMinutePart(Match match)
    {
        if (match.Groups["half"].Success)
            return 30;

        if (match.Groups["min"].Success)
            return Int32.Parse(match.Groups["min"].Value);

        return 0;
    }

    private static DateOnly? ParseWeekday(string text, DateOnly today)
    {
        var match = WeekdayRegex().Match(text);
        if (!match.Success)
            return null;

        var prefix = match.Groups["prefix"].Value;
        var dayName = match.Groups["day"].Value;

        var targetDow = dayName switch
        {
            "月" => DayOfWeek.Monday,
            "火" => DayOfWeek.Tuesday,
            "水" => DayOfWeek.Wednesday,
            "木" => DayOfWeek.Thursday,
            "金" => DayOfWeek.Friday,
            "土" => DayOfWeek.Saturday,
            "日" => DayOfWeek.Sunday,
            _ => (DayOfWeek?)null,
        };

        if (targetDow is null)
            return null;

        // 今日の曜日からの差分計算
        var todayDow = today.DayOfWeek;
        var diff = ((int)targetDow.Value - (int)todayDow + 7) % 7;

        if (prefix is "来週" or "来週の")
        {
            // 来週: 次の週の該当曜日（最低7日後）
            diff = diff == 0 ? 7 : diff;
            // 今週残り日数 + 来週の該当曜日
            var daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)todayDow + 7) % 7;
            if (daysUntilNextMonday == 0) daysUntilNextMonday = 7;
            diff = daysUntilNextMonday + ((int)targetDow.Value - (int)DayOfWeek.Monday + 7) % 7;
        }
        else if (prefix is "再来週" or "再来週の")
        {
            // 再来週: 2週間後の週の該当曜日
            var daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)todayDow + 7) % 7;
            if (daysUntilNextMonday == 0) daysUntilNextMonday = 7;
            diff = daysUntilNextMonday + 7 + ((int)targetDow.Value - (int)DayOfWeek.Monday + 7) % 7;
        }
        else
        {
            // 今週 or プレフィックスなし: 今週の残りの該当曜日（0の場合は7日後）
            if (diff == 0) diff = 7;
        }

        return today.AddDays(diff);
    }

    private static DateOnly? ParseRelativeMonth(string text, DateOnly today)
    {
        // 「来月の5日」「来月5日」「先月10日」「再来月1日」
        var match = RelativeMonthDayRegex().Match(text);
        if (!match.Success)
            return null;

        var prefix = match.Groups["prefix"].Value;
        var day = Int32.Parse(match.Groups["day"].Value);

        var monthOffset = prefix switch
        {
            "来月" => 1,
            "先月" => -1,
            "再来月" => 2,
            _ => 0,
        };

        var baseDate = new DateOnly(today.Year, today.Month, 1).AddMonths(monthOffset);
        var maxDay = DateTime.DaysInMonth(baseDate.Year, baseDate.Month);
        return new DateOnly(baseDate.Year, baseDate.Month, Math.Min(day, maxDay));
    }

    private static DateOnly? ParseAbsoluteDate(string text, DateOnly today)
    {
        // 「2026年3月5日」
        var fullMatch = FullDateRegex().Match(text);
        if (fullMatch.Success)
        {
            var year = Int32.Parse(fullMatch.Groups["year"].Value);
            var month = Int32.Parse(fullMatch.Groups["month"].Value);
            var day = Int32.Parse(fullMatch.Groups["day"].Value);
            return new DateOnly(year, month, day);
        }

        // 「3月5日」
        var shortMatch = ShortDateRegex().Match(text);
        if (shortMatch.Success)
        {
            var month = Int32.Parse(shortMatch.Groups["month"].Value);
            var day = Int32.Parse(shortMatch.Groups["day"].Value);
            var year = today.Year;
            // 過去の月なら来年と推定
            if (month < today.Month || (month == today.Month && day < today.Day))
                year++;
            return new DateOnly(year, month, day);
        }

        // 「5日」（月なし）→ 今月か翌月の該当日
        var dayOnlyMatch = DayOnlyRegex().Match(text);
        if (dayOnlyMatch.Success)
        {
            var day = Int32.Parse(dayOnlyMatch.Groups["day"].Value);
            if (day is < 1 or > 31) return null;

            // 今月の該当日が今日以降なら今月
            var maxDayThisMonth = DateTime.DaysInMonth(today.Year, today.Month);
            if (day <= maxDayThisMonth)
            {
                var candidate = new DateOnly(today.Year, today.Month, day);
                if (candidate >= today)
                    return candidate;
            }

            // 今月が過ぎているなら翌月
            var nextMonth = new DateOnly(today.Year, today.Month, 1).AddMonths(1);
            var maxDayNextMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
            return new DateOnly(nextMonth.Year, nextMonth.Month, Math.Min(day, maxDayNextMonth));
        }

        return null;
    }

    // --- Regex definitions ---

    [GeneratedRegex(@"(?<prefix>来週の?|今週の?|再来週の?)(?<day>[月火水木金土日])曜日?")]
    private static partial Regex WeekdayRegex();

    [GeneratedRegex(@"(?<prefix>来月|先月|再来月)の?(?<day>\d{1,2})日")]
    private static partial Regex RelativeMonthDayRegex();

    [GeneratedRegex(@"(?<year>\d{4})年(?<month>\d{1,2})月(?<day>\d{1,2})日")]
    private static partial Regex FullDateRegex();

    [GeneratedRegex(@"(?<month>\d{1,2})月(?<day>\d{1,2})日")]
    private static partial Regex ShortDateRegex();

    // 「5日」（月なし）: 月・年・数字の直後にならない日のみマッチ。「5日後/前/目/間」は除外
    [GeneratedRegex(@"(?<![月年\d])(?<day>\d{1,2})日(?!後|前|目|間)")]
    private static partial Regex DayOnlyRegex();

    [GeneratedRegex(@"午後(?<hour>\d{1,2})時(?:(?<half>半)|(?<min>\d{1,2})分)?")]
    private static partial Regex PmTimeRegex();

    [GeneratedRegex(@"午前(?<hour>\d{1,2})時(?:(?<half>半)|(?<min>\d{1,2})分)?")]
    private static partial Regex AmTimeRegex();

    [GeneratedRegex(@"(?<hour>\d{1,2})時(?:(?<half>半)|(?<min>\d{1,2})分)?")]
    private static partial Regex TimeRegex();
}
