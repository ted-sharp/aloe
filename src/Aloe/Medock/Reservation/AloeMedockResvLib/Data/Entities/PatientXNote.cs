using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("patient_x_note")]
[PrimaryKey(nameof(PatientXNote.OrgId), nameof(PatientXNote.NoteId))]
public class PatientXNote : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.NoteId;

    [Column("org_id", Order = 0)]
    [Required]
    public int OrgId { get; set; }

    [Column("note_id", Order = 1)]
    [Required]
    public int NoteId { get; set; }

    [Column("pt_note_code")]
    public int PtNoteCode { get; set; }
}
