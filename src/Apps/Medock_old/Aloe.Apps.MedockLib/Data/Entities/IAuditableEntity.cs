namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 監査フィールドを持つエンティティのインターフェース
/// </summary>
public interface IAuditableEntity
{
    /// <summary>作成日時</summary>
    DateTime CreatedAt { get; set; }

    /// <summary>作成ユーザーID</summary>
    Guid CreatedUserId { get; set; }

    /// <summary>作成セッションID</summary>
    Guid CreatedSessionId { get; set; }

    /// <summary>更新日時</summary>
    DateTime UpdatedAt { get; set; }

    /// <summary>更新ユーザーID</summary>
    Guid UpdatedUserId { get; set; }

    /// <summary>更新セッションID</summary>
    Guid UpdatedSessionId { get; set; }
}



