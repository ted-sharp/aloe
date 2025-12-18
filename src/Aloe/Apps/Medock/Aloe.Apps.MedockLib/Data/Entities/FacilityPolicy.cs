namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 施設ポリシーエンティティ
/// </summary>
[Table("facility_policies")]
public class FacilityPolicy : IAuditableEntity
{
    /// <summary>施設ポリシーID (PK)</summary>
    [Key]
    [Column("facility_policy_id")]
    public Guid FacilityPolicyId { get; set; }

    /// <summary>施設ID (FK)</summary>
    [Column("facility_id")]
    [ForeignKey("Facility")]
    public Guid FacilityId { get; set; }

    /// <summary>ポリシーコード (FK)</summary>
    [Column("policy_code")]
    [ForeignKey("Policy")]
    [MaxLength(100)]
    public string PolicyCode { get; set; } = String.Empty;

    /// <summary>ポリシー値</summary>
    [Column("policy_value")]
    [MaxLength(10)]
    public string PolicyValue { get; set; } = String.Empty;

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
    public virtual Policy Policy { get; set; } = null!;
}

