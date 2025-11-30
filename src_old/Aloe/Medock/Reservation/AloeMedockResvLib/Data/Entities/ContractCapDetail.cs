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
/// 契約キャップ対象プラン・オプション（contract_cap_details）
/// </summary>
[Table("contract_cap_details")]
public class ContractCapDetail : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.CtCapPlanId;

    [Column("ct_cap_plan_id")]
    [Key]
    [Required]
    public int CtCapPlanId { get; set; }

    [Column("ct_id")]
    [Required]
    public int CtId { get; set; }

    [Column("ct_cap_id")]
    [Required]
    public int CtCapId { get; set; }

    [Column("plan_id")]
    [Required]
    public int PlanId { get; set; }

    [Column("opt_id")]
    [Required]
    public int OptId { get; set; }

    public ContractCapDetail() { }

    //public ContractCapDetail(string prefCode, string prefName, string prefDesc, string dataType, string prefValue)
    //{
    //    this.PrefCode = prefCode;
    //    this.PrefName = prefName;
    //    this.PrefDesc = prefDesc;
    //    this.DataType = dataType;
    //    this.PrefValue = prefValue;
    //    this.IsActive = true;
    //}
}
