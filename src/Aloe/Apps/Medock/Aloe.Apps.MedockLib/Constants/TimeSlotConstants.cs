namespace Aloe.Apps.MedockLib.Constants;

/// <summary>
/// 時間スロット関連の定数（分数ベース）
/// </summary>
/// <remarks>
/// 時間スロットは分数（int）ベースで定義されています。
/// 文字列形式が必要な場合は、FilterTimeSlotsString プロパティを使用するか、
/// TimeConstants.MinutesToTimeString メソッドを使用してください。
/// </remarks>
public static class TimeSlotConstants
{
    /// <summary>
    /// フィルター用の時間スロット（1時間刻み、分数ベース）
    /// </summary>
    public static readonly int[] FilterTimeSlotsMin = [480, 540, 600, 660, 780, 840, 900, 960]; // 08:00, 09:00, 10:00, 11:00, 13:00, 14:00, 15:00, 16:00

    /// <summary>
    /// フィルター用の時間スロット（1時間刻み、文字列形式）
    /// </summary>
    public static string[] FilterTimeSlots => FilterTimeSlotsMin
        .Select(TimeConstants.MinutesToTimeString)
        .ToArray();
}

