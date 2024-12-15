using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;

public enum ScreenCode
{
    None = 0,
    Tray = 1,
    Login = 2,
    ReservationMain = 3,
    ReservationEquip = 4,
    ReservationEquipBooking = 5,
    ReservationDaily = 6,
    ReservationDailyBooking = 7,
}

public static class ScreenCodeExtensions
{
    public static bool IsDefault(this ScreenCode screenCode) => screenCode switch
    {
        ScreenCode.None or ScreenCode.Tray or ScreenCode.Login => true,
        _ => false,
    };
}
