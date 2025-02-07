using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("customer_notes")]
public class CustomerNote : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.CustNoteId;

    [Key]
    [Column("cust_note_id")]
    [Required]
    public int CustNoteId { get; set; }

    [Column("org_id")]
    [Required]
    public int OrgId { get; set; }

    [Column("pt_id")]
    [Required]
    public int PtId { get; set; }

    [Column("note_text")]
    [MaxLength(Int32.MaxValue)]
    public string NoteText { get; set; } = String.Empty;

    [Column("updated_user_name")]
    [MaxLength(Int32.MaxValue)]
    public string UpdatedUserName { get; set; } = String.Empty;
}
