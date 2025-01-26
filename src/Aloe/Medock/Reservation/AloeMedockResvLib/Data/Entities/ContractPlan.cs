using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
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
/// 契約プラン（contract_plans）
/// </summary>
[Table("contract_plans")]
public class ContractPlan : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.CtPlanId;

    [Column("ct_plan_id")]
    [Key]
    [Required]
    public int CtPlanId { get; set; }

    [Column("ct_id")]
    [Required]
    public int CtId { get; set; }

    [Column("plan_id")]
    [Required]
    public int PlanId { get; set; }

    [Column("is_active")]
    [Required]
    public bool IsActive { get; set; }

    [Column("ct_plan_name")]
    [Required]
    public string CtPlanName { get; set; } = String.Empty;

    [Column("ct_plan_short_name")]
    [Required]
    public string CtPlanShortName { get; set; } = String.Empty;

    [Column("ct_plan_desc")]
    [Required]
    public string CtPlanDesc { get; set; } = String.Empty;

    public ContractPlan() { }

    public ContractPlan(int ctId, CheckupPlan plan)
    {
        this.CtId = ctId;
        this.PlanId = plan.PlanId;
        this.IsActive = true;
    }
}
