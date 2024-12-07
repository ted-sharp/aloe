using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("organizations")]
public class Organization : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.OrgId;

    [Column("org_id")]
    [Key]
    [Required]
    public int OrgId { get; set; }

    [Column("insurance_type_code")]
    [Required]
    public string InsuranceTypeCode { get; set; } = String.Empty;

    [Column("insurance_provider_id")]
    [Required]
    public int InsuranceProviderId { get; set; } = 0;

    [Column("parent_org_id")]
    [Required]
    public int ParentOrgId { get; set; } = 0;

    [Column("org_name")]
    [Required]
    public string OrgName { get; set; } = String.Empty;

    [Column("org_name_katakana")]
    [Required]
    public string OrgNameKatakana { get; set; } = String.Empty;

    [Column("org_name_katakana_normalized")]
    [Required]
    public string OrgNameKatakanaNormalized { get; set; } = String.Empty;

    [Column("org_name_display")]
    [Required]
    public string OrgNameDisplay { get; set; } = String.Empty;

    [Column("org_name_print")]
    [Required]
    public string OrgNamePrint { get; set; } = String.Empty;

    public Organization() { }

    public Organization(string orgName, string katakana, string displayName, string printName)
    {
        this.OrgName = orgName;
        this.OrgNameKatakana = katakana;
        this.OrgNameDisplay = displayName;
        this.OrgNamePrint = printName;
    }
}
