namespace Aloe.Apps.MedockLib.Services.Dtos;

/// <summary>
/// 施設営業時間DTO
/// </summary>
public class BusinessHoursDto
{
    /// <summary>始業時間（例: "09:00"）</summary>
    public string StartTime { get; set; } = "09:00";

    /// <summary>就業時間（例: "18:00"）</summary>
    public string EndTime { get; set; } = "18:00";

    /// <summary>昼休み開始時間（例: "12:00"）</summary>
    public string LunchStartTime { get; set; } = "12:00";

    /// <summary>昼休み終了時間（例: "13:00"）</summary>
    public string LunchEndTime { get; set; } = "13:00";

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
}

