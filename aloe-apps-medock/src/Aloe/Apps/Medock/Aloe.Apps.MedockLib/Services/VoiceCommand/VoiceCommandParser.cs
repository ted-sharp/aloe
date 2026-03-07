using System.Text.RegularExpressions;

namespace Aloe.Apps.MedockLib.Services.VoiceCommand;

/// <summary>
/// 正規表現ベースの音声コマンドパーサー。
/// LLMが使えない閉域環境でのフォールバック用。
/// </summary>
public partial class VoiceCommandParser(IDateTimeProvider dateTimeProvider) : IVoiceCommandParser
{
    public VoiceCommandResult Parse(string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return new VoiceCommandResult(
                VoiceCommandIntent.Unknown, null, null, null, null, transcript,
                "認識できませんでした");
        }

        var normalized = NormalizeText(transcript);
        var today = dateTimeProvider.TodayDateOnly;

        var intent = DetectIntent(normalized);
        var date = JapaneseDateParser.ParseDate(normalized, today);
        var startMin = JapaneseDateParser.ParseTime(normalized);
        var patientName = ExtractPatientName(normalized);
        var filterParams = intent == VoiceCommandIntent.SetFilter
            ? ParseFilterParams(normalized)
            : null;

        var summary = BuildSummary(intent, date, startMin, patientName);

        return new VoiceCommandResult(intent, date, startMin, patientName, null, transcript, summary, filterParams);
    }

    private static VoiceCommandIntent DetectIntent(string text)
    {
        // キャンセル系
        if (CancelRegex().IsMatch(text))
            return VoiceCommandIntent.CancelAppointment;

        // 仮予約系（通常予約より前にチェック）
        if (TentativeRegex().IsMatch(text))
            return VoiceCommandIntent.TentativeAppointment;

        // フィルター系（検索より前にチェック）
        if (FilterRegex().IsMatch(text))
            return VoiceCommandIntent.SetFilter;

        // 検索・確認系
        if (SearchRegex().IsMatch(text))
            return VoiceCommandIntent.SearchAppointment;

        // 予約作成系
        if (CreateRegex().IsMatch(text))
            return VoiceCommandIntent.CreateAppointment;

        return VoiceCommandIntent.Unknown;
    }

    private static string? ExtractPatientName(string text)
    {
        // 「田中さん」「田中様」「田中氏」パターン
        var match = PatientNameWithSuffixRegex().Match(text);
        if (match.Success)
        {
            var name = match.Groups["name"].Value;
            // 助詞・時刻関連の文字を先頭から除去
            name = TrimNonNamePrefix(name);
            if (name.Length > 0)
                return name;
        }

        // 「〇〇の予約」パターン
        var ofMatch = PatientNameOfRegex().Match(text);
        if (ofMatch.Success)
        {
            var candidate = ofMatch.Groups["name"].Value;
            candidate = TrimNonNamePrefix(candidate);
            if (candidate.Length > 0 && !IsDateExpression(candidate))
                return candidate;
        }

        return null;
    }

    private static string TrimNonNamePrefix(string name)
    {
        // 先頭の助詞・時刻関連文字を除去
        var prefixChars = "のにをはがでとへもから時分半午前後年月日曜週間今明";
        var start = 0;
        while (start < name.Length && prefixChars.Contains(name[start]))
            start++;
        return name[start..];
    }

    private static bool IsDateExpression(string text) =>
        DateExpressionRegex().IsMatch(text) ||
        text is "今日" or "明日" or "明後日" or "昨日" or "一昨日" or "来月" or "先月"
            or "来月末" or "先月末" or "月末";

    private static string NormalizeText(string text)
    {
        var s = text.Trim()
            .Replace("　", " ")
            .Replace("\u3000", " ");

        // 句読点・記号を除去（音声認識が付与する場合がある）
        s = PunctuationRegex().Replace(s, "");

        // 口語表現→標準形に変換
        s = NormalizeColloquial(s);

        // 漢数字→アラビア数字（単純な1桁置換 + 複合パターン）
        s = ConvertKanjiNumbers(s);

        // 口語マーカーを復元（漢数字変換で壊されないよう一時退避していた）
        s = s.Replace("OTOTUI", "一昨日");
        s = s.Replace("SHASATTE", "三日後");

        // 「時」省略の補完（漢数字→数字変換後に実行）
        // 「午後1」「午前9」のように数字の後に「時」がないパターンを補完
        s = AmPmHourOmissionRegex().Replace(s, "${prefix}${hour}時");

        return s;
    }

    /// <summary>
    /// 口語・日常会話表現を標準形に変換する。
    /// 音声認識結果のひらがな表記にも対応。
    /// </summary>
    private static string NormalizeColloquial(string text)
    {
        // --- 日付の口語表現 ---
        // 長い表現から先に置換（部分一致を防ぐ）
        text = text.Replace("しあさって", "SHASATTE");
        text = text.Replace("おとつい", "OTOTUI");
        text = text.Replace("おととい", "OTOTUI");
        text = text.Replace("あさって", "明後日");
        text = text.Replace("きのう", "昨日");
        text = text.Replace("きょう", "今日");
        text = text.Replace("あした", "明日");

        // --- 日の口語表現（和語の日数え） ---
        // 「来月のついたち」→「来月の1日」
        text = text.Replace("ついたち", "1日");
        text = text.Replace("ふつか", "2日");
        text = text.Replace("みっか", "3日");
        text = text.Replace("よっか", "4日");
        text = text.Replace("いつか", "5日");
        text = text.Replace("むいか", "6日");
        text = text.Replace("なのか", "7日");
        text = text.Replace("ようか", "8日");
        text = text.Replace("ここのか", "9日");
        text = text.Replace("とおか", "10日");
        text = text.Replace("はつか", "20日");

        // --- 時刻の口語表現 ---
        // 午後いち / 午前いち → 午後1時 / 午前1時（「いち」は常に安全）
        text = text.Replace("午後いち", "午後1時");
        text = text.Replace("午前いち", "午前1時");

        // 午前/午後直後のひらがな数詞（一般語と衝突しうるため午前/午後限定）
        // ※「に」は助詞と衝突するため除外（「午後に予約」は「午後2時」ではない）
        text = ColloquialAmPmSanRegex().Replace(text, "${prefix}3時");
        text = ColloquialAmPmYonRegex().Replace(text, "${prefix}4時");
        text = ColloquialAmPmGoRegex().Replace(text, "${prefix}5時");

        // 朝いち / 朝一 → 9時（業務開始）
        text = text.Replace("朝いち", "9時");
        text = text.Replace("朝一", "9時");
        // お昼 / 昼 → 12時（時刻コンテキスト）
        text = ColloquialNoonRegex().Replace(text, "12時");

        return text;
    }

    [GeneratedRegex(@"(?:お昼|(?<![一昨])昼)(?!食|ご飯|休)")]
    private static partial Regex ColloquialNoonRegex();

    // 午前/午後直後のひらがな数詞（一般語と衝突するため午前/午後限定）
    // ※「に」は助詞（「午後に予約」）と区別できないため対象外
    [GeneratedRegex(@"(?<prefix>午前|午後)さん")]
    private static partial Regex ColloquialAmPmSanRegex();

    [GeneratedRegex(@"(?<prefix>午前|午後)(?:よん|し(?!時))")]
    private static partial Regex ColloquialAmPmYonRegex();

    [GeneratedRegex(@"(?<prefix>午前|午後)ご(?!時)")]
    private static partial Regex ColloquialAmPmGoRegex();

    // 「午後1」「午前9」→「午後1時」「午前9時」（「時」省略補完）
    [GeneratedRegex(@"(?<prefix>午前|午後)(?<hour>\d{1,2})(?!時|分|:)")]
    private static partial Regex AmPmHourOmissionRegex();

    /// <summary>
    /// 漢数字をアラビア数字に変換する。
    /// 「十時」→「10時」、「三月五日」→「3月5日」等。
    /// </summary>
    private static string ConvertKanjiNumbers(string text)
    {
        // 複合漢数字: 十一→11, 二十→20, 二十三→23 等
        text = CompoundKanjiTensRegex().Replace(text, m =>
        {
            var tens = KanjiToDigit(m.Groups["tens"].Value);
            var ones = m.Groups["ones"].Success ? KanjiToDigit(m.Groups["ones"].Value) : 0;
            return (tens * 10 + ones).ToString();
        });

        // 「十」単体 → 10
        text = text.Replace("十", "10");

        // 残った単純漢数字: 一→1, 二→2, ...
        text = SingleKanjiDigitRegex().Replace(text, m => KanjiToDigit(m.Value).ToString());

        return text;
    }

    private static int KanjiToDigit(string kanji) => kanji switch
    {
        "〇" or "零" => 0, "一" => 1, "二" => 2, "三" => 3, "四" => 4,
        "五" => 5, "六" => 6, "七" => 7, "八" => 8, "九" => 9,
        _ => 0,
    };

    [GeneratedRegex(@"[、。，．・！？\s]+")]
    private static partial Regex PunctuationRegex();

    [GeneratedRegex(@"(?<tens>[一二三四五六七八九])十(?<ones>[一二三四五六七八九])?")]
    private static partial Regex CompoundKanjiTensRegex();

    [GeneratedRegex(@"[一二三四五六七八九〇零]")]
    private static partial Regex SingleKanjiDigitRegex();

    private static string BuildSummary(
        VoiceCommandIntent intent, DateOnly? date, int? startMin, string? patientName)
    {
        var parts = new List<string>();

        if (date.HasValue)
            parts.Add(date.Value.ToString("M月d日"));

        if (startMin.HasValue)
        {
            var h = startMin.Value / 60;
            var m = startMin.Value % 60;
            parts.Add($"{h}:{m:D2}");
        }

        if (patientName is not null)
            parts.Add($"{patientName}さん");

        var intentLabel = intent switch
        {
            VoiceCommandIntent.CreateAppointment => "予約作成",
            VoiceCommandIntent.TentativeAppointment => "仮予約作成",
            VoiceCommandIntent.CancelAppointment => "予約キャンセル",
            VoiceCommandIntent.SearchAppointment => "予約検索",
            VoiceCommandIntent.SetFilter => "フィルター設定",
            _ => "不明なコマンド",
        };

        parts.Add(intentLabel);

        return string.Join(" ", parts);
    }

    private static VoiceCommandFilterParams ParseFilterParams(string text)
    {
        if (FilterClearRegex().IsMatch(text) || text.Contains("全部表示"))
            return new VoiceCommandFilterParams(null, null, true, null);

        var days = DayOfWeekFilterRegex().Matches(text)
            .Select(m => m.Groups["day"].Value switch
            {
                "日" => 0, "月" => 1, "火" => 2, "水" => 3,
                "木" => 4, "金" => 5, "土" => 6, _ => -1
            })
            .Where(d => d >= 0)
            .Distinct().ToList();

        List<string>? timeSlots = null;
        if (text.Contains("午前のみ") || text.Contains("午前だけ") || text.Contains("午前"))
            timeSlots = ["09:00-12:00"];
        else if (text.Contains("午後のみ") || text.Contains("午後だけ") || text.Contains("午後"))
            timeSlots = ["13:00-17:00"];

        int? capacity = null;
        var capMatch = RequiredCapacityRegex().Match(text);
        if (capMatch.Success) capacity = int.Parse(capMatch.Groups["n"].Value);

        return new VoiceCommandFilterParams(
            days.Count > 0 ? days : null,
            timeSlots,
            false,
            capacity);
    }

    // --- Intent detection patterns ---

    [GeneratedRegex(@"(キャンセル|取り消し|取消|削除)")]
    private static partial Regex CancelRegex();

    [GeneratedRegex(@"(仮予約|仮で|とりあえず)")]
    private static partial Regex TentativeRegex();

    [GeneratedRegex(@"(フィルター|絞り込み|午前のみ|午後のみ|午前だけ|午後だけ|全部表示|[月火水木金土日]曜だけ|[月火水木金土日]曜のみ|空きが?\d+[つ件個席]以上)")]
    private static partial Regex FilterRegex();

    [GeneratedRegex(@"(フィルタークリア|フィルターリセット|全部表示)")]
    private static partial Regex FilterClearRegex();

    [GeneratedRegex(@"(?<day>[月火水木金土日])曜")]
    private static partial Regex DayOfWeekFilterRegex();

    [GeneratedRegex(@"空きが?(?<n>\d+)(つ|件|個|席)以上")]
    private static partial Regex RequiredCapacityRegex();

    [GeneratedRegex(@"(確認|検索|調べ|見せ|表示|一覧|空き|開い)")]
    private static partial Regex SearchRegex();

    [GeneratedRegex(@"(予約|入れ|追加|登録|作成|取っ)")]
    private static partial Regex CreateRegex();

    // --- Patient name patterns ---

    [GeneratedRegex(@"(?<name>[\p{IsCJKUnifiedIdeographs}\p{IsKatakana}]{1,10})(さん|様|氏)")]
    private static partial Regex PatientNameWithSuffixRegex();

    [GeneratedRegex(@"(?<name>[\p{IsCJKUnifiedIdeographs}\p{IsHiragana}\p{IsKatakana}]{2,10})の予約")]
    private static partial Regex PatientNameOfRegex();

    [GeneratedRegex(@"^\d{1,2}月$|^\d{4}年|曜日?$|来週|今週|再来週")]
    private static partial Regex DateExpressionRegex();
}
