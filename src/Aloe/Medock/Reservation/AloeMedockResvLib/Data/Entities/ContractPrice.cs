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
/// 契約価格（contract_prices）
/// </summary>
[Table("contract_prices")]
public class ContractPrice : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.CtPriceId;

    [Column("ct_price_id")]
    [Key]
    [Required]
    public int CtPriceId { get; set; }

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

    [Column("price")]
    [Required]
    public decimal Price { get; set; }

    [Column("tax_rate")]
    [Required]
    public decimal TaxRate { get; set; }

    public ContractPrice() { }

    //public ContractPrice(string prefCode, string prefName, string prefDesc, string dataType, string prefValue)
    //{
    //    this.PrefCode = prefCode;
    //    this.PrefName = prefName;
    //    this.PrefDesc = prefDesc;
    //    this.DataType = dataType;
    //    this.PrefValue = prefValue;
    //    this.IsActive = true;
    //}
}
