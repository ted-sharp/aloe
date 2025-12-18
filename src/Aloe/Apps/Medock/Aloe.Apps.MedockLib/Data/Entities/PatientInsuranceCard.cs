namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 患者保険証エンティティ
/// </summary>
[Table("patient_insurance_cards")]
public class PatientInsuranceCard : IAuditableEntity
{
    /// <summary>患者保険証ID (PK)</summary>
    [Key]
    [Column("pt_insur_card_id")]
    public Guid PtInsurCardId { get; set; }

    /// <summary>患者ID (FK)</summary>
    [Column("pt_id")]
    [ForeignKey("Patient")]
    public Guid PtId { get; set; }

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

    /// <summary>保険者名</summary>
    [Column("insurer_name")]
    public string InsurerName { get; set; } = String.Empty;

    /// <summary>被保険者番号（記号、番号、枝番）</summary>
    [Column("insured_code")]
    public string InsuredCode { get; set; } = String.Empty;

    /// <summary>被保険者番号記号</summary>
    [Column("insured_code_symbol")]
    public string InsuredCodeSymbol { get; set; } = String.Empty;

    /// <summary>被保険者番号</summary>
    [Column("insured_code_number")]
    public string InsuredCodeNumber { get; set; } = String.Empty;

    /// <summary>被保険者番号枝番</summary>
    [Column("insured_code_branch_number")]
    public string InsuredCodeBranchNumber { get; set; } = String.Empty;

    /// <summary>被保険者氏名</summary>
    [Column("insured_person_name")]
    public string InsuredPersonName { get; set; } = String.Empty;

    /// <summary>本人・家族区分 (1=Self[本人], 2=Dependents[家族])</summary>
    [Column("self_family_relationship_code")]
    public string SelfFamilyRelationshipCode { get; set; } = String.Empty;

    /// <summary>負担割合区分 (0=負担0割, A=負担1割)</summary>
    [Column("assistance_code")]
    public string AssistanceCode { get; set; } = String.Empty;

    /// <summary>継続区分 (0=None, 1=ExtendedCare[長期継続], 2=VoluntaryContinuation[任意継続])</summary>
    [Column("continuation_code")]
    public string ContinuationCode { get; set; } = String.Empty;

    /// <summary>有効フラグ</summary>
    [Column("is_active")]
    public bool IsActive { get; set; }

    /// <summary>無効化日</summary>
    [Column("deactivated_on")]
    public DateOnly? DeactivatedOn { get; set; }

    /// <summary>患者保険証メモ</summary>
    [Column("pt_insure_card_memo")]
    public string PtInsureCardMemo { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    [Column("pt_insure_card_seq")]
    public int PtInsureCardSeq { get; set; }

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
    public virtual Patient Patient { get; set; } = null!;
    public virtual InsuranceProvider? InsuranceProvider { get; set; }
}

