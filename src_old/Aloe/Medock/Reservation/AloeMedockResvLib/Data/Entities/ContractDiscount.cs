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
/// 契約割引（contract_discounts）
/// </summary>
[Table("contract_discounts")]
public class ContractDiscount : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.CtDiscId;

    [Column("ct_disc_id")]
    [Key]
    [Required]
    public int CtDiscId { get; set; }

    [Column("ct_id")]
    [Required]
    public int CtId { get; set; }

    [Column("plan_id")]
    [Required]
    public int PlanId { get; set; }

    [Column("opt_id")]
    [Required]
    public int OptId { get; set; }

    [Column("payer_code")]
    [Required]
    public int PayerCode { get; set; }

    [Column("disc_scope_code")]
    [Required]
    public int DiscScopeCode { get; set; }

    [Column("disc_type_code")]
    [Required]
    public int DiscTypeCode { get; set; }

    [Column("disc_value")]
    [Required]
    public decimal DiscValue { get; set; }

    public ContractDiscount() { }

    //public ContractDiscount(string prefCode, string prefName, string prefDesc, string dataType, string prefValue)
    //{
    //    this.PrefCode = prefCode;
    //    this.PrefName = prefName;
    //    this.PrefDesc = prefDesc;
    //    this.DataType = dataType;
    //    this.PrefValue = prefValue;
    //    this.IsActive = true;
    //}
}
