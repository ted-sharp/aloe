namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// 団体エンティティ
/// </summary>
public class Organization : IAuditableEntity
{
    /// <summary>団体ID (PK)</summary>
    public Guid OrgId { get; set; }

    /// <summary>テナントID (FK)</summary>
    public Guid TenantId { get; set; }

    /// <summary>施設ID (FK)</summary>
    public Guid FacilityId { get; set; }

    /// <summary>親団体ID</summary>
    public Guid? ParentOrgId { get; set; }

    /// <summary>団体コード（病院の番・法人番号）</summary>
    public string OrgCode { get; set; } = String.Empty;

    /// <summary>団体名</summary>
    public string OrgName { get; set; } = String.Empty;

    /// <summary>団体名カタカナ</summary>
    public string OrgNameKatakana { get; set; } = String.Empty;

    /// <summary>団体名カタカナ（互換）</summary>
    public string OrgNameKatakanaCompat { get; set; } = String.Empty;

    /// <summary>団体表示名</summary>
    public string OrgNameDisplay { get; set; } = String.Empty;

    /// <summary>団体印刷名</summary>
    public string OrgNamePrint { get; set; } = String.Empty;

    /// <summary>メモ</summary>
    public string OrgMemo { get; set; } = String.Empty;

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
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Facility Facility { get; set; } = null!;
    public virtual Organization? ParentOrganization { get; set; }
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<EquipmentAppointment> EquipmentAppointments { get; set; } = new List<EquipmentAppointment>();
}


