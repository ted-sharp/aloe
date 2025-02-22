using Aloe.Common.AloeCoreLib.Util;
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
/// 標準税率マスタ（default_tax_rates）
/// </summary>
[Table("default_tax_rates")]
public class DefaultTaxRate : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.TaxRateId;

    [Column("tax_rate_id")]
    [Key]
    [Required]
    public int TaxRateId { get; set; }

    [Column("tax_rate_name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string TaxRateName { get; set; } = String.Empty;

    [Column("tax_rate")]
    [Required]
    public decimal TaxRate { get; set; }

    [Column("rounding_code")]
    [Required]
    public int RoundingCode { get; set; }

    [Column("rounding_scope_code")]
    [Required]
    public int RoundingScopeCode { get; set; }

    [Column("start_date", TypeName = "Date")]
    [Required]
    public DateOnly StartDate { get; set; } = DateOnlyHelper.GetToday();

    [Column("end_date", TypeName = "Date")]
    [Required]
    public DateOnly EndDate { get; set; }

    [Column("memo")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string Memo { get; set; } = String.Empty;

    public DefaultTaxRate() { }

    //public DefaultTaxRate(string prefCode, string prefName, string prefDesc, string dataType, string prefValue)
    //{
    //    this.PrefCode = prefCode;
    //    this.PrefName = prefName;
    //    this.PrefDesc = prefDesc;
    //    this.DataType = dataType;
    //    this.PrefValue = prefValue;
    //    this.IsActive = true;
    //}
}
