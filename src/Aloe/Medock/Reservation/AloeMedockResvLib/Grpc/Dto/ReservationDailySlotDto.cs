using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;

/// <summary>
/// 日次予約枠
/// </summary>
[MessagePackObject]
public class ReservationDailySlotDto
{
    [Key(0)]
    public required int ResvDailySlotId { get; set; }

    [Key(1)]
    public required DateOnly StartDate { get; set; }

    [Key(2)]
    public required DateOnly? EndDate { get; set; }

    [Key(3)]
    public required int DowCode { get; set; }

    [Key(4)]
    public required int FloorId { get; set; }

    [Key(5)]
    public required string Slots { get; set; }
}
