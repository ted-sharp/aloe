namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 予約エンティティ
/// </summary>
public class Appointment : IAuditableEntity
{
    /// <summary>予約ID (PK)</summary>
    public Guid ApptId { get; set; }

    /// <summary>フロアID (FK)</summary>
    public Guid FloorId { get; set; }

    /// <summary>団体ID (FK)</summary>
    public Guid OrgId { get; set; }

    /// <summary>患者ID (FK)</summary>
    public Guid PtId { get; set; }

    /// <summary>予約日（日付のみ予約がある）</summary>
    public DateOnly? ApptDate { get; set; }

    /// <summary>予約開始日時（時間枠予約がある）</summary>
    public DateTime? ApptStartAt { get; set; }

    /// <summary>予約終了日時（時間枠予約がある）</summary>
    public DateTime? ApptEndAt { get; set; }

    /// <summary>予約ステータスコード（仮置、予約、来院済み、診察完了、キャンセル、医師キャンセル）</summary>
    public int ApptStatusCode { get; set; }

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
    public virtual Organization Organization { get; set; } = null!;
    public virtual Patient Patient { get; set; } = null!;
}

