using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("patient_remarks")]
public class PatientRemark : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.PtRemarkId;

    [Key]
    [Column("pt_remark_id")]
    [Required]
    public int PtRemarkId { get; set; }

    [Column("pt_id")]
    public int PtId { get; set; }

    [Column("pt_remark_code")]
    public string? PtRemarkCode { get; set; }

    [Column("pt_remark_text")]
    public string? PtRemarkText { get; set; }

    [Column("updated_user_name")]
    public string? UpdatedUserName { get; set; }
}
