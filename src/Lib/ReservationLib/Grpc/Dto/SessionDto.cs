using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;

namespace AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;

[MessagePackObject]
public class SessionDto
{
    [Key(0)]
    public required Guid SessionId { get; set; }

    [Key(1)]
    public required int UserId { get; set; }

    [Key(2)]
    public required string UserDisplayName { get; set; }
}
