namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// ユーザー設定マスタエンティティ
/// </summary>
/// <remarks>
/// FUTURE FEATURE: Role-Based Access Control (RBAC) 実装用に予約されています。
/// 現在はアクティブに使用されていません。実装計画については CLAUDE.md を参照してください。
/// </remarks>
[Table("preferences")]
public class Preference : IAuditableEntity
{
    /// <summary>設定コード (PK)</summary>
    [Key]
    [Column("preference_code")]
    [MaxLength(100)]
    public string PreferenceCode { get; set; } = String.Empty;

    /// <summary>設定名</summary>
    [Column("preference_name")]
    [MaxLength(100)]
    public string PreferenceName { get; set; } = String.Empty;

    /// <summary>設定説明</summary>
    [Column("preference_desc")]
    [MaxLength(1000)]
    public string PreferenceDesc { get; set; } = String.Empty;

    /// <summary>データ型</summary>
    [Column("data_type")]
    [MaxLength(10)]
    public string DataType { get; set; } = String.Empty;

    /// <summary>設定値</summary>
    [Column("preference_value")]
    [MaxLength(10)]
    public string PreferenceValue { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    [Column("preference_seq")]
    public int PreferenceSeq { get; set; }

    /// <summary>有効フラグ</summary>
    [Column("is_active")]
    public bool IsActive { get; set; }

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
    public virtual ICollection<UserPreference> UserPreferences { get; set; } = new List<UserPreference>();
}

