namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 施設住所エンティティ
/// </summary>
[Table("facility_addresses")]
public class FacilityAddress : IAuditableEntity
{
    /// <summary>施設住所ID (PK)</summary>
    [Key]
    [Column("facility_adr_id")]
    public Guid FacilityAdrId { get; set; }

    /// <summary>施設ID (FK)</summary>
    [Column("facility_id")]
    [ForeignKey("Facility")]
    public Guid FacilityId { get; set; }

    /// <summary>住所タイプコード</summary>
    [Column("adr_type_code")]
    public int AdrTypeCode { get; set; }

    /// <summary>郵便番号</summary>
    [Column("postal_code")]
    [MaxLength(7)]
    public string PostalCode { get; set; } = String.Empty;

    /// <summary>住所1</summary>
    [Column("adr1")]
    [MaxLength(100)]
    public string Adr1 { get; set; } = String.Empty;

    /// <summary>住所2</summary>
    [Column("adr2")]
    [MaxLength(100)]
    public string Adr2 { get; set; } = String.Empty;

    /// <summary>住所3</summary>
    [Column("adr3")]
    [MaxLength(100)]
    public string Adr3 { get; set; } = String.Empty;

    /// <summary>宛名</summary>
    [Column("attention_name")]
    [MaxLength(100)]
    public string AttentionName { get; set; } = String.Empty;

    /// <summary>電話番号</summary>
    [Column("tel")]
    [MaxLength(20)]
    public string Tel { get; set; } = String.Empty;

    /// <summary>電話番号2</summary>
    [Column("tel2")]
    [MaxLength(20)]
    public string Tel2 { get; set; } = String.Empty;

    /// <summary>FAX番号</summary>
    [Column("fax")]
    [MaxLength(20)]
    public string Fax { get; set; } = String.Empty;

    /// <summary>メールアドレス</summary>
    [Column("email")]
    [MaxLength(255)]
    public string Email { get; set; } = String.Empty;

    /// <summary>住所メモ</summary>
    [Column("adr_memo")]
    [MaxLength(1000)]
    public string AdrMemo { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    [Column("adr_seq")]
    public int AdrSeq { get; set; }

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
}

