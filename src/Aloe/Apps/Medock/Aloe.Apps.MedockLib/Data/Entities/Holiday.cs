namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 祝日エンティティ
/// </summary>
[Table("holidays")]
public class Holiday : IAuditableEntity
{
    /// <summary>祝日の日付 (PK)</summary>
    [Key]
    [Column("holiday_date")]
    public DateOnly HolidayDate { get; set; }

    /// <summary>祝日名</summary>
    [Column("holiday_name")]
    public string HolidayName { get; set; } = String.Empty;

    /// <summary>削除フラグ</summary>
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    /// <summary>作成日時</summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>作成ユーザーID</summary>
    [Column("created_user_id")]
    public Guid CreatedUserId { get; set; }

    /// <summary>作成セッションID</summary>
    [Column("created_session_id")]
    public Guid CreatedSessionId { get; set; }

    /// <summary>更新日時</summary>
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>更新ユーザーID</summary>
    [Column("updated_user_id")]
    public Guid UpdatedUserId { get; set; }

    /// <summary>更新セッションID</summary>
    [Column("updated_session_id")]
    public Guid UpdatedSessionId { get; set; }
}

