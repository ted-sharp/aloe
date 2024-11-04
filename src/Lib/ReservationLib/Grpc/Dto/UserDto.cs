using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Dto;

[MessagePackObject]
public class UserDto
{
    [Key(0)]
    public required int UserId { get; set; }

    [Key(1)]
    public required string Name { get; set; }
}
