using System.Drawing;
using Aloe.Common.AloeCoreLib.Win32;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Assets;

public static class Icons
{
    public static readonly Dictionary<string, Lazy<Icon>> ByName = new()
    {
        ["Calendar"] = new(() => Images.Get("Calendar").ToIcon()),
    };

    public static Icon Get(string key) => Icons.ByName[key].Value;
}
