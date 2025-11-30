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
/// ルーム詳細
/// </summary>
[MessagePackObject]
public class ReservationRoomDetailDto
{
    [Key(0)]
    public required int RoomId { get; set; }

    [Key(1)]
    public required int ExamId { get; set; }
}

public static class ReservationRoomDetailExtensions
{
    public static ReservationRoomDetailDto ToReservationRoomDto(this ReservationRoomDetail detail)
    {
        return new ReservationRoomDetailDto
        {
            RoomId = detail.RoomId,
            ExamId = detail.ExamId,
        };
    }
}
