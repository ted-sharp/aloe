using AloeReservationGrid.Lib.ReservationLib.Rpc;
using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using System.Numerics;
using System.Text.RegularExpressions;

namespace AloeReservationGrid.Api.ReservationServer.Rpc;

// Implements RPC service in the server project.
// The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
{
    // `UnaryResult<T>` allows the method to be treated as `async` method.
    public async UnaryResult<int> SumAsync(int x, int y)
    {
        Console.WriteLine($"Received:{x}, {y}");
        return x + y;
    }
}




// Server implementation
// implements : StreamingHubBase<THub, TReceiver>, THub
public class GamingHub : StreamingHubBase<IGamingHub, IGamingHubReceiver>, IGamingHub
{
    // this class is instantiated per connected so fields are cache area of connection.
    IGroup room;
    Player self;
    IInMemoryStorage<Player> storage;

    public async ValueTask<Player[]> JoinAsync(string roomName, string userName)
    {
        this.self = new Player() { Name = userName };

        // Group can bundle many connections and it has inmemory-storage so add any type per group.
        (this.room, this.storage) = await this.Group.AddAsync(roomName, this.self);

        // Typed Server->Client broadcast.
        this.Broadcast(this.room).OnJoin(this.self);

        return this.storage.AllValues.ToArray();
    }

    public async ValueTask LeaveAsync()
    {
        await this.room.RemoveAsync(this.Context);
        this.Broadcast(this.room).OnLeave(this.self);
    }

    // You can hook OnConnecting/OnDisconnected by override.
    protected override ValueTask OnDisconnected()
    {
        // on disconnecting, if automatically removed this connection from group.
        return ValueTask.CompletedTask;
    }
}


