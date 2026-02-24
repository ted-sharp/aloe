namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// ユーザー設定エンティティ
/// </summary>
[Table("user_preferences")]
public class UserPreference : IAuditableEntity
{
    /// <summary>ユーザー設定ID (PK)</summary>
    [Key]
    [Column("user_preference_id")]
    public Guid UserPreferenceId { get; set; }

    /// <summary>ユーザーID (FK)</summary>
    [Column("user_id")]
    [ForeignKey("User")]
    public Guid UserId { get; set; }

    /// <summary>設定コード (FK)</summary>
    [Column("preference_code")]
    [ForeignKey("Preference")]
    [MaxLength(100)]
    public string PreferenceCode { get; set; } = String.Empty;

    /// <summary>設定値</summary>
    [Column("preference_value")]
    [MaxLength(10)]
    public string PreferenceValue { get; set; } = String.Empty;

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
    public virtual User User { get; set; } = null!;
    public virtual Preference Preference { get; set; } = null!;
}

