// <copyright file="GlobalHotkeyService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Aloe.Apps.Medock.MdLauncher.Models;

namespace Aloe.Apps.Medock.MdLauncher.Services;

/// <summary>
/// グローバルホットキーの登録・処理。
/// </summary>
public class GlobalHotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const int HotkeyId = 1;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private HwndSource? _hwndSource;
    private bool _disposed;

    /// <summary>ホットキーが押されたときに発火するイベント。</summary>
    public event EventHandler? HotkeyPressed;

    /// <summary>
    /// ホットキーを登録する。
    /// </summary>
    public void Register(Window window, HotkeyOptions options)
    {
        var helper = new WindowInteropHelper(window);
        _hwndSource = HwndSource.FromHwnd(helper.Handle);
        _hwndSource?.AddHook(this.WndProc);

        var modifiers = ParseModifiers(options.Modifiers);
        var key = ParseKey(options.Key);

        RegisterHotKey(helper.Handle, HotkeyId, modifiers, key);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }

    private static uint ParseModifiers(string modifiers)
    {
        uint result = 0;
        var parts = modifiers.Split('+', StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            result |= part.ToUpperInvariant() switch
            {
                "ALT" => 0x0001,
                "CONTROL" or "CTRL" => 0x0002,
                "SHIFT" => 0x0004,
                "WIN" => 0x0008,
                _ => 0,
            };
        }

        return result;
    }

    private static uint ParseKey(string key)
    {
        return key.ToUpperInvariant() switch
        {
            "SPACE" => 0x20,
            "ENTER" or "RETURN" => 0x0D,
            "TAB" => 0x09,
            "F1" => 0x70,
            "F2" => 0x71,
            "F3" => 0x72,
            "F4" => 0x73,
            "F5" => 0x74,
            "F6" => 0x75,
            "F7" => 0x76,
            "F8" => 0x77,
            "F9" => 0x78,
            "F10" => 0x79,
            "F11" => 0x7A,
            "F12" => 0x7B,
            _ when key.Length == 1 && char.IsLetterOrDigit(key[0]) => (uint)char.ToUpperInvariant(key[0]),
            _ => 0,
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_hwndSource != null)
        {
            UnregisterHotKey(_hwndSource.Handle, HotkeyId);
            _hwndSource.RemoveHook(this.WndProc);
        }

        _disposed = true;
    }
}
