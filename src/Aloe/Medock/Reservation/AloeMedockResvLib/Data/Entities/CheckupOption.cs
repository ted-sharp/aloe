using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

/// <summary>
/// オプション検査（checkup_options）
/// </summary>
[Table("checkup_options")]
public class CheckupOption : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.OptId;

    [Column("opt_id")]
    [Key]
    [Required]
    public int OptId { get; set; }

    [Column("insur_prov_id")]
    [Required]
    public int InsurProvId { get; set; }

    [Column("org_id")]
    [Required]
    public int OrgId { get; set; }

    [Column("opt_code")]
    [Required]
    public string OptCode { get; set; } = String.Empty;

    [Column("opt_name")]
    [Required]
    public string OptName { get; set; } = String.Empty;

    [Column("opt_short_name")]
    [Required]
    public string OptShortName { get; set; } = String.Empty;

    [Column("opt_abbr_name")]
    [Required]
    public string OptAbbrName { get; set; } = String.Empty;

    [Column("opt_desc")]
    [Required]
    public string OptDesc { get; set; } = String.Empty;

    [Column("is_active")]
    [Required]
    public bool IsActive { get; set; }

    [Column("start_date", TypeName = "Date")]
    [Required]
    public DateOnly StartDate { get; set; } = DateOnlyHelper.GetToday();

    [Column("end_date", TypeName = "Date")]
    public DateOnly? EndDate { get; set; }

    public CheckupOption() { }

    public CheckupOption(int insurProvId, int orgId, string code, string name, string shortName, string abbrName, string desc)
    {
        this.InsurProvId = insurProvId;
        this.OrgId = orgId;
        this.OptCode = code;
        this.OptName = name;
        this.OptShortName = shortName;
        this.OptAbbrName = abbrName;
        this.OptDesc = desc;
        this.IsActive = true;
    }
}
