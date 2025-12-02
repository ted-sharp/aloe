namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 祝日エンティティ
/// </summary>
public class Holiday : IAuditableEntity
{
    /// <summary>祝日の日付 (PK)</summary>
    public DateOnly HolidayDate { get; set; }

    /// <summary>祝日名</summary>
    public string HolidayName { get; set; } = String.Empty;

    /// <summary>削除フラグ</summary>
    public bool IsDeleted { get; set; }

    /// <summary>作成日時</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>作成ユーザーID</summary>
    public Guid CreatedUserId { get; set; }

    /// <summary>作成セッションID</summary>
    public Guid CreatedSessionId { get; set; }

    /// <summary>更新日時</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>更新ユーザーID</summary>
    public Guid UpdatedUserId { get; set; }

    /// <summary>更新セッションID</summary>
    public Guid UpdatedSessionId { get; set; }
}

