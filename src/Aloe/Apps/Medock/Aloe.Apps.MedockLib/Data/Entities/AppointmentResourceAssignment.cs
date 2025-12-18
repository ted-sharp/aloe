namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 予約リソース予約エンティティ
/// 予約とリソースの多対多関係を表す
/// </summary>
[Table("appointment_resource_assignments")]
public class AppointmentResourceAssignment : IAuditableEntity
{
    /// <summary>予約リソース予約ID (PK)</summary>
    [Key]
    [Column("appt_res_assign_id")]
    public Guid ApptResAssignId { get; set; }

    /// <summary>予約ID (FK)</summary>
    [Column("appt_id")]
    [ForeignKey("Appointment")]
    public Guid ApptId { get; set; }

    /// <summary>予約リソースID (FK)</summary>
    [Column("appt_res_id")]
    [ForeignKey("AppointmentResource")]
    public Guid ApptResId { get; set; }

    /// <summary>予約開始時間</summary>
    [Column("appt_start_time")]
    public TimeOnly? ApptStartTime { get; set; }

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
    public virtual Appointment Appointment { get; set; } = null!;
    public virtual AppointmentResource AppointmentResource { get; set; } = null!;
}

