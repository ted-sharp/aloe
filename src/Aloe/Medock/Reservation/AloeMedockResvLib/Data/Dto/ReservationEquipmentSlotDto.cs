using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;

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
    public required DateOnly StartDate { get; set; }

    [Key(2)]
    public DateOnly? EndDate { get; set; }

    [Key(3)]
    public required int DowCode { get; set; }

    [Key(4)]
    public required int EquipId { get; set; }

    [Key(5)]
    public required string[] Slots { get; set; }
}
