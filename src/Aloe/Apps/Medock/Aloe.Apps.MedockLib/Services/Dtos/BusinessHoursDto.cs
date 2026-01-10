using Aloe.Apps.MedockLib.Constants;

namespace Aloe.Apps.MedockLib.Services.Dtos;

// TODO: テーブル定義を分解しておきたい
/// <summary>
/// 施設営業時間DTO
/// </summary>
public class BusinessHoursDto
{
    /// <summary>始業時間（例: "09:00"）</summary>
    public string StartTime { get; set; } = BusinessHoursConstants.DefaultStartTime;

    /// <summary>就業時間（例: "18:00"）</summary>
    public string EndTime { get; set; } = BusinessHoursConstants.DefaultEndTime;

    /// <summary>昼休み開始時間（例: "12:00"）</summary>
    public string LunchStartTime { get; set; } = BusinessHoursConstants.DefaultLunchStartTime;

    /// <summary>昼休み終了時間（例: "13:00"）</summary>
    public string LunchEndTime { get; set; } = BusinessHoursConstants.DefaultLunchEndTime;

    /// <summary>
    /// 始業時間をTimeOnlyに変換
    /// </summary>
    public TimeOnly GetStartTimeOnly() => TimeOnly.Parse(this.StartTime);

    /// <summary>
    /// 就業時間をTimeOnlyに変換
    /// </summary>
    public TimeOnly GetEndTimeOnly() => TimeOnly.Parse(this.EndTime);

    /// <summary>
    /// 昼休み開始時間をTimeOnlyに変換
    /// </summary>
    public TimeOnly GetLunchStartTimeOnly() => TimeOnly.Parse(this.LunchStartTime);

    /// <summary>
    /// 昼休み終了時間をTimeOnlyに変換
    /// </summary>
    public TimeOnly GetLunchEndTimeOnly() => TimeOnly.Parse(this.LunchEndTime);

    /// <summary>
    /// 始業時間の時（Hour）を取得
    /// </summary>
    public int StartHour => this.GetStartTimeOnly().Hour;

    /// <summary>
    /// 就業時間の時（Hour）を取得
    /// </summary>
    public int EndHour => this.GetEndTimeOnly().Hour;

    /// <summary>
    /// 昼休み開始時間の時（Hour）を取得
    /// </summary>
    public int LunchStartHour => this.GetLunchStartTimeOnly().Hour;

    /// <summary>
    /// 昼休み終了時間の時（Hour）を取得
    /// </summary>
    public int LunchEndHour => this.GetLunchEndTimeOnly().Hour;

    /// <summary>
    /// スロットが業務時間外かどうかを判定します（分単位）
    /// </summary>
    /// <param name="slotStartMinutes">スロット開始時刻（分単位）</param>
    /// <param name="slotEndMinutes">スロット終了時刻（分単位）</param>
    /// <returns>業務時間外判定結果（Before/After/Lunchのフラグ）</returns>
    public (bool IsOutsideHoursBefore, bool IsOutsideHoursAfter, bool IsOutsideHoursLunch) CheckSlotOutsideHours(int slotStartMinutes, int slotEndMinutes)
    {
        var businessStart = this.GetStartTimeOnly();
        var businessEnd = this.GetEndTimeOnly();
        var slotStartTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(slotStartMinutes));
        var slotEndTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(slotEndMinutes));

        var isOutsideHoursBefore = false;
        var isOutsideHoursAfter = false;
        var isOutsideHoursLunch = false;

        // 朝の時間外: スロットが業務開始時刻より前に終わる
        if (slotEndTime <= businessStart)
        {
            isOutsideHoursBefore = true;
        }
        // 夕方の時間外: スロットが業務終了時刻以降に開始する、または業務終了時刻以降で終わる
        else if (slotStartTime >= businessEnd || slotEndTime > businessEnd)
        {
            isOutsideHoursAfter = true;
        }
        // 昼休み時間帯にある場合
        else if (!String.IsNullOrEmpty(this.LunchStartTime) && !String.IsNullOrEmpty(this.LunchEndTime))
        {
            var lunchStart = this.GetLunchStartTimeOnly();
            var lunchEnd = this.GetLunchEndTimeOnly();

            if (slotStartTime >= lunchStart && slotEndTime <= lunchEnd)
            {
                isOutsideHoursLunch = true;
            }
        }

        return (isOutsideHoursBefore, isOutsideHoursAfter, isOutsideHoursLunch);
    }
}

