namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 患者エンティティ
/// </summary>
[Table("patients")]
public class Patient : IAuditableEntity
{
    /// <summary>患者ID (PK)</summary>
    [Key]
    [Column("pt_id")]
    public Guid PtId { get; set; }

    /// <summary>正規患者ID（名寄せ検索用）</summary>
    [Column("canonical_pt_id")]
    public Guid CanonicalPtId { get; set; }

    /// <summary>施設ID (FK)</summary>
    [Column("facility_id")]
    [ForeignKey("Facility")]
    public Guid FacilityId { get; set; }

    /// <summary>主団体ID</summary>
    [Column("primary_org_id")]
    public Guid PrimaryOrgId { get; set; }

    /// <summary>患者コード</summary>
    [Column("pt_code")]
    [MaxLength(100)]
    public string PtCode { get; set; } = String.Empty;

    /// <summary>カルテコード</summary>
    [Column("karte_code")]
    [MaxLength(50)]
    public string? KarteCode { get; set; }

    /// <summary>患者名</summary>
    [Column("pt_name")]
    [MaxLength(200)]
    public string PtName { get; set; } = String.Empty;

    /// <summary>患者名（互換）JIS縮むで第二水準までにしたもの</summary>
    [Column("pt_name_compat")]
    [MaxLength(200)]
    public string PtNameCompat { get; set; } = String.Empty;

    /// <summary>患者名カタカナ</summary>
    [Column("pt_name_katakana")]
    [MaxLength(200)]
    public string PtNameKatakana { get; set; } = String.Empty;

    /// <summary>患者名カタカナ（互換）トリガで更新、全角カナ、区切り文字統一、unaccent</summary>
    [Column("pt_name_katakana_compat")]
    [MaxLength(200)]
    public string PtNameKatakanaCompat { get; set; } = String.Empty;

    /// <summary>旧姓名（名寄せなどで使用）</summary>
    [Column("pt_maiden_name")]
    [MaxLength(200)]
    public string PtMaidenName { get; set; } = String.Empty;

    /// <summary>別名・芸名（印刷用）</summary>
    [Column("pt_alias_name")]
    [MaxLength(200)]
    public string PtAliasName { get; set; } = String.Empty;

    /// <summary>生年月日</summary>
    [Column("birth_date")]
    public DateOnly BirthDate { get; set; }

    /// <summary>性別コード (0: None, 1: Man, 2: Woman, 9: Unknown)</summary>
    [Column("sex_code")]
    public int SexCode { get; set; }

    /// <summary>メモ</summary>
    [Column("pt_memo")]
    [MaxLength(500)]
    public string PtMemo { get; set; } = String.Empty;

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
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<EquipmentAppointment> EquipmentAppointments { get; set; } = new List<EquipmentAppointment>();
}


