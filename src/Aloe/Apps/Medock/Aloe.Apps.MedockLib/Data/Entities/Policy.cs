namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// ポリシーエンティティ
/// </summary>
/// <remarks>
/// FUTURE FEATURE: Role-Based Access Control (RBAC) 実装用に予約されています。
/// 現在はアクティブに使用されていません。実装計画については CLAUDE.md を参照してください。
/// </remarks>
[Table("policies")]
public class Policy : IAuditableEntity
{
    /// <summary>ポリシーコード (PK)</summary>
    [Key]
    [Column("policy_code")]
    [MaxLength(100)]
    public string PolicyCode { get; set; } = String.Empty;

    /// <summary>ポリシー名</summary>
    [Column("policy_name")]
    [MaxLength(100)]
    public string PolicyName { get; set; } = String.Empty;

    /// <summary>ポリシー説明</summary>
    [Column("policy_desc")]
    [MaxLength(1000)]
    public string PolicyDesc { get; set; } = String.Empty;

    /// <summary>データ型</summary>
    [Column("data_type")]
    [MaxLength(10)]
    public string DataType { get; set; } = String.Empty;

    /// <summary>ポリシー値</summary>
    [Column("policy_value")]
    [MaxLength(10)]
    public string PolicyValue { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    [Column("policy_seq")]
    public int PolicySeq { get; set; }

    /// <summary>有効フラグ</summary>
    [Column("is_active")]
    public bool IsActive { get; set; }

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
    public virtual ICollection<FacilityPolicy> FacilityPolicies { get; set; } = new List<FacilityPolicy>();
}

