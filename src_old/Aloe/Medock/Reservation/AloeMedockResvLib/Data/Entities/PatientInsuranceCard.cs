using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("patient_insurance_cards")]
public class PatientInsuranceCard : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.PtInsurCardId;

    [Key]
    [Column("pt_insur_card_id")]
    [Required]
    public int PtInsurCardId { get; set; }

    [Column("pt_id")]
    public int PtId { get; set; }

    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    [Column("insur_type_code")]
    [MaxLength(Int32.MaxValue)]
    public string? InsurTypeCode { get; set; }

    [Column("insur_prov_id")]
    public int? InsurProvId { get; set; }

    [Column("insur_prov_number")]
    [MaxLength(Int32.MaxValue)]
    public string? InsurProvNumber { get; set; }

    [Column("insur_prov_name")]
    [MaxLength(Int32.MaxValue)]
    public string? InsurProvName { get; set; }

    [Column("insured_code")]
    [MaxLength(Int32.MaxValue)]
    public string? InsuredCode { get; set; }

    [Column("insured_code_symbol")]
    [MaxLength(Int32.MaxValue)]
    public string? InsuredCodeSymbol { get; set; }

    [Column("insured_code_number")]
    [MaxLength(Int32.MaxValue)]
    public string? InsuredCodeNumber { get; set; }

    [Column("insured_code_branch_number")]
    [MaxLength(Int32.MaxValue)]
    public string? InsuredCodeBranchNumber { get; set; }

    [Column("insured_person_name")]
    [MaxLength(Int32.MaxValue)]
    public string? InsuredPersonName { get; set; }

    [Column("self_family_relationship_code")]
    [MaxLength(Int32.MaxValue)]
    public string? SelfFamilyRelationshipCode { get; set; }

    [Column("assistance_code")]
    [MaxLength(Int32.MaxValue)]
    public string? AssistanceCode { get; set; }

    [Column("continuation_code")]
    [MaxLength(Int32.MaxValue)]
    public string? ContinuationCode { get; set; }

    [Column("start_date")]
    public DateTime? StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    [Column("memo")]
    [MaxLength(Int32.MaxValue)]
    public string? Memo { get; set; }

}
