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
/// 契約（contracts）
/// </summary>
[Table("contracts")]
public class Contract : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.CtId;

    [Column("ct_id")]
    [Key]
    [Required]
    public int CtId { get; set; }

    [Column("insur_prov_id")]
    [Required]
    public int InsurProvId { get; set; }

    [Column("org_id")]
    [Required]
    public int OrgId { get; set; }

    [Column("parent_ct_code")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string ParentCtCode { get; set; } = String.Empty;

    [Column("ct_code")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string CtCode { get; set; } = String.Empty;

    [Column("ct_rev")]
    [Required]
    public int CtRev { get; set; }

    [Column("ct_name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string CtName { get; set; } = String.Empty;

    [Column("ct_desc")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string CtDesc { get; set; } = String.Empty;

    [Column("start_date")]
    [Required]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    [Required]
    public DateTime? EndDate { get; set; }

    [Column("is_tax_including")]
    [Required]
    public bool IsTaxIncluding { get; set; }

    [Column("rounding_code")]
    [Required]
    public int RoundingCode { get; set; }

    [Column("rounding_scope_code")]
    [Required]
    public int RoundingScopeCode { get; set; }

    public Contract() { }

    public Contract(int insurProvId, int orgId, string parentCtCode, string code, string name, string desc)
    {
        this.InsurProvId = insurProvId;
        this.OrgId = orgId;
        this.ParentCtCode = parentCtCode;
        this.CtCode = code;
        this.CtName = name;
        this.CtDesc = desc;
        this.StartDate = DateTime.Today;
    }
}
