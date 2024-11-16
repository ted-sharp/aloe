using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace AloeReservationGrid.Lib.ReservationLib.Grpc.Services;

public interface ICooperationHub : IStreamingHub<ICooperationHub, ICooperationHubReceiver>
{
    ValueTask<Player[]> JoinAsync(string roomName, string userName);

    ValueTask LeaveAsync();
}

public interface ICooperationHubReceiver
{
    void OnJoin(Player player);

    void OnLeave(Player player);
}

[MessagePackObject]
public class Player
{
    [Key(0)]
    public string Name { get; set; } = "";
}
