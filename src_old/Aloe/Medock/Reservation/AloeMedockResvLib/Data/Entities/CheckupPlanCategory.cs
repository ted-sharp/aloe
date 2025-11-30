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
/// 健診プランカテゴリ（checkup_plan_categories）
/// </summary>
[Table("checkup_plan_categories")]
public class CheckupPlanCategory : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.PlanCatId;

    [Column("plan_cat_id")]
    [Key]
    [Required]
    public int PlanCatId { get; set; }

    [Column("plan_cat_name")]
    [Required]
    public string PlanCatName { get; set; } = String.Empty;

    [Column("plan_cat_short_name")]
    [Required]
    public string PlanCatShortName { get; set; } = String.Empty;

    [Column("plan_cat_desc")]
    [Required]
    public string PlanCatDesc { get; set; } = String.Empty;

    [Column("seq")]
    [Required]
    public int Seq { get; set; } = 0;

    public CheckupPlanCategory() { }

    public CheckupPlanCategory(string catName, string shortName, string desc)
    {
        this.PlanCatName = catName;
        this.PlanCatShortName = shortName;
        this.PlanCatDesc = desc;
    }
}
