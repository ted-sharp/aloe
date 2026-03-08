namespace Aloe.Utils.Drawing.Samples.Assets;

public static class Icons
{
    public static Lazy<Icon> Aloe = new(() => Images.Aloe.Value.ToIcon());
}
