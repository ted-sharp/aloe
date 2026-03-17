// <copyright file="ILauncherMenuService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Models;

namespace Aloe.Apps.Medock.MdLauncherLib.Services;

/// <summary>
/// ユーザーのロールに基づいてランチャーメニューを取得するサービス。
/// </summary>
public interface ILauncherMenuService
{
    /// <summary>
    /// ユーザーコードに紐づくメニュー・アプリ一覧を取得する。
    /// </summary>
    Task<List<MenuAppInfo>> GetMenusByUserCodeAsync(string userCode);
}
