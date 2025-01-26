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
/// 契約キャップ（contract_caps）
/// </summary>
[Table("contract_caps")]
public class ContractCap : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.CtCapId;

    [Column("ct_cap_id")]
    [Key]
    [Required]
    public int CtCapId { get; set; }

    [Column("ct_id")]
    [Required]
    public int CtId { get; set; }

    [Column("payer_code")]
    [Required]
    public int PayerCode { get; set; }

    [Column("distr_payer_code")]
    [Required]
    public int DistrPayerCode { get; set; }

    [Column("cap_amount")]
    [Required]
    public decimal CapAmount { get; set; }

    [Column("priority")]
    [Required]
    public int Priority { get; set; }

    [Column("is_active")]
    [Required]
    public bool IsActive { get; set; }

    public ContractCap() { }

    //public ContractCap(string prefCode, string prefName, string prefDesc, string dataType, string prefValue)
    //{
    //    this.PrefCode = prefCode;
    //    this.PrefName = prefName;
    //    this.PrefDesc = prefDesc;
    //    this.DataType = dataType;
    //    this.PrefValue = prefValue;
    //    this.IsActive = true;
    //}
}
