using System.Drawing;

namespace Aloe.Utils.Drawing.Wpf.Samples.Assets;

public static class Icons
{
    public static Lazy<Icon> Aloe = new(() => Images.Aloe.Value.ToIcon());
}
