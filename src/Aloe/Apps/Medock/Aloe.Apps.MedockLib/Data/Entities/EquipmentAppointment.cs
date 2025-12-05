namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 設備予約エンティティ
/// </summary>
public class EquipmentAppointment : IAuditableEntity
{
    /// <summary>設備予約ID (PK)</summary>
    public Guid EquipApptId { get; set; }

    /// <summary>設備ID (FK)</summary>
    public Guid EquipId { get; set; }

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

    /// <summary>予約メモ</summary>
    public string ApptMemo { get; set; } = String.Empty;

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
    public virtual Equipment Equipment { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
    public virtual Patient Patient { get; set; } = null!;
}
