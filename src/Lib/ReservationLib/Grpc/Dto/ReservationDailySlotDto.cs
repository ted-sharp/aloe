using AloeReservationGrid.Lib.ReservationLib.Data.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;

/// <summary>
/// 日次予約枠
/// </summary>
[MessagePackObject]
public class ReservationDailySlotDto
{
    [Key(0)]
    public required int ResvDailySlotId { get; set; }

    [Key(1)]
    public required DateTime StartDate { get; set; }

    [Key(2)]
    public required DateTime EndDate { get; set; }

    [Key(3)]
    public required int DowCode { get; set; }

    [Key(4)]
    public required int FloorId { get; set; }

    [Key(5)]
    public required int RoomId { get; set; }

    [Key(6)]
    public required int DailyCap { get; set; }

    [Key(7)]
    public required int AmCap { get; set; }

    [Key(8)]
    public required int PmCap { get; set; }

    [Key(9)]
    public required string Slots { get; set; }

    [Key(10)]
    public required string SlotCaps { get; set; }
}
