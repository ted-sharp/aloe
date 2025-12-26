namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 予約統計スロットエンティティ
/// appointment_stats の時間帯枠ごとの詳細統計データを保持
/// 
/// 注意: 以前は appointment_stats.graph_data (JSONB) で保持していたが、
/// テーブル化によりクエリ性能とデータ整合性が向上しました。
/// </summary>
[Table("appointment_stat_slots")]
public class AppointmentStatSlots : IAuditableEntity
{
    /// <summary>予約統計スロットID (PK)</summary>
    [Key]
    [Column("appt_stat_slot_id")]
    public Guid ApptStatSlotId { get; set; }

    /// <summary>予約統計ID (FK)</summary>
    [Column("appt_stat_id")]
    [ForeignKey("AppointmentStats")]
    public Guid ApptStatId { get; set; }

    /// <summary>予約日</summary>
    [Column("appt_date")]
    public DateOnly ApptDate { get; set; }

    /// <summary>予約リソースID</summary>
    [Column("appt_res_id")]
    public Guid ApptResId { get; set; }

    /// <summary>スロット開始時刻（分単位、0:00からの分）</summary>
    [Column("slot_start")]
    public int SlotStart { get; set; }

    /// <summary>スロット終了時刻（分単位、0:00からの分）</summary>
    [Column("slot_end")]
    public int SlotEnd { get; set; }

    /// <summary>スロット容量</summary>
    [Column("slot_cap")]
    public int SlotCap { get; set; }

    /// <summary>スロット予約数</summary>
    [Column("slot_count")]
    public int SlotCount { get; set; }

    /// <summary>利用可能数（GENERATEDカラム、読み取り専用）</summary>
    [Column("slot_available")]
    public int SlotAvailable { get; set; }

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
    public virtual AppointmentStats AppointmentStats { get; set; } = null!;
    public virtual AppointmentResource? AppointmentResource { get; set; }

    // Helper Properties (NotMapped)
    /// <summary>
    /// スロット開始時刻（TimeOnly形式）
    /// SlotStart（分単位）を TimeOnly に変換
    /// </summary>
    [NotMapped]
    public TimeOnly SlotStartTime
    {
        get => TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(this.SlotStart));
        set => this.SlotStart = value.Hour * 60 + value.Minute;
    }

    /// <summary>
    /// スロット終了時刻（TimeOnly形式）
    /// SlotEnd（分単位）を TimeOnly に変換
    /// </summary>
    [NotMapped]
    public TimeOnly SlotEndTime
    {
        get => TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(this.SlotEnd));
        set => this.SlotEnd = value.Hour * 60 + value.Minute;
    }
}

