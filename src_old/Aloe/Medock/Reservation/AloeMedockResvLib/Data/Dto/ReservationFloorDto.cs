using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;

/// <summary>
/// フロア
/// </summary>
[MessagePackObject]
public class ReservationFloorDto
{
    [Key(0)]
    public required int FloorId { get; set; }

    [Key(1)]
    public required string FloorCode { get; set; }

    [Key(2)]
    public required string FloorName { get; set; }

    [Key(3)]
    public required int Seq { get; set; }
}

public static class ReservationFloorExtensions
{
    public static ReservationFloorDto ToReservationFloorDto(this ReservationFloor floor)
    {
        return new ReservationFloorDto
        {
            FloorId = floor.FloorId,
            FloorCode = floor.FloorCode,
            FloorName = floor.FloorName,
            Seq = floor.Seq,
        };
    }
}
