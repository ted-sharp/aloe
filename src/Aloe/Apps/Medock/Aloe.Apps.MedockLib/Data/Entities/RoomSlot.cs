namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 部屋スロット定義エンティティ
/// 部屋ごとの予約可能時間帯をJSONBで保持
/// </summary>
public class RoomSlot : IAuditableEntity
{
    /// <summary>部屋スロットID (PK)</summary>
    public Guid RoomSlotId { get; set; }

    /// <summary>部屋ID (FK)</summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// 部屋スロット定義（JSONB）
    /// 例: { "slots": [{ "time": "08:00", "max": 1, "duration": 30 }, ...] }
    /// </summary>
    public string RoomSlots { get; set; } = "{}";

    /// <summary>有効フラグ</summary>
    public bool IsActive { get; set; }

    /// <summary>有効開始日</summary>
    public DateOnly ActiveFrom { get; set; }

    /// <summary>有効終了日</summary>
    public DateOnly ActiveTo { get; set; } = new DateOnly(9999, 12, 31);

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
    public virtual Room Room { get; set; } = null!;
}

