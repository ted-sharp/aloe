namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// プラン条件エンティティ
/// </summary>
[Table("plan_conditions")]
public class PlanCondition : IAuditableEntity
{
    /// <summary>プラン条件ID (PK)</summary>
    [Key]
    [Column("plan_cond_id")]
    public Guid PlanCondId { get; set; }

    /// <summary>条件名</summary>
    [Column("condition_name")]
    [MaxLength(100)]
    public string ConditionName { get; set; } = String.Empty;

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
    public virtual ICollection<PlanConditionMember> PlanConditionMembers { get; set; } = new List<PlanConditionMember>();
}

