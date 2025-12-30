namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// カレンダーリソースフィルタリングサービスインターフェース
/// </summary>
public interface ICalendarResourceFilterService
{
    /// <summary>
    /// 選択されているリソースグループ/プラン/オプション/フロアに関連するリソースIDを取得します
    /// </summary>
    /// <param name="floorIds">選択されているフロアID</param>
    /// <param name="resourceGroupIds">選択されているリソースグループID</param>
    /// <param name="planIds">選択されているプランID</param>
    /// <param name="optionPlanIds">選択されているオプションプランID</param>
    /// <returns>関連するリソースIDのリスト</returns>
    Task<List<Guid>> GetRelatedResourceIdsAsync(
        IEnumerable<Guid> floorIds,
        IEnumerable<Guid> resourceGroupIds,
        IEnumerable<Guid> planIds,
        IEnumerable<Guid> optionPlanIds);
}
