using Aloe.Apps.MedockServer.Components.Pages;

namespace Aloe.Apps.MedockServer.ApplicationServices.Calendar.DataLoaders;

/// <summary>
/// カレンダーメタデータ（休日、フィルターオプション）のロード処理を担当するサービス
/// </summary>
public interface IMetadataLoader
{
    /// <summary>
    /// 休日情報をロードして状態に反映します。
    /// </summary>
    Task LoadHolidaysAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7);

    /// <summary>
    /// フィルター用のオプション（フロア、リソースグループ、プラン、オプション）をロードして状態に反映します。
    /// </summary>
    Task LoadFilterOptionsAsync(CalendarState state);
}
