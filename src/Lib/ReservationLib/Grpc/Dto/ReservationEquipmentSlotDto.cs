using AloeReservationGrid.Lib.ReservationLib.Data.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;

/// <summary>
/// 設備予約枠
/// </summary>
[MessagePackObject]
public class ReservationEquipmentSlotDto
{
    public static readonly string SlotDelimiter = " ";

    [Key(0)]
    public required int ResvEquipSlotId { get; set; }

    [Key(1)]
    public required DateTime StartDate { get; set; }

    [Key(2)]
    public required DateTime EndDate { get; set; }

    [Key(3)]
    public required int DowCode { get; set; }

    [Key(4)]
    public required int EquipId { get; set; }

    [Key(5)]
    public required string[] Slots { get; set; }
}
