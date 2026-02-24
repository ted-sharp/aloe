namespace Aloe.Apps.MedockLib.Services;

using Aloe.Apps.MedockLib.Data;

/// <summary>
/// 予約統計の自動再計算サービスインターフェース
/// 予約の作成・更新・削除時にStats/StatSlotsを再計算する
/// </summary>
public interface IAppointmentStatsUpdateService
{
    /// <summary>
    /// 指定した日付・リソースの統計を再計算します
    /// 影響を受けるStats/StatSlotsをソフト削除後、新しい値を挿入します
    /// </summary>
    /// <param name="context">トランザクションに参加するDbContext（SaveChangesは呼び出し側で実行）</param>
    /// <param name="affectedDates">再計算対象の日付リスト</param>
    /// <param name="affectedResourceIds">再計算対象のリソースIDリスト</param>
    Task RecalculateStatsAsync(
        MedockDbContext context,
        List<DateOnly> affectedDates,
        List<Guid> affectedResourceIds);
}
