using Aloe.Apps.MedockServer.Components.Pages;

namespace Aloe.Apps.MedockServer.ApplicationServices.Calendar;

/// <summary>
/// カレンダーの日付範囲を計算するヘルパークラス
/// </summary>
public static class CalendarDateRangeHelper
{
    /// <summary>
    /// ビューと日付に基づいて取得期間を計算します。
    /// </summary>
    /// <param name="viewType">カレンダーの表示タイプ</param>
    /// <param name="currentDate">現在の日付</param>
    /// <param name="weekDays">週表示の場合の日数（デフォルト: 7）</param>
    /// <returns>開始日と終了日のタプル</returns>
    public static (DateOnly StartDate, DateOnly EndDate) GetDateRange(
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7)
    {
        return viewType switch
        {
            CalendarViewType.Year => (
                new DateOnly(currentDate.Year, 1, 1),
                new DateOnly(currentDate.Year, 12, 31)
            ),
            CalendarViewType.Month => (
                new DateOnly(currentDate.Year, currentDate.Month, 1),
                new DateOnly(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month))
            ),
            CalendarViewType.Week => (
                currentDate,
                currentDate.AddDays(weekDays - 1)
            ),
            _ => (currentDate, currentDate)
        };
    }
}
