using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Defaults;

public static class DefaultPreference
{

    public static Dictionary<string, Preference> CreateDefaultPreferences()
    {
        var policies = new Dictionary<string, Preference>
        {
            [PreferenceCode.WindowRememberPosition] = new()
            {
                PrefCode = PreferenceCode.WindowRememberPosition,
                PrefName = nameof(PreferenceCode.WindowRememberPosition),
                PrefDesc = "Window ポジションを記憶します。",
                DataType = Constants.DataType.String,
                PrefValue = "",
                IsActive = true,
            },
        };

        return policies;
    }

}
