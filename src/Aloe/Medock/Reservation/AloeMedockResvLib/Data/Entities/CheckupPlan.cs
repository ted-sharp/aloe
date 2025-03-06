using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Common.AloeCoreLib.Util;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

/// <summary>
/// 健診プラン（checkup_plans）
/// </summary>
[Table("checkup_plans")]
public class CheckupPlan : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.PlanId;

    [Column("plan_id")]
    [Key]
    [Required]
    public int PlanId { get; set; }

    [Column("plan_cat_id")]
    [Required]
    public int PlanCatId { get; set; }

    [Column("insur_prov_id")]
    [Required]
    public int InsurProvId { get; set; }

    [Column("org_id")]
    [Required]
    public int OrgId { get; set; }

    [Column("plan_code")]
    [Required]
    public string PlanCode { get; set; } = String.Empty;

    [Column("plan_name")]
    [Required]
    public string Name { get; set; } = String.Empty;

    [Column("plan_short_name")]
    [Required]
    public string ShortName { get; set; } = String.Empty;

    [Column("plan_abbr_name")]
    [Required]
    public string AbbrName { get; set; } = String.Empty;

    [Column("plan_desc")]
    [Required]
    public string PlanDesc { get; set; } = String.Empty;

    [Column("is_active")]
    [Required]
    public bool IsActive { get; set; }

    [Column("start_date", TypeName = "Date")]
    [Required]
    public DateOnly StartDate { get; set; } = DateOnlyHelper.GetToday();

    [Column("end_date", TypeName = "Date")]
    public DateOnly? EndDate { get; set; }

    public CheckupPlan() { }

    public CheckupPlan(int catId, int insurProvId, int orgId, string code, string name, string shortName, string abbrName, string desc)
    {
        this.PlanCatId = catId;
        this.InsurProvId = insurProvId;
        this.OrgId = orgId;
        this.PlanCode = code;
        this.Name = name;
        this.ShortName = shortName;
        this.AbbrName = abbrName;
        this.PlanDesc = desc;
        this.IsActive = true;
    }
}
