namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 操作エンティティ
/// </summary>
/// <remarks>
/// FUTURE FEATURE: Role-Based Access Control (RBAC) 実装用に予約されています。
/// 現在はアクティブに使用されていません。実装計画については CLAUDE.md を参照してください。
/// </remarks>
[Table("operations")]
public class Operation : IAuditableEntity
{
    /// <summary>操作コード (PK)</summary>
    [Key]
    [Column("operation_code")]
    [MaxLength(10)]
    public string OperationCode { get; set; } = String.Empty;

    /// <summary>操作名</summary>
    [Column("operation_name")]
    [MaxLength(100)]
    public string OperationName { get; set; } = String.Empty;

    /// <summary>操作説明</summary>
    [Column("operation_desc")]
    [MaxLength(1000)]
    public string OperationDesc { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    [Column("operation_seq")]
    public int OperationSeq { get; set; }

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
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
