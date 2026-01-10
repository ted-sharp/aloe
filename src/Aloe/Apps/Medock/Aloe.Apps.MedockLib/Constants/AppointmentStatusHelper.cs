namespace Aloe.Apps.MedockLib.Constants;

/// <summary>
/// 予約ステータスに関するUI表示ヘルパークラス
/// </summary>
public static class AppointmentStatusHelper
{
    /// <summary>
    /// ステータスの表示テキストを取得します
    /// </summary>
    /// <param name="status">ステータスコード</param>
    /// <returns>表示テキスト</returns>
    public static string GetStatusText(int status)
        => GetStatusText((AppointmentStatus)status);

    /// <summary>
    /// ステータスの表示テキストを取得します
    /// </summary>
    /// <param name="status">ステータス</param>
    /// <returns>表示テキスト</returns>
    public static string GetStatusText(AppointmentStatus status)
    {
        return status switch
        {
            AppointmentStatus.Reserved => "予約",
            AppointmentStatus.Waiting => "待機中",
            AppointmentStatus.Visited => "来院済み",
            AppointmentStatus.Cancelled => "キャンセル",
            _ => "不明"
        };
    }

    /// <summary>
    /// ステータス用のボーダークラスを取得します（DayView用）
    /// </summary>
    /// <param name="status">ステータスコード</param>
    /// <returns>Tailwind CSSのボーダークラス</returns>
    public static string GetBorderClass(int status)
        => GetBorderClass((AppointmentStatus)status);

    /// <summary>
    /// ステータス用のボーダークラスを取得します（DayView用）
    /// </summary>
    /// <param name="status">ステータス</param>
    /// <returns>Tailwind CSSのボーダークラス</returns>
    public static string GetBorderClass(AppointmentStatus status)
    {
        return status switch
        {
            AppointmentStatus.Reserved => "border-l-4 border-warning",
            AppointmentStatus.Waiting => "border-l-4 border-info",
            AppointmentStatus.Visited => "border-l-4 border-success",
            AppointmentStatus.Cancelled => "border-l-4 border-error",
            _ => ""
        };
    }

    /// <summary>
    /// ステータス用のバッジクラスを取得します（DayView用）
    /// </summary>
    /// <param name="status">ステータスコード</param>
    /// <returns>Tailwind CSSのバッジクラス</returns>
    public static string GetBadgeClass(int status)
        => GetBadgeClass((AppointmentStatus)status);

    /// <summary>
    /// ステータス用のバッジクラスを取得します（DayView用）
    /// </summary>
    /// <param name="status">ステータス</param>
    /// <returns>Tailwind CSSのバッジクラス</returns>
    public static string GetBadgeClass(AppointmentStatus status)
    {
        return status switch
        {
            AppointmentStatus.Reserved => "badge-warning",
            AppointmentStatus.Waiting => "badge-info",
            AppointmentStatus.Visited => "badge-success",
            AppointmentStatus.Cancelled => "badge-error",
            _ => ""
        };
    }

    /// <summary>
    /// ステータス用の背景・ボーダークラスを取得します（WeekView用）
    /// </summary>
    /// <param name="status">ステータスコード</param>
    /// <returns>Tailwind CSSの背景・ボーダークラス</returns>
    public static string GetBackgroundWithBorderClass(int status)
        => GetBackgroundWithBorderClass((AppointmentStatus)status);

    /// <summary>
    /// ステータス用の背景・ボーダークラスを取得します（WeekView用）
    /// </summary>
    /// <param name="status">ステータス</param>
    /// <returns>Tailwind CSSの背景・ボーダークラス</returns>
    public static string GetBackgroundWithBorderClass(AppointmentStatus status)
    {
        return status switch
        {
            AppointmentStatus.Reserved => "bg-warning/20 border-l-4 border-warning",
            AppointmentStatus.Waiting => "bg-info/20 border-l-4 border-info",
            AppointmentStatus.Visited => "bg-success/20 border-l-4 border-success",
            AppointmentStatus.Cancelled => "bg-error/20 border-l-4 border-error",
            _ => "bg-base-200"
        };
    }
}
