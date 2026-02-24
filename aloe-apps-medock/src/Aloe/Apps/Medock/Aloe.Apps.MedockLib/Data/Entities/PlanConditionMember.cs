namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// プラン条件メンバーエンティティ
/// </summary>
/// <remarks>
/// FUTURE FEATURE: Role-Based Access Control (RBAC) 実装用に予約されています。
/// 現在はアクティブに使用されていません。実装計画については CLAUDE.md を参照してください。
/// </remarks>
[Table("plan_condition_members")]
public class PlanConditionMember : IAuditableEntity
{
    /// <summary>プラン条件メンバーID (PK)</summary>
    [Key]
    [Column("plan_cond_member_id")]
    public Guid PlanCondMemberId { get; set; }

    /// <summary>プランID (FK)</summary>
    [Column("plan_id")]
    [ForeignKey("Plan")]
    public Guid PlanId { get; set; }

    /// <summary>プラン条件ID (FK)</summary>
    [Column("plan_cond_id")]
    [ForeignKey("PlanCondition")]
    public Guid PlanCondId { get; set; }

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
    public virtual PlanCondition PlanCondition { get; set; } = null!;
}

