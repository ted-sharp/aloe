namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 部屋エンティティ
/// </summary>
public class Room : IAuditableEntity
{
    /// <summary>部屋ID (PK)</summary>
    public Guid RoomId { get; set; }

    /// <summary>フロアID (FK)</summary>
    public Guid FloorId { get; set; }

    /// <summary>部屋名</summary>
    public string RoomName { get; set; } = string.Empty;

    /// <summary>部屋説明</summary>
    public string RoomDesc { get; set; } = string.Empty;

    /// <summary>表示順</summary>
    public int RoomSeq { get; set; }

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
    public virtual Floor Floor { get; set; } = null!;
}

