// <copyright file="NativeMethods.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Aloe.Utils.Drawing;

/// <summary>
/// Win32 APIラッパー：アイコンハンドル等の操作メソッドを提供します。
/// </summary>
[SupportedOSPlatform("windows")]
internal static class NativeMethods
{
    /// <summary>
    /// アイコンのハンドルを解放します。
    /// </summary>
    /// <param name="hIcon">解放するアイコンのハンドル</param>
    /// <returns>成功した場合は true、それ以外は false</returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool DestroyIcon(IntPtr hIcon);
}
