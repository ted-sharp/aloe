using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace AloeReservationGrid.Lib.ReservationLib.Grpc.Services;

public interface IMyFirstService : IService<IMyFirstService>
{
    // The return type must be `UnaryResult<T>` or `UnaryResult`.
    UnaryResult<int> SumAsync(int x, int y);
}

// TODO: streaming で接続して、誰がどこを選択しているか見たい

// Activeなのはどの画面か？
// Floor, Room, Equipment あたり？
// Org, Pt なども同じ人を開いていたらわかったら嬉しいかも？

// 全体メッセージなども流せる
// マスタに変更があった場合も変更を通知できそう

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
