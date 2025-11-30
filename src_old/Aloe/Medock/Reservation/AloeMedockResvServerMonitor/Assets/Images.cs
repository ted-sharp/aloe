using Aloe.Common.AloeCoreLib.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Aloe.Common.AloeCoreLib.Util;

namespace Aloe.Medock.Reservation.AloeMedockResvServerMonitor.Assets;

public static class Images
{
    public static Lazy<Image> PlayCircle = new(() => Images.Get("PlayCircle"));
    public static Lazy<Image> StopCircle = new(() => Images.Get("StopCircle"));
    public static Lazy<Image> Hourglass = new(() => Images.Get("Hourglass"));
    public static Lazy<Image> PauseCircle = new(() => Images.Get("PauseCircle"));
    public static Lazy<Image> Cancel = new(() => Images.Get("Cancel"));
    public static Lazy<Image> Circle = new(() => Images.Get("Circle"));

    public static Lazy<Image> Add = new(() => Images.Get("Add"));
    public static Lazy<Image> Remove = new(() => Images.Get("Remove"));
    public static Lazy<Image> Play = new(() => Images.Get("Play"));
    public static Lazy<Image> Stop = new(() => Images.Get("Stop"));
    public static Lazy<Image> Restart = new(() => Images.Get("Restart"));

    public static Lazy<Image> FolderOpen = new(() => Images.Get("FolderOpen"));
    public static Lazy<Image> Settings = new(() => Images.Get("Settings"));
    public static Lazy<Image> Logout = new(() => Images.Get("Logout"));

    public static readonly Dictionary<string, Lazy<Image>> ByName = new()
    {
        ["PlayCircle"] = new(() => AssetHelper.LoadImageWithoutLock(
            PathHelper.FromBase(@"Assets\MaterialIcons\play_circle_16dp_75FB4C.png")
        )),
        ["StopCircle"] = new(() => AssetHelper.LoadImageWithoutLock(
            PathHelper.FromBase(@"Assets\MaterialIcons\stop_circle_16dp_EA3323.png")
        )),
        ["Hourglass"] = new(() => AssetHelper.LoadImageWithoutLock(
            PathHelper.FromBase(@"Assets\MaterialIcons\hourglass_top_16dp_F19E39.png")
        )),
        ["PauseCircle"] = new(() => AssetHelper.LoadImageWithoutLock(
            PathHelper.FromBase(@"Assets\MaterialIcons\pause_circle_16dp_F19E39.png")
        )),
        ["Cancel"] = new(() => AssetHelper.LoadImageWithoutLock(
            PathHelper.FromBase(@"Assets\MaterialIcons\cancel_16dp_8C1AF6.png")
        )),
        ["Circle"] = new(() => AssetHelper.LoadImageWithoutLock(
            PathHelper.FromBase(@"Assets\MaterialIcons\circle_16dp_8C1AF6.png")
        )),

        ["Add"] = new(() => AssetHelper.LoadImageWithoutLock(
            PathHelper.FromBase(@"Assets\MaterialIcons\add_16dp_75FB4C.png")
        )),
        ["Remove"] = new(() => AssetHelper.LoadImageWithoutLock(
            PathHelper.FromBase(@"Assets\MaterialIcons\remove_16dp_EA3323.png")
        )),
        ["Play"] = new(() => AssetHelper.LoadImageWithoutLock(
            PathHelper.FromBase(@"Assets\MaterialIcons\play_arrow_16dp_75FB4C.png")
        )),
        ["Stop"] = new(() => AssetHelper.LoadImageWithoutLock(
            PathHelper.FromBase(@"Assets\MaterialIcons\stop_16dp_EA3323.png")
        )),
        ["Restart"] = new(() => AssetHelper.LoadImageWithoutLock(
            PathHelper.FromBase(@"Assets\MaterialIcons\restart_alt_16dp_8C1AF6.png")
        )),

        ["FolderOpen"] = new(() => AssetHelper.LoadImageWithoutLock(
            PathHelper.FromBase(@"Assets\MaterialIcons\folder_open_16dp_F19E39.png")
        )),
        ["Settings"] = new(() => AssetHelper.LoadImageWithoutLock(
            PathHelper.FromBase(@"Assets\MaterialIcons\settings_16dp_75FBFD.png")
        )),
        ["Logout"] = new(() => AssetHelper.LoadImageWithoutLock(
            PathHelper.FromBase(@"Assets\MaterialIcons\logout_16dp_8C1AF6.png")
        )),
    };

    public static Image Get(string key) => Images.ByName[key].Value;
}
