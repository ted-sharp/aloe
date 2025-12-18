namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 団体保険エンティティ
/// </summary>
[Table("organization_insurances")]
public class OrganizationInsurance : IAuditableEntity
{
    /// <summary>団体保険ID (PK)</summary>
    [Key]
    [Column("org_insurance_id")]
    public Guid OrgInsuranceId { get; set; }

    /// <summary>団体ID (FK)</summary>
    [Column("org_id")]
    [ForeignKey("Organization")]
    public Guid OrgId { get; set; }

    /// <summary>主保険フラグ</summary>
    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    /// <summary>保険者ID (FK)</summary>
    [Column("insurer_id")]
    [ForeignKey("InsuranceProvider")]
    public Guid? InsurerId { get; set; }

    /// <summary>保険者タイプコード (0=None, 1=全国健康保険協会, 2=組合健保, 3=国民健康保険組合, 4=国保, 5=その他)</summary>
    [Column("insurer_type_code")]
    public int InsurerTypeCode { get; set; }

    /// <summary>保険者コード</summary>
    [Column("insurer_code")]
    public string InsurerCode { get; set; } = String.Empty;

    /// <summary>有効フラグ</summary>
    [Column("is_active")]
    public bool IsActive { get; set; }

    /// <summary>無効化日</summary>
    [Column("deactivated_on")]
    public DateOnly? DeactivatedOn { get; set; }

    /// <summary>団体保険メモ</summary>
    [Column("org_insurance_memo")]
    public string OrgInsuranceMemo { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    [Column("org_insurance_seq")]
    public int OrgInsuranceSeq { get; set; }

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
    public virtual Organization Organization { get; set; } = null!;
    public virtual InsuranceProvider? InsuranceProvider { get; set; }
}

