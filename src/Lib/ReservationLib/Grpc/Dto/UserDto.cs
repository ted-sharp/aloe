using AloeReservationGrid.Lib.ReservationLib.Data.Entities;
using MessagePack;

namespace AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;

[MessagePackObject]
public class UserDto
{
    [Key(0)]
    public required int UserId { get; set; }

    [Key(1)]
    public required string DisplayName { get; set; }
}

public static class UserExtensions
{
    public static UserDto ToUserDto(this User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            DisplayName = user.DisplayName,
        };
    }
}
