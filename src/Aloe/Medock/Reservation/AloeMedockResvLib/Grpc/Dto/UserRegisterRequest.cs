using MessagePack;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;

[MessagePackObject]
public class UserRegisterRequest
{
    [Key(0)]
    public required Guid SessionId { get; set; }

    [Key(1)]
    public required string LoginName { get; set; }

    [Key(2)]
    public required string Email { get; set; }

    [Key(3)]
    public required string Password { get; set; }

    [Key(4)]
    public required string DisplayName { get; set; }
}
