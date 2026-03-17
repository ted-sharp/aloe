// <copyright file="ILauncherConfigService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Models;

namespace Aloe.Apps.Medock.MdLauncherLib.Services;

/// <summary>
/// ナビゲーション設定ファイルの読み取りサービス。
/// </summary>
public interface ILauncherConfigService
{
    /// <summary>現在の設定を取得する。</summary>
    LauncherConfig GetConfig();

    /// <summary>設定変更時に発火するイベント。</summary>
    event EventHandler? ConfigChanged;
}
