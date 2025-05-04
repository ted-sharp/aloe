using Aloe.Common.AloeCoreLib.Win32;
using System.Drawing;
using Aloe.Common.AloeCoreLib.Util;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Assets;

public static class Images
{
    public static readonly Dictionary<string, Lazy<Image>> ByName = new()
    {
        ["Calendar"] = new(() => AssetHelper.LoadImageWithoutLock(
            PathHelper.FromBase(@"Assets\MaterialIcons\calendar_month_32dp_EA33F7_FILL0_wght400_GRAD0_opsz40.png")
        )),
    };

    public static Image Get(string key) => Images.ByName[key].Value;
}
