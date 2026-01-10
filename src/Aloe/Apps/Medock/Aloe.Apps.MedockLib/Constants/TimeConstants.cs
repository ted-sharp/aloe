namespace Aloe.Apps.MedockLib.Constants;

/// <summary>
/// 時間関連の定数（分数ベース、00:00からの経過分数）
/// </summary>
/// <remarks>
/// すべての時間値は真夜中（00:00）からの経過分数で定義されています。
/// 例：09:00 = 540分、12:00 = 720分、17:00 = 1020分
/// </remarks>
public static class TimeConstants
{
    // ========================================
    // 業務時間（標準営業時間）
    // ========================================

    /// <summary>
    /// 標準営業開始時間（09:00 = 540分）
    /// </summary>
    public const int WorkStartMin = 540;

    /// <summary>
    /// 標準営業終了時間（17:00 = 1020分）
    /// </summary>
    public const int WorkEndMin = 1020;

    /// <summary>
    /// 標準昼休み開始時間（12:00 = 720分）
    /// </summary>
    public const int LunchStartMin = 720;

    /// <summary>
    /// 標準昼休み終了時間（13:00 = 780分）
    /// </summary>
    public const int LunchEndMin = 780;

    /// <summary>
    /// 昼休み時間（分）
    /// </summary>
    public const int LunchDurationMin = LunchEndMin - LunchStartMin; // 60分

    // ========================================
    // ピーク時間帯
    // ========================================

    /// <summary>
    /// 朝のピーク時間帯開始（09:00 = 540分）
    /// </summary>
    public const int MorningPeakStartMin = 540;

    /// <summary>
    /// 朝のピーク時間帯終了（11:00 = 660分）
    /// </summary>
    public const int MorningPeakEndMin = 660;

    /// <summary>
    /// 午後の営業開始（13:00 = 780分）
    /// </summary>
    public const int AfternoonStartMin = 780;

    // ========================================
    // 外来営業時間帯（営業外の予約）
    // ========================================

    /// <summary>
    /// 早朝営業開始（07:00 = 420分）
    /// </summary>
    public const int EarlyMorningStartMin = 420;

    /// <summary>
    /// 夜間営業終了（18:00 = 1080分）
    /// </summary>
    public const int EveningEndMin = 1080;

    // ========================================
    // 土曜日営業時間
    // ========================================

    /// <summary>
    /// 土曜日営業終了時間（12:30 = 750分）
    /// </summary>
    public const int SaturdayEndMin = 750;

    // ========================================
    // ユーティリティメソッド
    // ========================================

    /// <summary>
    /// 分数を "HH:mm" 形式の文字列に変換します。
    /// </summary>
    /// <param name="minutes">真夜中からの経過分数</param>
    /// <returns>"HH:mm" 形式の文字列</returns>
    public static string MinutesToTimeString(int minutes)
    {
        var hours = minutes / 60;
        var mins = minutes % 60;
        return $"{hours:D2}:{mins:D2}";
    }

    /// <summary>
    /// "HH:mm" または "H:mm" 形式の文字列を分数に変換します。
    /// </summary>
    /// <param name="timeString">"HH:mm" または "H:mm" 形式の時間文字列</param>
    /// <returns>真夜中からの経過分数</returns>
    /// <exception cref="FormatException">形式が正しくない場合</exception>
    public static int TimeStringToMinutes(string timeString)
    {
        var parts = timeString.Split(':');
        if (parts.Length != 2 || !Int32.TryParse(parts[0], out var hours) || !Int32.TryParse(parts[1], out var minutes))
        {
            throw new FormatException($"Invalid time format: {timeString}. Expected 'HH:mm' or 'H:mm'.");
        }

        return hours * 60 + minutes;
    }

    /// <summary>
    /// "HH:mm" または "H:mm" 形式の文字列を分数に変換します（null許可版）
    /// 変換できない場合はnullを返します
    /// </summary>
    /// <param name="timeString">"HH:mm" または "H:mm" 形式の時間文字列</param>
    /// <returns>真夜中からの経過分数、変換できない場合はnull</returns>
    public static int? TryTimeStringToMinutes(string? timeString)
    {
        if (String.IsNullOrEmpty(timeString)) return null;
        try
        {
            return TimeStringToMinutes(timeString);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
