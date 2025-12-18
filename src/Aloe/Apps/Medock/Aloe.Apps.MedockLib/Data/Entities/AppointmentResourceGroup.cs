namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 予約リソースグループエンティティ
/// </summary>
[Table("appointment_resource_groups")]
public class AppointmentResourceGroup : IAuditableEntity
{
    /// <summary>予約リソースグループID (PK)</summary>
    [Key]
    [Column("appt_res_group_id")]
    public Guid ApptResGroupId { get; set; }

    /// <summary>施設ID (FK)</summary>
    [Column("facility_id")]
    [ForeignKey("Facility")]
    public Guid FacilityId { get; set; }

    /// <summary>リソースグループコード</summary>
    [Column("res_group_code")]
    [MaxLength(20)]
    public string ResGroupCode { get; set; } = String.Empty;

    /// <summary>リソースグループ名</summary>
    [Column("res_group_name")]
    [MaxLength(100)]
    public string ResGroupName { get; set; } = String.Empty;

    /// <summary>リソースグループ説明</summary>
    [Column("res_group_desc")]
    [MaxLength(1000)]
    public string ResGroupDesc { get; set; } = String.Empty;

    /// <summary>表示順</summary>
    [Column("res_group_seq")]
    public int ResGroupSeq { get; set; } = 0;

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
    public virtual Facility Facility { get; set; } = null!;
    public virtual ICollection<AppointmentResourceGroupMember> AppointmentResourceGroupMembers { get; set; } = new List<AppointmentResourceGroupMember>();
}

