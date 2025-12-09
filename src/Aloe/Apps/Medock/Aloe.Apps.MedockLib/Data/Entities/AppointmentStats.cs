namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 予約統計エンティティ
/// 日別の予約状況（時間帯枠ごとの予約数/最大数）をJSONBで保持
/// </summary>
[Table("appointment_stats")]
public class AppointmentStats : IAuditableEntity
{
    /// <summary>予約統計ID (PK)</summary>
    [Key]
    [Column("appt_stat_id")]
    public Guid ApptStatId { get; set; }

    /// <summary>フロアID (FK)</summary>
    [Column("floor_id")]
    [ForeignKey("Floor")]
    public Guid FloorId { get; set; }

    /// <summary>予約日</summary>
    [Column("appt_date")]
    public DateOnly ApptDate { get; set; }

    /// <summary>予約数（合計）</summary>
    [Column("appt_count")]
    public int ApptCount { get; set; }

    /// <summary>予約最大数（合計）</summary>
    [Column("appt_max")]
    public int ApptMax { get; set; }

    /// <summary>
    /// 時間帯枠ごとのグラフデータ（JSONB）
    /// 例: { "slots": [{ "time": "08:00", "count": 3, "max": 5 }, ...] }
    /// </summary>
    [Column("appt_graph")]
    public string ApptGraph { get; set; } = "{}";

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
    public virtual Floor Floor { get; set; } = null!;
}

