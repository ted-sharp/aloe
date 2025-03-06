using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;

/// <summary>
/// 日次備考
/// </summary>
[MessagePackObject]
public class ReservationDailyNoteDto
{
    [Key(0)]
    public required int ResvDailyNoteId { get; set; }

    [Key(1)]
    public required DateOnly BkgDate { get; set; }

    [Key(2)]
    public required int FloorId { get; set; }

    [Key(4)]
    public required string NoteText { get; set; }

    [Key(5)]
    public required DateTime UpdatedAt { get; set; }

    [Key(6)]
    public required string UpdatedUserName { get; set; }
}

public static class ReservationDailyNoteExtensions
{
    public static ReservationDailyNoteDto ToReservationDailyNoteDto(this ReservationDailyNote note)
    {
        return new ReservationDailyNoteDto
        {
            ResvDailyNoteId = note.ResvDailyNoteId,
            BkgDate = note.BkgDate,
            FloorId = note.FloorId,
            NoteText = note.NoteText,
            UpdatedAt = note.UpdatedAt,
            UpdatedUserName = note.UpdatedUserName,
        };
    }
}
