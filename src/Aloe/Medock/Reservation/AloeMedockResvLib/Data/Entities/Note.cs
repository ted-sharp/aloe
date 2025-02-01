using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("notes")]
public class Note : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.NoteId;

    [Key]
    [Column("note_id")]
    [Required]
    public int NoteId { get; set; }

    [Column("note_text")]
    public string? NoteText { get; set; }

    [Column("updated_user_name")]
    public string? UpdatedUserName { get; set; }
}
