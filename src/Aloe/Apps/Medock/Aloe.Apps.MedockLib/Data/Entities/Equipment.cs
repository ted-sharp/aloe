namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 設備エンティティ
/// </summary>
public class Equipment : IAuditableEntity
{
    /// <summary>設備ID (PK)</summary>
    public Guid EquipId { get; set; }

    /// <summary>フロアID (FK)</summary>
    public Guid FloorId { get; set; }

    /// <summary>設備名</summary>
    public string EquipName { get; set; } = string.Empty;

    /// <summary>設備説明</summary>
    public string EquipDesc { get; set; } = string.Empty;

    /// <summary>表示順</summary>
    public int EquipSeq { get; set; }

    /// <summary>削除フラグ</summary>
    public bool IsDeleted { get; set; }

    // IAuditableEntity
    public DateTimeOffset CreatedAt { get; set; }
    public Guid CreatedUserId { get; set; }
    public Guid CreatedSessionId { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid UpdatedUserId { get; set; }
    public Guid UpdatedSessionId { get; set; }

    // Navigation Properties
    public virtual Floor Floor { get; set; } = null!;
    public virtual ICollection<EquipmentSlot> EquipmentSlots { get; set; } = new List<EquipmentSlot>();
    public virtual ICollection<EquipmentAppointment> EquipmentAppointments { get; set; } = new List<EquipmentAppointment>();
    public virtual ICollection<EquipmentAppointmentStats> EquipmentAppointmentStats { get; set; } = new List<EquipmentAppointmentStats>();
}
