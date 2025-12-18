namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 予約リソースグループメンバーエンティティ
/// </summary>
[Table("appointment_resource_group_members")]
public class AppointmentResourceGroupMember : IAuditableEntity
{
    /// <summary>予約リソースグループメンバーID (PK)</summary>
    [Key]
    [Column("appt_res_group_member_id")]
    public Guid ApptResGroupMemberId { get; set; }

    /// <summary>予約リソースID (FK)</summary>
    [Column("appt_res_id")]
    [ForeignKey("AppointmentResource")]
    public Guid ApptResId { get; set; }

    /// <summary>予約リソースグループID (FK)</summary>
    [Column("appt_res_group_id")]
    [ForeignKey("AppointmentResourceGroup")]
    public Guid ApptResGroupId { get; set; }

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
    public virtual AppointmentResourceGroup AppointmentResourceGroup { get; set; } = null!;
}

