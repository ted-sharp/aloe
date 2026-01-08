namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 施設営業時間エンティティ
/// 施設ごとの営業時間を保持
/// </summary>
[Table("facility_business_hours")]
public class FacilityBusinessHours : IAuditableEntity
{
    /// <summary>施設営業時間ID (PK)</summary>
    [Key]
    [Column("facility_business_hours_id")]
    public Guid FacilityBusinessHoursId { get; set; }

    /// <summary>施設ID (FK)</summary>
    [Column("facility_id")]
    [ForeignKey("Facility")]
    public Guid FacilityId { get; set; }

    /// <summary>始業時間（0時からの分数、例: 09:00 = 540）</summary>
    [Column("work_start_min")]
    public int WorkStartMin { get; set; }

    /// <summary>終業時間（0時からの分数、例: 18:00 = 1080）</summary>
    [Column("work_end_min")]
    public int WorkEndMin { get; set; }

    /// <summary>昼休み開始時間（0時からの分数、例: 12:00 = 720）</summary>
    [Column("lunch_start_min")]
    public int LunchStartMin { get; set; }

    /// <summary>昼休み終了時間（0時からの分数、例: 13:00 = 780）</summary>
    [Column("lunch_end_min")]
    public int LunchEndMin { get; set; }

    /// <summary>有効フラグ</summary>
    [Column("is_active")]
    public bool IsActive { get; set; }

    /// <summary>有効開始日</summary>
    [Column("active_from")]
    public DateOnly ActiveFrom { get; set; }

    /// <summary>有効終了日</summary>
    [Column("active_to")]
    public DateOnly ActiveTo { get; set; } = new DateOnly(9999, 12, 31);

    /// <summary>削除フラグ</summary>
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    // IAuditableEntity
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    [Column("created_user_id")]
    public Guid CreatedUserId { get; set; }
    [Column("created_session_id")]
    public Guid CreatedSessionId { get; set; }
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
    [Column("updated_user_id")]
    public Guid UpdatedUserId { get; set; }
    [Column("updated_session_id")]
    public Guid UpdatedSessionId { get; set; }

    // Navigation Properties
    public virtual Facility Facility { get; set; } = null!;

    /// <summary>
    /// 分数から "HH:mm" 形式の文字列に変換
    /// </summary>
    public static string MinutesToTimeString(int minutes)
    {
        var hours = minutes / 60;
        var mins = minutes % 60;
        return $"{hours:D2}:{mins:D2}";
    }

    /// <summary>
    /// "HH:mm" 形式の文字列から分数に変換
    /// </summary>
    public static int TimeStringToMinutes(string timeString)
    {
        var parts = timeString.Split(':');
        if (parts.Length != 2) return 0;
        if (!Int32.TryParse(parts[0], out var hours)) return 0;
        if (!Int32.TryParse(parts[1], out var mins)) return 0;
        return hours * 60 + mins;
    }
}
