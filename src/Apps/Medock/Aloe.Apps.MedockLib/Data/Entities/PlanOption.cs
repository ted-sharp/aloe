namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// プランオプションエンティティ
/// </summary>
[Table("plan_options")]
public class PlanOption : IAuditableEntity
{
    /// <summary>プランオプションID (PK)</summary>
    [Key]
    [Column("plan_option_id")]
    public Guid PlanOptionId { get; set; }

    /// <summary>プランID (FK)</summary>
    [Column("plan_id")]
    [ForeignKey("Plan")]
    public Guid PlanId { get; set; }

    /// <summary>オプションプランID (FK)</summary>
    [Column("option_plan_id")]
    [ForeignKey("OptionPlan")]
    public Guid OptionPlanId { get; set; }

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
    public virtual Plan OptionPlan { get; set; } = null!;
}

