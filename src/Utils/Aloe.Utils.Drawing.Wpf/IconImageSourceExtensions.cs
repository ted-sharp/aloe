// <copyright file="IconImageSourceExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Drawing;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aloe.Utils.Drawing.Wpf;

/// <summary>
/// Image から Icon への変換拡張メソッドを提供します。
/// </summary>
[SupportedOSPlatform("windows")]
public static class IconImageSourceExtensions
{
    /// <summary>
    /// Icon オブジェクトを ImageSource に変換します。
    /// </summary>
    /// <param name="icon">変換元の Icon オブジェクト</param>
    /// <returns>変換された ImageSource オブジェクト</returns>
    /// <exception cref="ArgumentNullException"><paramref name="icon"/> が null の場合</exception>
    public static ImageSource ToImageSource(this Icon icon)
    {
        ArgumentNullException.ThrowIfNull(icon);

        // HICON から直接 BitmapSource を生成
        var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

        // フリーズしてスレッドセーフ＆パフォーマンス向上
        if (bitmapSource.CanFreeze)
        {
            bitmapSource.Freeze();
        }

        return bitmapSource;
    }
}
