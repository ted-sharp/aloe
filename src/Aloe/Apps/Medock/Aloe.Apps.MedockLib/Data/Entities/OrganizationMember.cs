namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 団体メンバーエンティティ
/// </summary>
[Table("organization_members")]
public class OrganizationMember : IAuditableEntity
{
    /// <summary>団体メンバーID (PK)</summary>
    [Key]
    [Column("org_member_id")]
    public Guid OrgMemberId { get; set; }

    /// <summary>団体ID (FK)</summary>
    [Column("org_id")]
    [ForeignKey("Organization")]
    public Guid OrgId { get; set; }

    /// <summary>患者ID (FK)</summary>
    [Column("pt_id")]
    [ForeignKey("Patient")]
    public Guid PtId { get; set; }

    /// <summary>個人コード（社員番号、学生番号）</summary>
    [Column("personal_code")]
    [MaxLength(100)]
    public string PersonalCode { get; set; } = String.Empty;

    /// <summary>部署</summary>
    [Column("department")]
    [MaxLength(100)]
    public string Department { get; set; } = String.Empty;

    /// <summary>有効フラグ</summary>
    [Column("is_active")]
    public bool IsActive { get; set; }

    /// <summary>無効化日</summary>
    [Column("deactivated_on")]
    public DateOnly? DeactivatedOn { get; set; }

    /// <summary>団体メンバーメモ</summary>
    [Column("org_member_memo")]
    [MaxLength(1000)]
    public string OrgMemberMemo { get; set; } = String.Empty;

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
    public virtual Patient Patient { get; set; } = null!;
}

