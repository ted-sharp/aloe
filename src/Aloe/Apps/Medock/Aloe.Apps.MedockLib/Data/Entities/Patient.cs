namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 患者エンティティ
/// </summary>
public class Patient : IAuditableEntity
{
    /// <summary>患者ID (PK)</summary>
    public Guid PtId { get; set; }

    /// <summary>正規患者ID（名寄せ検索用）</summary>
    public Guid CanonicalPtId { get; set; }

    /// <summary>テナントID (FK)</summary>
    public Guid TenantId { get; set; }

    /// <summary>施設ID (FK)</summary>
    public Guid FacilityId { get; set; }

    /// <summary>主団体ID</summary>
    public Guid PrimaryOrgId { get; set; }

    /// <summary>患者コード</summary>
    public string PtCode { get; set; } = string.Empty;

    /// <summary>カルテコード</summary>
    public string? KarteCode { get; set; }

    /// <summary>患者名</summary>
    public string PtName { get; set; } = string.Empty;

    /// <summary>患者名（互換）JIS縮むで第二水準までにしたもの</summary>
    public string PtNameCompat { get; set; } = string.Empty;

    /// <summary>患者名カタカナ</summary>
    public string PtNameKatakana { get; set; } = string.Empty;

    /// <summary>患者名カタカナ（互換）トリガで更新、全角カナ、区切り文字統一、unaccent</summary>
    public string PtNameKatakanaCompat { get; set; } = string.Empty;

    /// <summary>旧姓名（名寄せなどで使用）</summary>
    public string PtMadenName { get; set; } = string.Empty;

    /// <summary>別名・芸名（印刷用）</summary>
    public string PtAliasName { get; set; } = string.Empty;

    /// <summary>生年月日</summary>
    public DateOnly BirthDate { get; set; }

    /// <summary>性別コード (0: None, 1: Man, 2: Woman, 9: Unknown)</summary>
    public int SexCode { get; set; }

    /// <summary>メモ</summary>
    public string PtMemo { get; set; } = string.Empty;

    /// <summary>削除フラグ</summary>
    public bool IsDeleted { get; set; }

    // IAuditableEntity
    public DateTimeOffset CreatedAt { get; set; }
    public Guid CreatedUserId { get; set; }
    public Guid CreatedSessionId { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid UpdatedUserId { get; set; }
    public Guid UpdatedSessionId { get; set; }

    // Navigation Properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Facility Facility { get; set; } = null!;
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}


