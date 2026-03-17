// <copyright file="HotCornerService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;
using Aloe.Apps.Medock.MdLauncher.Models;

namespace Aloe.Apps.Medock.MdLauncher.Services;

/// <summary>
/// 画面コーナーへのマウス滞在を検知するサービス。
/// </summary>
public class HotCornerService : IDisposable
{
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    private readonly HotCornerOptions _options;
    private System.Threading.Timer? _timer;
    private DateTime _cornerEntryTime = DateTime.MinValue;
    private bool _activated;
    private DateTime _lastActivation = DateTime.MinValue;
    private bool _disposed;

    /// <summary>ホットコーナーがアクティベートされたときに発火するイベント。</summary>
    public event EventHandler? HotCornerActivated;

    /// <summary>
    /// <see cref="HotCornerService"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public HotCornerService(HotCornerOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// ホットコーナー検知を開始する。
    /// </summary>
    public void Start()
    {
        _timer = new System.Threading.Timer(this.CheckCursorPosition, null, 0, 50);
    }

    private void CheckCursorPosition(object? state)
    {
        if (!GetCursorPos(out var point))
        {
            return;
        }

        var isInCorner = _options.CornerPosition.ToUpperInvariant() switch
        {
            "TOPLEFT" => point.X <= _options.CornerSizePx && point.Y <= _options.CornerSizePx,
            "TOPRIGHT" => point.X >= System.Windows.SystemParameters.PrimaryScreenWidth - _options.CornerSizePx && point.Y <= _options.CornerSizePx,
            "BOTTOMLEFT" => point.X <= _options.CornerSizePx && point.Y >= System.Windows.SystemParameters.PrimaryScreenHeight - _options.CornerSizePx,
            "BOTTOMRIGHT" => point.X >= System.Windows.SystemParameters.PrimaryScreenWidth - _options.CornerSizePx && point.Y >= System.Windows.SystemParameters.PrimaryScreenHeight - _options.CornerSizePx,
            _ => point.X <= _options.CornerSizePx && point.Y <= _options.CornerSizePx,
        };

        if (isInCorner)
        {
            if (_cornerEntryTime == DateTime.MinValue)
            {
                _cornerEntryTime = DateTime.UtcNow;
            }
            else if (!_activated && (DateTime.UtcNow - _cornerEntryTime).TotalMilliseconds >= _options.ActivationDelayMs)
            {
                // デバウンス: 最後のアクティベーションから 1 秒以上経過している場合のみ発火
                if ((DateTime.UtcNow - _lastActivation).TotalSeconds >= 1)
                {
                    _activated = true;
                    _lastActivation = DateTime.UtcNow;
                    HotCornerActivated?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        else
        {
            _cornerEntryTime = DateTime.MinValue;
            _activated = false;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _timer?.Dispose();
        _disposed = true;
    }
}
