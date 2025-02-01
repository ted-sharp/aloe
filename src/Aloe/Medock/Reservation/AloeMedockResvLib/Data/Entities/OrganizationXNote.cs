using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("organization_x_note")]
[PrimaryKey(nameof(OrganizationXNote.OrgId), nameof(OrganizationXNote.NoteId))]
public class OrganizationXNote : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.NoteId;

    [Column("org_id", Order = 0)]
    [Required]
    public int OrgId { get; set; }

    [Column("note_id", Order = 1)]
    [Required]
    public int NoteId { get; set; }

    [Column("org_note_code")]
    public int OrgNoteCode { get; set; }
}
