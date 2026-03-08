using Microsoft.AspNetCore.SignalR;

namespace Aloe.Apps.MedockServer.Hubs;

/// <summary>
/// 予約統計更新通知用のSignalR Hub
/// </summary>
public class AppointmentStatsHub : Hub<IAppointmentStatsHubClient>
{
    /// <summary>
    /// カレンダーグループに参加
    /// </summary>
    /// <param name="calendarId">カレンダーID（通常はFacilityIdなど）</param>
    public async Task JoinCalendarGroup(string calendarId)
    {
        await this.Groups.AddToGroupAsync(this.Context.ConnectionId, calendarId);
    }

    /// <summary>
    /// カレンダーグループから退出
    /// </summary>
    /// <param name="calendarId">カレンダーID</param>
    public async Task LeaveCalendarGroup(string calendarId)
    {
        await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, calendarId);
    }
}

/// <summary>
/// クライアント側のメソッド定義インターフェース
/// </summary>
public interface IAppointmentStatsHubClient
{
    /// <summary>
    /// 統計データが更新されたことを通知
    /// </summary>
    /// <param name="updatedDate">更新された日付</param>
    /// <param name="updatedResourceIds">更新されたリソースIDのリスト</param>
    Task OnStatsUpdated(string updatedDate, List<string> updatedResourceIds);
}

