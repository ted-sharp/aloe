using Aloe.Apps.MedockServer.Components.Pages;

namespace Aloe.Apps.MedockServer.ApplicationServices.Calendar.DataLoaders;

/// <summary>
/// 予約データおよび営業時間のロード処理を担当するサービス
/// </summary>
public interface IAppointmentLoader
{
    /// <summary>
    /// 営業時間をロードして状態に反映します。
    /// </summary>
    Task LoadBusinessHoursAsync(CalendarState state);

    /// <summary>
    /// 期間に応じた予約データを取得して状態に反映します。
    /// </summary>
    Task LoadAppointmentsAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7);
}
