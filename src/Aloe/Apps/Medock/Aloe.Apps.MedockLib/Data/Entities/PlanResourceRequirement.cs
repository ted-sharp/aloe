namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// プランリソース要件エンティティ
/// </summary>
[Table("plan_resource_requirements")]
public class PlanResourceRequirement : IAuditableEntity
{
    /// <summary>プランリソース要件ID (PK)</summary>
    [Key]
    [Column("plan_res_req_id")]
    public Guid PlanResReqId { get; set; }

    /// <summary>プランID (FK)</summary>
    [Column("plan_id")]
    [ForeignKey("Plan")]
    public Guid PlanId { get; set; }

    /// <summary>予約リソースID (FK)</summary>
    [Column("appt_res_id")]
    [ForeignKey("AppointmentResource")]
    public Guid ApptResId { get; set; }

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
    public virtual Plan Plan { get; set; } = null!;
    public virtual AppointmentResource AppointmentResource { get; set; } = null!;
}

