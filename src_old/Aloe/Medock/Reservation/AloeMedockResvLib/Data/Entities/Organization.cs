using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
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

    [Column("insur_prov_id")]
    [Required]
    public int InsurProvId { get; set; } = 0;

    [Column("insur_prov_type_code")]
    [Required]
    public int InsurProvTypeCode { get; set; }

    [Column("parent_org_id")]
    [Required]
    public int ParentOrgId { get; set; } = 0;

    [Column("org_number")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string OrgNumber { get; set; } = String.Empty;

    [Column("org_name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string OrgName { get; set; } = String.Empty;

    [Column("org_name_katakana")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string OrgNameKatakana { get; set; } = String.Empty;

    [Column("org_name_katakana_normalized")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string OrgNameKatakanaNormalized { get; set; } = String.Empty;

    [Column("org_name_display")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string OrgNameDisplay { get; set; } = String.Empty;

    [Column("org_name_print")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string OrgNamePrint { get; set; } = String.Empty;

    [Column("memo")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string Memo { get; set; } = String.Empty;

    public Organization() { }

    public Organization(int insurProvTypeCode, int insurProvId, string orgName, string katakana, string displayName, string printName)
    {
        this.InsurProvTypeCode = insurProvTypeCode;
        this.InsurProvId = insurProvId;
        this.OrgName = orgName;
        this.OrgNameKatakana = katakana;
        this.OrgNameDisplay = displayName;
        this.OrgNamePrint = printName;
    }
}
