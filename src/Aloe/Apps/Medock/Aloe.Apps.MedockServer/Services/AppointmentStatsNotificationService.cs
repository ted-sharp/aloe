using Microsoft.AspNetCore.SignalR;
using Aloe.Apps.MedockServer.Hubs;

namespace Aloe.Apps.MedockServer.Services;

/// <summary>
/// 予約統計更新通知サービス
/// </summary>
public interface IAppointmentStatsNotificationService
{
    /// <summary>
    /// 統計データが更新されたことを通知
    /// </summary>
    /// <param name="updatedDate">更新された日付</param>
    /// <param name="updatedResourceIds">更新されたリソースIDのリスト</param>
    Task NotifyStatsUpdatedAsync(DateOnly updatedDate, List<Guid> updatedResourceIds);
}

/// <summary>
/// 予約統計更新通知サービス実装
/// </summary>
public class AppointmentStatsNotificationService : IAppointmentStatsNotificationService
{
    private readonly IHubContext<AppointmentStatsHub, IAppointmentStatsHubClient> _hubContext;
    private readonly ILogger<AppointmentStatsNotificationService> _logger;

    public AppointmentStatsNotificationService(
        IHubContext<AppointmentStatsHub, IAppointmentStatsHubClient> hubContext,
        ILogger<AppointmentStatsNotificationService> logger)
    {
        this._hubContext = hubContext;
        this._logger = logger;
    }

    /// <inheritdoc />
    public async Task NotifyStatsUpdatedAsync(DateOnly updatedDate, List<Guid> updatedResourceIds)
    {
        try
        {
            var dateStr = updatedDate.ToString("yyyy-MM-dd");
            var resourceIdStrs = updatedResourceIds.Select(id => id.ToString()).ToList();

            // 全クライアントに通知（グループ機能を使う場合は、特定のグループにのみ通知可能）
            await this._hubContext.Clients.All.OnStatsUpdated(dateStr, resourceIdStrs);

            this._logger.LogDebug(
                "AppointmentStats updated notification sent: Date={Date}, ResourceIds={ResourceIds}",
                dateStr,
                String.Join(", ", resourceIdStrs));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to send AppointmentStats update notification");
        }
    }
}

