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
/// ルーム
/// </summary>
[MessagePackObject]
public class ReservationRoomDto
{
    [Key(0)]
    public required int RoomId { get; set; }

    [Key(1)]
    public required string RoomName { get; set; }

    [Key(2)]
    public required int Seq { get; set; }
}

public static class ReservationRoomExtensions
{
    public static ReservationRoomDto ToReservationRoomDto(this ReservationRoom room)
    {
        return new ReservationRoomDto
        {
            RoomId = room.RoomId,
            RoomName = room.RoomName,
            Seq = room.Seq,
        };
    }
}
