namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// フロアエンティティ
/// </summary>
[Table("floors")]
public class Floor : IAuditableEntity
{
    /// <summary>フロアID (PK)</summary>
    [Key]
    [Column("floor_id")]
    public Guid FloorId { get; set; }

    /// <summary>施設ID (FK)</summary>
    [Column("facility_id")]
    [ForeignKey("Facility")]
    public Guid FacilityId { get; set; }

    /// <summary>フロアコード</summary>
    [Column("floor_code")]
    [MaxLength(10)]
    public string FloorCode { get; set; } = String.Empty;

    /// <summary>フロア名</summary>
    [Column("floor_name")]
    [MaxLength(100)]
    public string FloorName { get; set; } = String.Empty;

    /// <summary>フロア説明</summary>
    [Column("floor_desc")]
    [MaxLength(1000)]
    public string FloorDesc { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    [Column("floor_seq")]
    public int FloorSeq { get; set; }

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
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<AppointmentResource> AppointmentResources { get; set; } = new List<AppointmentResource>();
}


