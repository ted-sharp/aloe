using Aloe.Apps.MedockServer.Components.Pages;

namespace Aloe.Apps.MedockServer.ApplicationServices.Calendar.DataLoaders;

/// <summary>
/// 統計データ（MainStats, EquipmentStats）のロード処理を担当するサービス
/// </summary>
public interface IStatsLoader
{
    /// <summary>
    /// Mainリソースの統計データを日付ごとにグループ化して状態に反映します。
    /// </summary>
    Task LoadMainStatsAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7);

    /// <summary>
    /// Mainリソースのスロット単位統計データをロードして状態に反映します。
    /// </summary>
    Task LoadMainStatsSlotsAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7);

    /// <summary>
    /// Equipmentリソースの統計データを日付ごとにグループ化して状態に反映します。
    /// フィルターで選択されたEquipmentリソースのみを取得します。
    /// </summary>
    Task LoadEquipmentStatsAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7);
}
