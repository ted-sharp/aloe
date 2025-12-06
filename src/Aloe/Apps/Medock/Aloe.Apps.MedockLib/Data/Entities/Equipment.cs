namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 設備エンティティ
/// </summary>
[Table("equipments")]
public class Equipment : IAuditableEntity
{
    /// <summary>設備ID (PK)</summary>
    [Key]
    [Column("equip_id")]
    public Guid EquipId { get; set; }

    /// <summary>フロアID (FK)</summary>
    [Column("floor_id")]
    [ForeignKey("Floor")]
    public Guid FloorId { get; set; }

    /// <summary>設備名</summary>
    [Column("equip_name")]
    [MaxLength(100)]
    public string EquipName { get; set; } = String.Empty;

    /// <summary>設備説明</summary>
    [Column("equip_desc")]
    [MaxLength(500)]
    public string EquipDesc { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    [Column("equip_seq")]
    public int EquipSeq { get; set; }

    /// <summary>削除フラグ</summary>
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    // IAuditableEntity
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    [Column("created_user_id")]
    public Guid CreatedUserId { get; set; }
    [Column("created_session_id")]
    public Guid CreatedSessionId { get; set; }
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
    [Column("updated_user_id")]
    public Guid UpdatedUserId { get; set; }
    [Column("updated_session_id")]
    public Guid UpdatedSessionId { get; set; }

    // Navigation Properties
    public virtual Floor Floor { get; set; } = null!;
    public virtual ICollection<EquipmentSlot> EquipmentSlots { get; set; } = new List<EquipmentSlot>();
    public virtual ICollection<EquipmentAppointment> EquipmentAppointments { get; set; } = new List<EquipmentAppointment>();
    public virtual ICollection<EquipmentAppointmentStats> EquipmentAppointmentStats { get; set; } = new List<EquipmentAppointmentStats>();
}
