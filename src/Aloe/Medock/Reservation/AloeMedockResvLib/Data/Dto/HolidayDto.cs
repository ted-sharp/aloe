using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using MessagePack;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;

[MessagePackObject]
public class HolidayDto
{
    [Key(0)]
    public required DateOnly HolidayDate { get; set; }

    [Key(1)]
    public required string HolidayName { get; set; }
}

public static class HolidayExtensions
{
    public static HolidayDto ToHolidayDto(this Holiday holiday)
    {
        return new HolidayDto
        {
            // EFCore の関係でエンティティに DateOnly 型は使わないものとする。
            HolidayDate = DateOnly.FromDateTime(holiday.HolidayDate),
            HolidayName = holiday.HolidayName,
        };
    }
}
