namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// Appointmentエンティティの拡張メソッド
/// </summary>
public static class AppointmentExtensions
{
    /// <summary>
    /// 開始時間と継続時間から終了時間を計算
    /// </summary>
    /// <param name="appointment">予約エンティティ</param>
    /// <returns>終了時間。開始時間または継続時間がnullの場合はnull</returns>
    public static TimeOnly? CalculateEndTime(this Appointment appointment)
    {
        if (appointment.ApptStartTime.HasValue && appointment.ApptDurationMin.HasValue)
        {
            return appointment.ApptStartTime.Value.AddMinutes(appointment.ApptDurationMin.Value);
        }
        return null;
    }

    /// <summary>
    /// 開始時間と継続時間から終了時間を計算（静的メソッド版）
    /// </summary>
    /// <param name="startTime">開始時間</param>
    /// <param name="durationMinutes">継続時間（分）</param>
    /// <returns>終了時間。開始時間または継続時間がnullの場合はnull</returns>
    public static TimeOnly? CalculateEndTime(TimeOnly? startTime, int? durationMinutes)
    {
        if (startTime.HasValue && durationMinutes.HasValue)
        {
            return startTime.Value.AddMinutes(durationMinutes.Value);
        }
        return null;
    }
}

