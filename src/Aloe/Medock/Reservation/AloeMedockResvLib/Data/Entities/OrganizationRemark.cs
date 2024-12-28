using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("organization_remarks")]
public class OrganizationRemark : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.OrgRemarkId;

    [Key]
    [Column("org_remark_id")]
    [Required]
    public int OrgRemarkId { get; set; }

    [Column("org_id")]
    public int OrgId { get; set; }

    [Column("org_remark_code")]
    public string? OrgRemarkCode { get; set; }

    [Column("org_remark_text")]
    public string? OrgRemarkText { get; set; }

    [Column("updated_user_name")]
    public string? UpdatedUserName { get; set; }
}
