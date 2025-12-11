namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// リソースエンティティ
/// </summary>
[Table("resources")]
public class Resource : IAuditableEntity
{
    /// <summary>リソースコード (PK)</summary>
    [Key]
    [Column("resource_code")]
    [MaxLength(10)]
    public string ResourceCode { get; set; } = String.Empty;

    /// <summary>リソース名</summary>
    [Column("resource_name")]
    [MaxLength(100)]
    public string ResourceName { get; set; } = String.Empty;

    /// <summary>リソース説明</summary>
    [Column("resource_desc")]
    [MaxLength(1000)]
    public string ResourceDesc { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    [Column("resource_seq")]
    public int ResourceSeq { get; set; }

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
