using Aloe.Apps.MedockLib.Services.Dtos.Appointments;

namespace Aloe.Apps.MedockServer.Components.Calendar;

/// <summary>
/// 予約フィルタリング用のヘルパークラス
/// </summary>
public static class AppointmentFilterHelper
{
    /// <summary>
    /// 指定された日付と時間の予約を取得します
    /// </summary>
    /// <param name="appointments">予約リスト</param>
    /// <param name="date">日付</param>
    /// <param name="hour">時間（0-23）</param>
    /// <returns>該当する時間帯の予約リスト（名前、組織名、ステータス）</returns>
    public static IEnumerable<(string Name, string Org, int Status)> GetHourAppointments(
        IEnumerable<AppointmentDto>? appointments,
        DateOnly date,
        int hour)
    {
        if (appointments == null) yield break;

        var hourStartMinutes = hour * 60;
        var hourEndMinutes = (hour + 1) * 60;

        foreach (var appt in appointments)
        {
            if (appt.Date != date) continue;

            // 予約の開始時間がこの時間帯に含まれるかチェック
            if (appt.StartMin >= hourStartMinutes && appt.StartMin < hourEndMinutes)
            {
                yield return (
                    appt.PatientName ?? "未設定",
                    appt.OrganizationName ?? "",
                    appt.Status
                );
            }
        }
    }
}
