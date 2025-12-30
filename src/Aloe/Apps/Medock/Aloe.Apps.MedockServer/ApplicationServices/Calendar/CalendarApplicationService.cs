using Aloe.Apps.MedockServer.ApplicationServices.Calendar.DataLoaders;
using Aloe.Apps.MedockServer.Components.Pages;

namespace Aloe.Apps.MedockServer.ApplicationServices.Calendar;

/// <summary>
/// カレンダーページのデータロード処理を統合するアプリケーションサービス（Facade）
/// 複数のLoaderサービスを調整し、Calendar.razor.csに対して統一されたインターフェースを提供します。
/// </summary>
public class CalendarApplicationService
{
    private readonly IStatsLoader _statsLoader;
    private readonly IAppointmentLoader _appointmentLoader;
    private readonly IMetadataLoader _metadataLoader;

    public CalendarApplicationService(
        IStatsLoader statsLoader,
        IAppointmentLoader appointmentLoader,
        IMetadataLoader metadataLoader)
    {
        this._statsLoader = statsLoader;
        this._appointmentLoader = appointmentLoader;
        this._metadataLoader = metadataLoader;
    }

    /// <summary>
    /// 営業時間をロードして状態に反映します。
    /// </summary>
    public Task LoadBusinessHoursAsync(CalendarState state)
        => this._appointmentLoader.LoadBusinessHoursAsync(state);

    /// <summary>
    /// 休日情報をロードして状態に反映します。
    /// </summary>
    public Task LoadHolidaysAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7)
        => this._metadataLoader.LoadHolidaysAsync(state, viewType, currentDate, weekDays);

    /// <summary>
    /// フィルター用のオプション（フロア、リソースグループ、プラン、オプション）をロードして状態に反映します。
    /// </summary>
    public Task LoadFilterOptionsAsync(CalendarState state)
        => this._metadataLoader.LoadFilterOptionsAsync(state);

    /// <summary>
    /// 期間に応じた予約データを取得して状態に反映します。
    /// </summary>
    public Task LoadAppointmentsAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7)
        => this._appointmentLoader.LoadAppointmentsAsync(state, viewType, currentDate, weekDays);

    /// <summary>
    /// Mainリソースの統計データを日付ごとにグループ化して状態に反映します。
    /// </summary>
    public Task LoadMainStatsAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7)
        => this._statsLoader.LoadMainStatsAsync(state, viewType, currentDate, weekDays);

    /// <summary>
    /// Equipmentリソースの統計データを日付ごとにグループ化して状態に反映します。
    /// フィルターで選択されたEquipmentリソースのみを取得します。
    /// </summary>
    public Task LoadEquipmentStatsAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7)
        => this._statsLoader.LoadEquipmentStatsAsync(state, viewType, currentDate, weekDays);
}
