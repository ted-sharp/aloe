namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    /// <summary>予約リソースタイプコード</summary>
    [Column("appt_res_type_code")]
    public int ApptResTypeCode { get; set; }

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
    public int ApptResSeq { get; set; }

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
    public virtual ICollection<AppointmentSlot> AppointmentSlots { get; set; } = new List<AppointmentSlot>();
    public virtual ICollection<AppointmentSlotOverride> AppointmentSlotOverrides { get; set; } = new List<AppointmentSlotOverride>();
    public virtual ICollection<AppointmentStats> AppointmentStats { get; set; } = new List<AppointmentStats>();
    public virtual ICollection<AppointmentResourceAssignment> AppointmentResourceReservations { get; set; } = new List<AppointmentResourceAssignment>();
}

