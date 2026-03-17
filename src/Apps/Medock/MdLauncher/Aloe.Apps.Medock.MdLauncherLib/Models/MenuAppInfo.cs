// <copyright file="MenuAppInfo.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.Medock.MdLauncherLib.Models;

/// <summary>
/// メニューとアプリ情報を結合した DTO。
/// </summary>
public class MenuAppInfo
{
    /// <summary>メニューコード。</summary>
    public string MenuCode { get; set; } = string.Empty;

    /// <summary>メニュー名。</summary>
    public string MenuName { get; set; } = string.Empty;

    /// <summary>メニュー説明。</summary>
    public string MenuDesc { get; set; } = string.Empty;

    /// <summary>メニュー表示順。</summary>
    public int MenuSeq { get; set; }

    /// <summary>アプリコード。</summary>
    public string AppCode { get; set; } = string.Empty;

    /// <summary>アプリ名。</summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>アプリ説明。</summary>
    public string AppDesc { get; set; } = string.Empty;

    /// <summary>実行ファイルパス。</summary>
    public string AppPath { get; set; } = string.Empty;

    /// <summary>起動引数。</summary>
    public string AppArgs { get; set; } = string.Empty;

    /// <summary>作業ディレクトリ。</summary>
    public string AppDir { get; set; } = string.Empty;

    /// <summary>アイコンパス。</summary>
    public string AppIcon { get; set; } = string.Empty;

    /// <summary>NamedPipe 名。</summary>
    public string PipeName { get; set; } = string.Empty;
}
