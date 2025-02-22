using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("reservation_daily_notes")]
public class ReservationDailyNote : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.ResvDailyNoteId;

    [Key]
    [Column("resv_daily_note_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ResvDailyNoteId { get; set; }

    [Column("bkg_date", TypeName = "Date")]
    [Required]
    public DateOnly BkgDate { get; set; } = DateOnlyHelper.GetToday();

    [Column("floor_id")]
    [Required]
    public int FloorId { get; set; } = 0;

    [Column("note_text")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string NoteText { get; set; } = String.Empty;

    [Column("updated_user_name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string UpdatedUserName { get; set; } = String.Empty;

    public ReservationDailyNote() { }

    public ReservationDailyNote(
        DateOnly date,
        int floorId,
        string text,
        string userName)
    {
        this.BkgDate = date;
        this.FloorId = floorId;
        this.NoteText = text;
        this.UpdatedUserName = userName;
    }
}
