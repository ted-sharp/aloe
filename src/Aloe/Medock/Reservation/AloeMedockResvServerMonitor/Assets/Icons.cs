using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvServerMonitor.Assets;

public static class Icons
{
    public static Lazy<Icon> CheckCircle = new(() => Images.CheckCircle.Value.ToIcon());
    public static Lazy<Icon> Close = new(() => Images.Close.Value.ToIcon());
    public static Lazy<Icon> Hourglass = new(() => Images.Hourglass.Value.ToIcon());
    public static Lazy<Icon> Pause = new(() => Images.Pause.Value.ToIcon());
    public static Lazy<Icon> DoNotDisturb = new(() => Images.DoNotDisturb.Value.ToIcon());

    // Win32 API の DestroyIcon を利用して一時的に作成したハンドルを解放します。
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    /// <summary>
    /// 既存の Image 型（実体が Bitmap として扱えるもの）から単一サイズの Icon を生成します。
    /// </summary>
    /// <param name="image">変換元の Image オブジェクト</param>
    /// <returns>変換後の Icon オブジェクト</returns>
    private static Icon ToIcon(this Image image)
    {
        var bmp = image as Bitmap ?? new Bitmap(image);
        var hIcon = bmp.GetHicon();
        var tempIcon = Icon.FromHandle(hIcon);
        var newIcon = (Icon)tempIcon.Clone();
        DestroyIcon(hIcon);
        return newIcon;
    }
}
