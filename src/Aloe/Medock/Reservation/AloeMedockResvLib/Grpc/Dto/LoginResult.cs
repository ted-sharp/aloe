using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;

/// <summary>
/// ログインを試行した結果を返します。
/// </summary>
[MessagePackObject]
public class LoginResult
{
    [Key(0)]
    public required bool IsSuccess { get; set; }

    [Key(1)]
    public required string? ErrorMessage { get; set; }

    [Key(2)]
    public required SessionDto? SessionDto { get; set; }

    [Key(3)]
    public required string? Host { get; set; }

}
