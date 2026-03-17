// <copyright file="TrayIconManager.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Aloe.Apps.Medock.MdLauncher.Services;

/// <summary>
/// システムトレイアイコンの管理。
/// </summary>
public class TrayIconManager : IDisposable
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private readonly NotifyIcon _notifyIcon;
    private bool _disposed;

    /// <summary>メニュー表示が要求されたときに発火するイベント。</summary>
    public event EventHandler? ShowMenuRequested;

    /// <summary>終了が要求されたときに発火するイベント。</summary>
    public event EventHandler? ExitRequested;

    /// <summary>
    /// <see cref="TrayIconManager"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public TrayIconManager()
    {
        _notifyIcon = new NotifyIcon
        {
            Visible = true,
            Text = "MdLauncher",
            Icon = CreateDefaultIcon(),
            ContextMenuStrip = this.CreateContextMenu(),
        };

        _notifyIcon.DoubleClick += this.NotifyIcon_DoubleClick;
    }

    private static Icon CreateDefaultIcon()
    {
        using var bitmap = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var brush = new SolidBrush(Color.DodgerBlue);
        graphics.FillEllipse(brush, 0, 0, 16, 16);
        var hIcon = bitmap.GetHicon();
        return Icon.FromHandle(hIcon);
    }

    private ContextMenuStrip CreateContextMenu()
    {
        var menu = new ContextMenuStrip();

        var showMenuItem = new ToolStripMenuItem("メニューを開く");
        showMenuItem.Click += (s, e) => ShowMenuRequested?.Invoke(this, EventArgs.Empty);
        menu.Items.Add(showMenuItem);

        menu.Items.Add(new ToolStripSeparator());

        var exitMenuItem = new ToolStripMenuItem("終了");
        exitMenuItem.Click += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);
        menu.Items.Add(exitMenuItem);

        return menu;
    }

    private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        ShowMenuRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        var oldIcon = _notifyIcon.Icon;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();

        if (oldIcon != null)
        {
            DestroyIcon(oldIcon.Handle);
            oldIcon.Dispose();
        }

        _disposed = true;
    }
}
