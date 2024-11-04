using AloeReservationGrid.Lib.CoreLib.Interfaces;

namespace AloeReservationGrid.Api.ReservationServer.Uuid;

public class PostgreSqlUuidGenerator : IUuidGenerator
{
    public Guid NewGuid()
    {
        return UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql);
    }
}
