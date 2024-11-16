using AloeReservationGrid.Lib.CoreLib.Security;

namespace AloeReservationGrid.Api.ReservationServer.Uuid;

public class UuidGeneratorFriendlyPostgreSql : IUuidGenerator
{
    public Guid NewGuid()
    {
        return UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql);
    }
}
