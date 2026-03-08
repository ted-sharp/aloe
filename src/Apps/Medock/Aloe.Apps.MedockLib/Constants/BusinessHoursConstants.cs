namespace Aloe.Apps.MedockLib.Constants;

/// <summary>
/// 営業時間関連の定数（分数ベース）
/// </summary>
/// <remarks>
/// TimeConstants の分数ベースの定数を参照しています。
/// 文字列形式が必要な場合は、プロパティまたは TimeConstants.MinutesToTimeString メソッドを使用してください。
/// </remarks>
public static class BusinessHoursConstants
{
    /// <summary>
    /// デフォルトの始業時間（09:00 = 540分）
    /// </summary>
    public const int DefaultStartMin = TimeConstants.WorkStartMin;

    /// <summary>
    /// デフォルトの就業時間（17:00 = 1020分）
    /// </summary>
    public const int DefaultEndMin = TimeConstants.WorkEndMin;

    /// <summary>
    /// デフォルトの昼休み開始時間（12:00 = 720分）
    /// </summary>
    public const int DefaultLunchStartMin = TimeConstants.LunchStartMin;

    /// <summary>
    /// デフォルトの昼休み終了時間（13:00 = 780分）
    /// </summary>
    public const int DefaultLunchEndMin = TimeConstants.LunchEndMin;

    /// <summary>
    /// デフォルトの予約開始時間（09:00 = 540分）
    /// </summary>
    public const int DefaultAppointmentStartMin = TimeConstants.WorkStartMin;

    /// <summary>
    /// デフォルトの予約時間（分）
    /// </summary>
    public const int DefaultAppointmentDurationMin = 60;

    /// <summary>
    /// デフォルトの予約終了時間（10:00 = DefaultAppointmentStartMin + DefaultAppointmentDurationMin）
    /// </summary>
    public const int DefaultAppointmentEndMin = DefaultAppointmentStartMin + DefaultAppointmentDurationMin;

    /// <summary>
    /// デフォルトの始業時間（"09:00"）を取得
    /// </summary>
    public static string DefaultStartTime => TimeConstants.MinutesToTimeString(DefaultStartMin);

    /// <summary>
    /// デフォルトの就業時間（"17:00"）を取得
    /// </summary>
    public static string DefaultEndTime => TimeConstants.MinutesToTimeString(DefaultEndMin);

    /// <summary>
    /// デフォルトの昼休み開始時間（"12:00"）を取得
    /// </summary>
    public static string DefaultLunchStartTime => TimeConstants.MinutesToTimeString(DefaultLunchStartMin);

    /// <summary>
    /// デフォルトの昼休み終了時間（"13:00"）を取得
    /// </summary>
    public static string DefaultLunchEndTime => TimeConstants.MinutesToTimeString(DefaultLunchEndMin);

    /// <summary>
    /// デフォルトの予約開始時間（"09:00"）を取得
    /// </summary>
    public static string DefaultAppointmentStartTime => TimeConstants.MinutesToTimeString(DefaultAppointmentStartMin);

    /// <summary>
    /// デフォルトの予約終了時間（"10:00"）を取得
    /// </summary>
    public static string DefaultAppointmentEndTime => TimeConstants.MinutesToTimeString(DefaultAppointmentEndMin);
}

