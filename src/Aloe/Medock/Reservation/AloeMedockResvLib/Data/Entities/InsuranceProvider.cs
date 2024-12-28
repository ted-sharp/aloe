using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("insurance_providers")]
public class InsuranceProvider : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.InsurProvId;

    [Key]
    [Column("insur_prov_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int InsurProvId { get; set; }

    [Column("insur_prov_type_code")]
    [Required]
    public int InsurProvTypeCode { get; set; }

    [Column("insur_prov_number")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string InsurProvNumber { get; set; } = String.Empty;

    [Column("insur_prov_name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string InsurProvName { get; set; } = String.Empty;

    [Column("memo")]
    [MaxLength(Int32.MaxValue)]
    public string? Memo { get; set; }

    public InsuranceProvider() { }

    public InsuranceProvider(int insurProvTypeCode, string insurProvNumber, string insurProvName, string memo)
    {
        this.InsurProvTypeCode = insurProvTypeCode;
        this.InsurProvNumber = insurProvNumber;
        this.InsurProvName = insurProvName;
        this.Memo = memo;
    }
}
