using Aloe.Apps.MedockServer.Services;

namespace Aloe.Apps.MedockServer.Services;

/// <summary>
/// AppointmentStats更新通知のヘルパークラス
/// Seederやバッチ処理から通知を送信する際に使用
/// </summary>
public static class AppointmentStatsNotificationHelper
{
    /// <summary>
    /// 日付とリソースIDのリストから通知を送信
    /// </summary>
    /// <param name="notificationService">通知サービス</param>
    /// <param name="updatedDates">更新された日付のリスト</param>
    /// <param name="updatedResourceIds">更新されたリソースIDのリスト</param>
    public static async Task NotifyStatsUpdatedAsync(
        IAppointmentStatsNotificationService? notificationService,
        IEnumerable<DateOnly> updatedDates,
        IEnumerable<Guid> updatedResourceIds)
    {
        if (notificationService == null)
        {
            // 通知サービスが利用できない場合（Seeder実行時など）はスキップ
            return;
        }

        var resourceIdList = updatedResourceIds.ToList();
        foreach (var date in updatedDates)
        {
            await notificationService.NotifyStatsUpdatedAsync(date, resourceIdList);
        }
    }

    /// <summary>
    /// 単一の日付とリソースIDから通知を送信
    /// </summary>
    public static async Task NotifyStatsUpdatedAsync(
        IAppointmentStatsNotificationService? notificationService,
        DateOnly updatedDate,
        Guid updatedResourceId)
    {
        await NotifyStatsUpdatedAsync(notificationService, new[] { updatedDate }, new[] { updatedResourceId });
    }
}

