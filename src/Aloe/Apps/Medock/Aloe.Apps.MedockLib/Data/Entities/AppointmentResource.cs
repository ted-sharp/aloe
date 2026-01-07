namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Aloe.Apps.MedockLib.Constants;

/// <summary>
/// 予約リソースエンティティ
/// 部屋、設備などの予約可能なリソースを表す
/// </summary>
[Table("appointment_resources")]
public class AppointmentResource : IAuditableEntity
{
    /// <summary>予約リソースID (PK)</summary>
    [Key]
    [Column("appt_res_id")]
    public Guid ApptResId { get; set; }

    /// <summary>フロアID (FK)</summary>
    [Column("floor_id")]
    [ForeignKey("Floor")]
    public Guid FloorId { get; set; }

    /// <summary>
    /// 予約リソースタイプコード
    /// 1: Main - メインスロット（30分区切り、棒グラフ表示予定）
    /// 2: Equipment - 設備（折れ線グラフ表示予定）
    /// 3: Environment - 環境（背景積み立てグラフ表示予定）
    /// 99: Others - その他（カレンダー非表示）
    /// </summary>
    [Column("appt_res_type_code")]
    public int ApptResTypeCode { get; set; } = 0;

    /// <summary>
    /// 予約リソースタイプ（enum型、型安全なアクセサー）
    /// ApptResTypeCode の enum ラッパー
    /// </summary>
    [NotMapped]
    public AppointmentResourceType AppointmentResourceType
    {
        get => (AppointmentResourceType)this.ApptResTypeCode;
        set => this.ApptResTypeCode = (int)value;
    }

    /// <summary>予約リソース名</summary>
    [Column("appt_res_name")]
    [MaxLength(100)]
    public string ApptResName { get; set; } = String.Empty;

    /// <summary>予約リソース説明</summary>
    [Column("appt_res_desc")]
    [MaxLength(1000)]
    public string ApptResDesc { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    [Column("appt_res_seq")]
    public int ApptResSeq { get; set; } = 0;

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
    public virtual ICollection<AppointmentSchedule> AppointmentSchedules { get; set; } = new List<AppointmentSchedule>();
    public virtual ICollection<AppointmentStats> AppointmentStats { get; set; } = new List<AppointmentStats>();
    public virtual ICollection<AppointmentResourceAssignment> AppointmentResourceReservations { get; set; } = new List<AppointmentResourceAssignment>();
}

