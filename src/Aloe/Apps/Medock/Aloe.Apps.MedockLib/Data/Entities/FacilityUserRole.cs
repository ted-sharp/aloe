namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// ユーザーロール（ユーザーとロールの関連）エンティティ
/// </summary>
[Table("facility_user_roles")]
public class FacilityUserRole : IAuditableEntity
{
    /// <summary>ユーザーロールID (PK)</summary>
    [Key]
    [Column("facility_user_role_id")]
    public Guid FacilityUserRoleId { get; set; }

    /// <summary>ユーザーID (FK)</summary>
    [Column("facility_user_id")]
    [ForeignKey("FacilityUser")]
    public Guid FacilityUserId { get; set; }

    /// <summary>ロールコード (FK)</summary>
    [Column("role_code")]
    [MaxLength(10)]
    [ForeignKey("Role")]
    public string RoleCode { get; set; } = String.Empty;

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
    public virtual FacilityUser FacilityUser { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}


