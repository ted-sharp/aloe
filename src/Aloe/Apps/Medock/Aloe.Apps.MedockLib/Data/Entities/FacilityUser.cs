namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 施設ユーザー（施設とユーザーの関連）エンティティ
/// </summary>
[Table("facility_users")]
public class FacilityUser : IAuditableEntity
{
    /// <summary>施設ユーザーID (PK)</summary>
    [Key]
    [Column("facility_user_id")]
    public Guid FacilityUserId { get; set; }

    /// <summary>施設ID (FK)</summary>
    [Column("facility_id")]
    [ForeignKey("Facility")]
    public Guid FacilityId { get; set; }

    /// <summary>ユーザーID (FK)</summary>
    [Column("user_id")]
    [ForeignKey("User")]
    public Guid UserId { get; set; }

    /// <summary>表示名</summary>
    [Column("display_name")]
    [MaxLength(100)]
    public string DisplayName { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    [Column("facility_user_seq")]
    public int FacilityUserSeq { get; set; }

    /// <summary>施設管理者フラグ</summary>
    [Column("is_facility_admin")]
    public bool IsFacilityAdmin { get; set; }

    /// <summary>権限レベル (full, readonly, limited)</summary>
    [Column("permission_level")]
    [MaxLength(20)]
    public string PermissionLevel { get; set; } = "full";

    /// <summary>削除フラグ</summary>
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    // IAuditableEntity
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    [Column("created_user_id")]
    public Guid CreatedUserId { get; set; }
    [Column("created_session_id")]
    public Guid CreatedSessionId { get; set; }
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
    [Column("updated_user_id")]
    public Guid UpdatedUserId { get; set; }
    [Column("updated_session_id")]
    public Guid UpdatedSessionId { get; set; }

    // Navigation Properties
    public virtual Facility Facility { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
