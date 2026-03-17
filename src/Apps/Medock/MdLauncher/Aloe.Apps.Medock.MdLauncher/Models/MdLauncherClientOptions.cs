// <copyright file="MdLauncherClientOptions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.Medock.MdLauncher.Models;

/// <summary>
/// MdLauncher クライアント設定。
/// </summary>
public class MdLauncherClientOptions
{
    /// <summary>サーバーアドレス。</summary>
    public string ServerAddress { get; set; } = "http://localhost:5119";

    /// <summary>ユーザーコード。</summary>
    public string UserCode { get; set; } = string.Empty;

    /// <summary>ホットキー設定。</summary>
    public HotkeyOptions Hotkey { get; set; } = new();

    /// <summary>ホットコーナー設定。</summary>
    public HotCornerOptions HotCorner { get; set; } = new();

    /// <summary>動作設定。</summary>
    public BehaviorOptions Behavior { get; set; } = new();
}

/// <summary>
/// ホットキー設定。
/// </summary>
public class HotkeyOptions
{
    /// <summary>有効フラグ。</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>修飾キー（例: "Control+Alt"）。</summary>
    public string Modifiers { get; set; } = "Control+Alt";

    /// <summary>キー（例: "Space"）。</summary>
    public string Key { get; set; } = "Space";
}

/// <summary>
/// ホットコーナー設定。
/// </summary>
public class HotCornerOptions
{
    /// <summary>有効フラグ。</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>コーナー位置。</summary>
    public string CornerPosition { get; set; } = "TopLeft";

    /// <summary>アクティベーション遅延（ミリ秒）。</summary>
    public int ActivationDelayMs { get; set; } = 300;

    /// <summary>コーナー判定サイズ（ピクセル）。</summary>
    public int CornerSizePx { get; set; } = 5;
}

/// <summary>
/// 動作設定。
/// </summary>
public class BehaviorOptions
{
    /// <summary>アプリ起動後に自動最小化するかどうか。</summary>
    public bool AutoMinimizeAfterLaunch { get; set; } = true;

    /// <summary>表示アニメーション時間（ミリ秒）。</summary>
    public int ShowAnimationDurationMs { get; set; } = 200;
}
