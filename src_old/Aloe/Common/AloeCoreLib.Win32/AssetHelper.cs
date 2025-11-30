using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;


namespace Aloe.Common.AloeCoreLib.Win32;

[SupportedOSPlatform("windows")]
public static class AssetHelper
{

    // Win32 API の DestroyIcon を利用して一時的に作成したハンドルを解放します。
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    /// <summary>
    /// 既存の Image 型（実体が Bitmap として扱えるもの）から単一サイズの Icon を生成します。
    /// </summary>
    /// <param name="image">変換元の Image オブジェクト</param>
    /// <returns>変換後の Icon オブジェクト</returns>
    public static Icon ToIcon(this Image image)
    {
        var bmp = image as Bitmap ?? new Bitmap(image);
        var hIcon = bmp.GetHicon();
        var tempIcon = Icon.FromHandle(hIcon);
        var newIcon = (Icon)tempIcon.Clone();
        DestroyIcon(hIcon);
        return newIcon;
    }

    /// <summary>
    /// ファイルシステム上の PNG ファイルからファイルロックしない形で Image を生成します。
    /// </summary>
    /// <param name="filePath">PNG ファイルのパス</param>
    /// <returns>生成された Image</returns>
    public static Image LoadImageWithoutLock(string filePath)
    {
        var imageData = File.ReadAllBytes(filePath);
        using var ms = new MemoryStream(imageData);
        return Image.FromStream(ms);
    }
}
