using System.Text.Json.Serialization;
using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Data.Entities;

namespace Aloe.Apps.MedockLib.Services.Dtos;

/// <summary>
/// 施設営業時間DTO
/// 内部的には int（分単位）で処理、JSON シリアライズ時のみ文字列に変換
/// </summary>
public class BusinessHoursDto
{
    /// <summary>始業時間（分単位、例: 540 = 09:00）</summary>
    [JsonIgnore]
    public int WorkStartMin { get; set; } = 540;

    /// <summary>終業時間（分単位、例: 1080 = 18:00）</summary>
    [JsonIgnore]
    public int WorkEndMin { get; set; } = 1080;

    /// <summary>昼休み開始時間（分単位、例: 720 = 12:00）</summary>
    [JsonIgnore]
    public int LunchStartMin { get; set; } = 720;

    /// <summary>昼休み終了時間（分単位、例: 780 = 13:00）</summary>
    [JsonIgnore]
    public int LunchEndMin { get; set; } = 780;

    // === JSON シリアライズ用プロパティ ===

    /// <summary>始業時間（JSON シリアライズ用、例: "09:00"）</summary>
    [JsonPropertyName("startTime")]
    public string StartTime
    {
        get => TimeConstants.MinutesToTimeString(this.WorkStartMin);
        set => this.WorkStartMin = TimeConstants.TryTimeStringToMinutes(value) ?? 540;
    }

    /// <summary>終業時間（JSON シリアライズ用、例: "18:00"）</summary>
    [JsonPropertyName("endTime")]
    public string EndTime
    {
        get => TimeConstants.MinutesToTimeString(this.WorkEndMin);
        set => this.WorkEndMin = TimeConstants.TryTimeStringToMinutes(value) ?? 1080;
    }

    /// <summary>昼休み開始時間（JSON シリアライズ用、例: "12:00"）</summary>
    [JsonPropertyName("lunchStartTime")]
    public string LunchStartTime
    {
        get => TimeConstants.MinutesToTimeString(this.LunchStartMin);
        set => this.LunchStartMin = TimeConstants.TryTimeStringToMinutes(value) ?? 720;
    }

    /// <summary>昼休み終了時間（JSON シリアライズ用、例: "13:00"）</summary>
    [JsonPropertyName("lunchEndTime")]
    public string LunchEndTime
    {
        get => TimeConstants.MinutesToTimeString(this.LunchEndMin);
        set => this.LunchEndMin = TimeConstants.TryTimeStringToMinutes(value) ?? 780;
    }

    /// <summary>
    /// 始業時間の時（Hour）を取得
    /// </summary>
    public int StartHour => this.WorkStartMin / 60;

    /// <summary>
    /// 終業時間の時（Hour）を取得
    /// </summary>
    public int EndHour => this.WorkEndMin / 60;

    /// <summary>
    /// 昼休み開始時間の時（Hour）を取得
    /// </summary>
    public int LunchStartHour => this.LunchStartMin / 60;

    /// <summary>
    /// 昼休み終了時間の時（Hour）を取得
    /// </summary>
    public int LunchEndHour => this.LunchEndMin / 60;

    /// <summary>
    /// スロットが業務時間外かどうかを判定します（int計算のみ）
    /// </summary>
    /// <param name="slotStartMinutes">スロット開始時刻（分単位）</param>
    /// <param name="slotEndMinutes">スロット終了時刻（分単位）</param>
    /// <returns>業務時間外判定結果（Before/After/Lunchのフラグ）</returns>
    public (bool IsOutsideHoursBefore, bool IsOutsideHoursAfter, bool IsOutsideHoursLunch) CheckSlotOutsideHours(int slotStartMinutes, int slotEndMinutes)
    {
        var isOutsideHoursBefore = false;
        var isOutsideHoursAfter = false;
        var isOutsideHoursLunch = false;

        // 朝の時間外: スロットが業務開始時刻より前に終わる
        if (slotEndMinutes <= this.WorkStartMin)
        {
            isOutsideHoursBefore = true;
        }
        // 夕方の時間外: スロットが業務終了時刻以降に開始する、または業務終了時刻を超える
        else if (slotStartMinutes >= this.WorkEndMin || slotEndMinutes > this.WorkEndMin)
        {
            isOutsideHoursAfter = true;
        }
        // 昼休み時間帯にある場合
        else if (this.LunchStartMin > 0 && this.LunchEndMin > 0)
        {
            if (slotStartMinutes >= this.LunchStartMin && slotEndMinutes <= this.LunchEndMin)
            {
                isOutsideHoursLunch = true;
            }
        }

        return (isOutsideHoursBefore, isOutsideHoursAfter, isOutsideHoursLunch);
    }
}

