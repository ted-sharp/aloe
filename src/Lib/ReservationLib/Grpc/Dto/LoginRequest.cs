using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;

/// <summary>
/// ログインを試行した結果を返します。
/// </summary>
[MessagePackObject]
public class LoginRequest
{
    [Key(0)]
    public required string LoginName { get; set; }

    [Key(1)]
    public required string Password { get; set; }

    [Key(2)]
    public required string ClientAppName { get; set; }
}
