using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;

/// <summary>
/// ログイン要求のパラメータです。
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

    /// <summary>
    /// gRPCのサーバー側でPeer名が補完されます。
    /// </summary>
    [Key(3)]
    public string ClientEndpoint { get; set; } = "";
}
