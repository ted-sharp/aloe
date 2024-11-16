using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;

/// <summary>
/// ユーザー登録を試行した結果を返します。
/// </summary>
[MessagePackObject]
public class UserRegisterResult
{
    [Key(0)]
    public required bool IsSuccess { get; set; }

    [Key(2)]
    public required string? ErrorMessage { get; set; }

    [Key(3)]
    public required UserDto? UserDto { get; set; }
}
