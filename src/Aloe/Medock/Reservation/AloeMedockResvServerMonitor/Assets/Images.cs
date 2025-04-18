using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvServerMonitor.Assets;

public static class Images
{
    public static Lazy<Image> PlayCircle = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\play_circle_16dp_75FB4C.png"));
    //public static Lazy<Image> CheckCircle = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\check_circle_16dp_75FB4C.png"));
    public static Lazy<Image> StopCircle = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\stop_circle_16dp_EA3323.png"));
    //public static Lazy<Image> Close = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\close_16dp_EA3323.png"));
    public static Lazy<Image> Hourglass = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\hourglass_top_16dp_F19E39.png"));
    public static Lazy<Image> PauseCircle = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\pause_circle_16dp_F19E39.png"));
    //public static Lazy<Image> Pause = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\pause_16dp_EA3323.png"));
    public static Lazy<Image> Cancel = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\cancel_16dp_8C1AF6.png"));
    public static Lazy<Image> Circle = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\circle_16dp_8C1AF6.png"));
    //public static Lazy<Image> DoNotDisturb = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\do_not_disturb_16dp_EA3323.png"));

    public static Lazy<Image> Add = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\add_16dp_75FB4C.png"));
    public static Lazy<Image> Remove = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\remove_16dp_EA3323.png"));
    public static Lazy<Image> Play = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\play_arrow_16dp_75FB4C.png"));
    public static Lazy<Image> Stop = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\stop_16dp_EA3323.png"));
    public static Lazy<Image> Restart = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\restart_alt_16dp_8C1AF6.png"));

    public static Lazy<Image> FolderOpen = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\folder_open_16dp_F19E39.png"));
    public static Lazy<Image> Settings = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\settings_16dp_75FBFD.png"));
    public static Lazy<Image> Logout = new(() => LoadImageWithoutLock(@"Assets\MaterialIcons\logout_16dp_8C1AF6.png"));

    /// <summary>
    /// ファイルシステム上の PNG ファイルからファイルロックしない形で Image を生成します。
    /// </summary>
    /// <param name="filePath">PNG ファイルのパス</param>
    /// <returns>生成された Image</returns>
    private static Image LoadImageWithoutLock(string filePath)
    {
        var imageData = File.ReadAllBytes(filePath);
        using var ms = new MemoryStream(imageData);
        return Image.FromStream(ms);
    }
}
