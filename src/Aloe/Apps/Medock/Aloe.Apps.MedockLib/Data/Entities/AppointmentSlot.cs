namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 予約スロット定義エンティティ
/// フロアごとの予約可能時間帯をJSONBで保持
/// </summary>
public class AppointmentSlot : IAuditableEntity
{
    /// <summary>予約スロットID (PK)</summary>
    public Guid ApptSlotId { get; set; }

    /// <summary>フロアID (FK)</summary>
    public Guid FloorId { get; set; }

    /// <summary>
    /// 予約スロット定義（JSONB）
    /// 例: { "slots": [{ "time": "08:00", "max": 5, "duration": 60 }, ...] }
    /// </summary>
    public string ApptSlots { get; set; } = "{}";

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
    public virtual Floor Floor { get; set; } = null!;
}

