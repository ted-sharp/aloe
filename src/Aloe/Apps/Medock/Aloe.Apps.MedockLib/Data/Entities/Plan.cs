namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// プランエンティティ
/// </summary>
[Table("plans")]
public class Plan : IAuditableEntity
{
    /// <summary>プランID (PK)</summary>
    [Key]
    [Column("plan_id")]
    public Guid PlanId { get; set; }

    /// <summary>施設ID (FK)</summary>
    [Column("facility_id")]
    [ForeignKey("Facility")]
    public Guid FacilityId { get; set; }

    /// <summary>プランコード</summary>
    [Column("plan_code")]
    [MaxLength(20)]
    public string PlanCode { get; set; } = String.Empty;

    /// <summary>プラン名</summary>
    [Column("plan_name")]
    [MaxLength(100)]
    public string PlanName { get; set; } = String.Empty;

    /// <summary>プラン短縮名</summary>
    [Column("plan_short_name")]
    [MaxLength(100)]
    public string PlanShortName { get; set; } = String.Empty;

    /// <summary>プラン略称</summary>
    [Column("plan_abbr_name")]
    [MaxLength(100)]
    public string PlanAbbrName { get; set; } = String.Empty;

    /// <summary>プラン説明</summary>
    [Column("plan_desc")]
    [MaxLength(1000)]
    public string PlanDesc { get; set; } = String.Empty;

    /// <summary>プラン種別コード</summary>
    [Column("plan_kind_code")]
    public int PlanKindCode { get; set; }

    /// <summary>有効フラグ</summary>
    [Column("is_active")]
    public bool IsActive { get; set; }

    /// <summary>有効開始日</summary>
    [Column("active_from")]
    public DateOnly ActiveFrom { get; set; }

    /// <summary>有効終了日</summary>
    [Column("active_to")]
    public DateOnly ActiveTo { get; set; } = new DateOnly(9999, 12, 31);

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
    public virtual Facility Facility { get; set; } = null!;
    public virtual ICollection<PlanConditionMember> PlanConditionMembers { get; set; } = new List<PlanConditionMember>();
    public virtual ICollection<PlanOption> PlanOptions { get; set; } = new List<PlanOption>();
    public virtual ICollection<PlanResourceRequirement> PlanResourceRequirements { get; set; } = new List<PlanResourceRequirement>();
}

