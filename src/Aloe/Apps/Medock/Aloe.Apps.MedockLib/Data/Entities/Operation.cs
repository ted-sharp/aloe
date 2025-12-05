namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 操作エンティティ
/// </summary>
public class Operation : IAuditableEntity
{
    /// <summary>操作コード (PK)</summary>
    public string OperationCode { get; set; } = string.Empty;

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
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
