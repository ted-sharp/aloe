namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 予約リソースメンバーエンティティ
/// appt_res_type_code = EquipmentGroup の関連リソース関連付け
/// </summary>
[Table("appointment_resource_members")]
public class AppointmentResourceMember : IAuditableEntity
{
    /// <summary>予約リソースメンバーID (PK)</summary>
    [Key]
    [Column("appt_res_member_id")]
    public Guid ApptResMemberId { get; set; }

    /// <summary>予約リソースID (FK)</summary>
    [Column("appt_res_id")]
    [ForeignKey("AppointmentResource")]
    public Guid ApptResId { get; set; }

    /// <summary>メンバー予約リソースID (FK)</summary>
    [Column("member_appt_res_id")]
    [ForeignKey("MemberAppointmentResource")]
    public Guid MemberApptResId { get; set; }

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
    public virtual AppointmentResource MemberAppointmentResource { get; set; } = null!;
}
