namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// フロアエンティティ
/// </summary>
public class Floor : IAuditableEntity
{
    /// <summary>フロアID (PK)</summary>
    public Guid FloorId { get; set; }

    /// <summary>施設ID (FK)</summary>
    public Guid FacilityId { get; set; }

    /// <summary>フロアコード</summary>
    public string FloorCode { get; set; } = String.Empty;

    /// <summary>フロア名</summary>
    public string FloorName { get; set; } = String.Empty;

    /// <summary>フロア説明</summary>
    public string FloorDesc { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    public int FloorSeq { get; set; }

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
    public virtual Facility Facility { get; set; } = null!;
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    public virtual ICollection<AppointmentSlot> AppointmentSlots { get; set; } = new List<AppointmentSlot>();
    public virtual ICollection<AppointmentStats> AppointmentStats { get; set; } = new List<AppointmentStats>();
}


