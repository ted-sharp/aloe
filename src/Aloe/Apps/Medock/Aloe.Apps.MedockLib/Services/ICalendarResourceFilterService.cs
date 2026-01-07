namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// カレンダーリソースフィルタリングサービスインターフェース
/// </summary>
public interface ICalendarResourceFilterService
{
    /// <summary>
    /// 選択されているフロア/プランに関連するリソースIDを取得します
    /// </summary>
    /// <param name="floorIds">選択されているフロアID</param>
    /// <param name="planIds">選択されているプランID</param>
    /// <returns>関連するリソースIDのリスト</returns>
    Task<List<Guid>> GetRelatedResourceIdsAsync(
        IEnumerable<Guid> floorIds,
        IEnumerable<Guid> planIds);
}
