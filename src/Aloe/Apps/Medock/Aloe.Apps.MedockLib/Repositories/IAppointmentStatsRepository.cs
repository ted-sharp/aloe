namespace Aloe.Apps.MedockLib.Repositories;

/// <summary>
/// 予約統計リポジトリインターフェース
/// </summary>
public interface IAppointmentStatsRepository
{
    /// <summary>
    /// 指定日の予約件数を取得します。
    /// </summary>
    Task<int> GetCountByDateAsync(DateOnly date);

    /// <summary>
    /// 指定フロア・日付のステータス別予約件数を取得します。
    /// </summary>
    Task<Dictionary<int, int>> GetStatusCountByFloorAndDateAsync(Guid floorId, DateOnly date);

    /// <summary>
    /// Mainリソース統計用の予約データ（日付と開始時刻（分単位）のみ）を取得します。
    /// </summary>
    Task<List<(DateOnly? ApptDate, int ApptStartMin)>> GetForMainStatsAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// 指定日付範囲のMainリソース（AppointmentResourceType.Main）のAppointmentStatsを取得します。
    /// </summary>
    Task<List<Data.Entities.AppointmentStats>> GetMainResourceStatsByDateRangeAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// 指定日付範囲とフィルター条件でMainリソース（AppointmentResourceType.Main）のAppointmentStatsを取得します。
    /// </summary>
    Task<List<Data.Entities.AppointmentStats>> GetMainResourceStatsByDateRangeWithFiltersAsync(
        DateOnly startDate,
        DateOnly endDate,
        List<Guid>? floorIds = null,
        List<Guid>? resourceGroupIds = null,
        List<Guid>? resourceIds = null,
        List<Guid>? planIds = null,
        List<Guid>? optionPlanIds = null);

    /// <summary>
    /// 指定日付とリソースIDでMainリソース（AppointmentResourceType.Main）のAppointmentStatsを取得します（差分取得用）。
    /// </summary>
    Task<List<Data.Entities.AppointmentStats>> GetMainResourceStatsByDateAndResourcesAsync(
        DateOnly date,
        List<Guid> resourceIds);

    /// <summary>
    /// 指定日付範囲とEquipmentリソースIDリストでEquipmentリソース（AppointmentResourceType.Equipment）のAppointmentStatsを取得します。
    /// </summary>
    Task<List<Data.Entities.AppointmentStats>> GetEquipmentResourceStatsByDateRangeAsync(
        DateOnly startDate,
        DateOnly endDate,
        List<Guid> equipmentResourceIds);

    /// <summary>
    /// 指定日付範囲のEquipmentリソースのスロット情報を配列として最適化されたSQLで取得します。
    /// パフォーマンス最適化用：array_agg() で SQL側で配列化。
    /// 日付ごとにグループ化された辞書形式で返します。
    /// equipmentResourceIds が null または空の場合は全 Equipment リソースを取得します。
    /// </summary>
    Task<Dictionary<string, List<Services.Dtos.ResourceStatSlotsDto>>> GetEquipmentResourceSlotsAsArraysByDateAsync(
        DateOnly startDate,
        DateOnly endDate,
        List<Guid>? equipmentResourceIds);

    /// <summary>
    /// 指定された日付範囲のStatスロットを取得します。
    /// AppointmentStatsの削除された navigation property に代わるメソッド
    /// </summary>
    Task<Dictionary<(DateOnly ApptDate, Guid ApptResId), List<Data.Entities.AppointmentStatSlots>>> GetStatSlotsByDateRangeAsync(
        DateOnly startDate,
        DateOnly endDate,
        List<Guid>? resourceIds = null);
}
