using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;



[Table("organization_contacts")]
public class OrganizationContact : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.OrgCtcId;

    [Key]
    [Column("org_ctc_id")]
    [Required]
    public int OrgCtcId { get; set; }

    [Column("org_id")]
    public int OrgId { get; set; }

    [Column("ctc_name")]
    [MaxLength(Int32.MaxValue)]
    public string CtcName { get; set; } = "";

    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    [Column("is_form_shipping")]
    public bool IsFormShipping { get; set; }

    [Column("is_result_shipping")]
    public bool IsResultShipping { get; set; }

    [Column("is_billing")]
    public bool IsBilling { get; set; }

    [Column("zip_code")]
    [MaxLength(Int32.MaxValue)]
    public string ZipCode { get; set; } = "";

    [Column("adr1")]
    [MaxLength(Int32.MaxValue)]
    public string Adr1 { get; set; } = "";

    [Column("adr2")]
    [MaxLength(Int32.MaxValue)]
    public string Adr2 { get; set; } = "";

    [Column("adr3")]
    [MaxLength(Int32.MaxValue)]
    public string Adr3 { get; set; } = "";

    [Column("recipient_name")]
    [MaxLength(Int32.MaxValue)]
    public string RecipientName { get; set; } = "";

    [Column("honorific")]
    [MaxLength(Int32.MaxValue)]
    public string Honorific { get; set; } = "";

    [Column("tel")]
    [MaxLength(Int32.MaxValue)]
    public string Tel { get; set; } = "";

    [Column("tel2")]
    [MaxLength(Int32.MaxValue)]
    public string Tel2 { get; set; } = "";

    [Column("fax")]
    [MaxLength(Int32.MaxValue)]
    public string Fax { get; set; } = "";

    [Column("email")]
    [MaxLength(Int32.MaxValue)]
    public string Email { get; set; } = "";

    [Column("memo")]
    [MaxLength(Int32.MaxValue)]
    public string Memo { get; set; } = "";
}
