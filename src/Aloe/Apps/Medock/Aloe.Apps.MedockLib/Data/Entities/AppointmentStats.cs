namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 予約統計エンティティ
/// 日別の予約状況（時間帯枠ごとの予約数/最大数）を保持
/// 
/// 注意: 年間カレンダー表示などのパフォーマンス向上のため、このエンティティは
/// AppointmentResourceやAppointmentSlotを直接使用せず、事前に集計された統計データを保持します。
/// これにより、毎回の集計処理のコストを削減し、表示速度を向上させています。
/// 
/// 時間帯枠ごとの詳細データは AppointmentStatSlots テーブルで管理されます。
/// </summary>
[Table("appointment_stats")]
public class AppointmentStats : IAuditableEntity
{
    /// <summary>予約統計ID (PK)</summary>
    [Key]
    [Column("appt_stat_id")]
    public Guid ApptStatId { get; set; }

    /// <summary>予約日</summary>
    [Column("appt_date")]
    public DateOnly ApptDate { get; set; }

    /// <summary>予約リソースID (FK)</summary>
    [Column("appt_res_id")]
    [ForeignKey("AppointmentResource")]
    public Guid ApptResId { get; set; }

    /// <summary>予約容量（合計）</summary>
    [Column("appt_cap")]
    public int ApptCap { get; set; } = 0;

    /// <summary>予約数（合計）</summary>
    [Column("appt_count")]
    public int ApptCount { get; set; } = 0;

    /// <summary>利用可能数（GENERATEDカラム、読み取り専用）</summary>
    [Column("appt_available")]
    public int ApptAvailable { get; set; }

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
    public virtual AppointmentResource AppointmentResource { get; set; } = null!;
    public virtual ICollection<AppointmentStatSlots> AppointmentStatSlots { get; set; } = new List<AppointmentStatSlots>();
}

