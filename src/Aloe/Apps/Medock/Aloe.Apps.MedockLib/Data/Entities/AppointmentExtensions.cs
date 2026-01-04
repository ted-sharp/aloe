namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// Appointmentエンティティの拡張メソッド
/// </summary>
public static class AppointmentExtensions
{
    /// <summary>
    /// 分単位から TimeOnly に変換
    /// </summary>
    /// <param name="minutes">分単位（0-1439）</param>
    /// <returns>TimeOnly。無効な値の場合はnull</returns>
    public static TimeOnly? MinutesToTimeOnly(int? minutes)
    {
        if (!minutes.HasValue || minutes.Value < 0 || minutes.Value >= 1440)
            return null;
        return TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(minutes.Value));
    }

    /// <summary>
    /// TimeOnly から分単位に変換
    /// </summary>
    /// <param name="time">時刻</param>
    /// <returns>分単位（0-1439）</returns>
    public static int TimeOnlyToMinutes(TimeOnly time)
    {
        return time.Hour * 60 + time.Minute;
    }
}

