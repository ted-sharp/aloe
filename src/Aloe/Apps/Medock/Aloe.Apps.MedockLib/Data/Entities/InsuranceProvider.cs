namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 保険者エンティティ
/// </summary>
[Table("insurance_providers")]
public class InsuranceProvider : IAuditableEntity
{
    /// <summary>保険者ID (PK)</summary>
    [Key]
    [Column("insurer_id")]
    public Guid InsurerId { get; set; }

    /// <summary>保険者タイプコード (0=None, 1=全国健康保険協会, 2=組合健保, 3=国民健康保険組合, 4=国保, 5=その他)</summary>
    [Column("insurer_type_code")]
    public int InsurerTypeCode { get; set; }

    /// <summary>保険者コード（可能であれば保険者番号）</summary>
    [Column("insurer_code")]
    public string InsurerCode { get; set; } = String.Empty;

    /// <summary>保険者名</summary>
    [Column("insurer_name")]
    public string InsurerName { get; set; } = String.Empty;

    /// <summary>保険者略称</summary>
    [Column("insurer_short_name")]
    public string InsurerShortName { get; set; } = String.Empty;

    /// <summary>保険者説明</summary>
    [Column("insurer_desc")]
    public string InsurerDesc { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    [Column("insurer_seq")]
    public int InsurerSeq { get; set; }

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
    public virtual ICollection<OrganizationInsurance> OrganizationInsurances { get; set; } = new List<OrganizationInsurance>();
    public virtual ICollection<PatientInsuranceCard> PatientInsuranceCards { get; set; } = new List<PatientInsuranceCard>();
}

