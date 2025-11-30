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
/// 契約オプション（contract_options）
/// </summary>
[Table("contract_options")]
public class ContractOption : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.CtOptId;

    [Column("ct_opt_id")]
    [Key]
    [Required]
    public int CtOptId { get; set; }

    [Column("ct_id")]
    [Required]
    public int CtId { get; set; }

    [Column("opt_id")]
    [Required]
    public int OptId { get; set; }

    [Column("is_active")]
    [Required]
    public bool IsActive { get; set; }

    [Column("ct_opt_name")]
    [Required]
    public string CtOptName { get; set; } = String.Empty;

    [Column("ct_opt_short_name")]
    [Required]
    public string CtOptShortName { get; set; } = String.Empty;

    [Column("ct_opt_desc")]
    [Required]
    public string CtOptDesc { get; set; } = String.Empty;

    public ContractOption() { }

    public ContractOption(int ctId, CheckupOption option)
    {
        this.CtId = ctId;
        this.OptId = option.OptId;
        this.IsActive = true;
    }
}
