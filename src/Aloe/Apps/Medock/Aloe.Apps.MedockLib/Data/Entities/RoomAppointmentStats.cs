namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 部屋予約統計エンティティ
/// 部屋別・日別の予約状況（時間帯枠ごとの予約数/最大数）をJSONBで保持
/// </summary>
public class RoomAppointmentStats : IAuditableEntity
{
    /// <summary>予約統計ID (PK)</summary>
    public Guid ApptStatId { get; set; }

    /// <summary>部屋ID (FK)</summary>
    public Guid RoomId { get; set; }

    /// <summary>予約日</summary>
    public DateOnly ApptDate { get; set; }

    /// <summary>予約数（合計）</summary>
    public int ApptCount { get; set; }

    /// <summary>予約最大数（合計）</summary>
    public int ApptMax { get; set; }

    /// <summary>
    /// 時間帯枠ごとのグラフデータ（JSONB）
    /// 例: { "slots": [{ "time": "08:00", "count": 3, "max": 5 }, ...] }
    /// </summary>
    public string ApptGraph { get; set; } = "{}";

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
