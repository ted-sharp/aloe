using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

namespace AloeReservationGrid.Lib.ReservationLib.Rpc;

// TODO: どんな処理があればよいか？

// 起動時にマスタデータをある程度キャッシュしておきたい
// キャッシュの仕組みはどうする？
// ログイン時にキャッシュを更新する処理をいれておけば、ログインしなおしでよいかも？

//ログイン、ログアウト
// 頻度が少ない処理は単方向でよさそう
//public Session Login(string user, string pwd, recv)
//public void Logout(Session session, recv)
//マスタデータの更新なども




// それぞれのテーブルのデータの取得
// 何度もやるので双方向で維持しておきたい
// reservation_equipments
// reservation_equipment_slots
// reservation_equipment_bookings
// 予約データの更新




//予約情報の取得
//public ReservationSummary[] FetchReservationSummaries(DateTime date, int floorId1, int floorId2)




public interface IMyFirstService : IService<IMyFirstService>
{
    // The return type must be `UnaryResult<T>` or `UnaryResult`.
    UnaryResult<int> SumAsync(int x, int y);
}

// Server -> Client definition
public interface IGamingHubReceiver
{
    // The method must have a return type of `void` and can have up to 15 parameters of any type.
    void OnJoin(Player player);
    void OnLeave(Player player);
}

// Client -> Server definition
// implements `IStreamingHub<TSelf, TReceiver>`  and share this type between server and client.
public interface IGamingHub : IStreamingHub<IGamingHub, IGamingHubReceiver>, IService<IGamingHub>
{
    // The method must return `ValueTask`, `ValueTask<T>`, `Task` or `Task<T>` and can have up to 15 parameters of any type.
    ValueTask<Player[]> JoinAsync(string roomName, string userName);
    ValueTask LeaveAsync();
}

// for example, request object by MessagePack.
[MessagePackObject]
public class Player
{
    [Key(0)]
    public string Name { get; set; }
}
