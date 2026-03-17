// <copyright file="ILauncherService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using MagicOnion;
using MessagePack;

namespace Aloe.Apps.Medock.MdLauncherLib.Contracts.Services;

/// <summary>ナビゲーション設定レスポンス。</summary>
[MessagePackObject]
public class GetLauncherConfigResponse
{
    /// <summary>設定バージョン。</summary>
    [Key(0)]
    public int Version { get; set; }

    /// <summary>カテゴリ一覧。</summary>
    [Key(1)]
    public List<LauncherCategoryItem> Categories { get; set; } = [];
}

/// <summary>ナビゲーションカテゴリ。</summary>
[MessagePackObject]
public class LauncherCategoryItem
{
    /// <summary>カテゴリ ID。</summary>
    [Key(0)]
    public string Id { get; set; } = string.Empty;

    /// <summary>カテゴリ名。</summary>
    [Key(1)]
    public string Name { get; set; } = string.Empty;

    /// <summary>表示順。</summary>
    [Key(2)]
    public int SortOrder { get; set; }

    /// <summary>アプリ一覧。</summary>
    [Key(3)]
    public List<LauncherAppItem> Apps { get; set; } = [];
}

/// <summary>ナビゲーションアプリ。</summary>
[MessagePackObject]
public class LauncherAppItem
{
    /// <summary>アプリ ID。</summary>
    [Key(0)]
    public string Id { get; set; } = string.Empty;

    /// <summary>アプリ名。</summary>
    [Key(1)]
    public string Name { get; set; } = string.Empty;

    /// <summary>説明。</summary>
    [Key(2)]
    public string Description { get; set; } = string.Empty;

    /// <summary>実行ファイルパス。</summary>
    [Key(3)]
    public string ExePath { get; set; } = string.Empty;

    /// <summary>起動引数。</summary>
    [Key(4)]
    public string Arguments { get; set; } = string.Empty;

    /// <summary>作業ディレクトリ。</summary>
    [Key(5)]
    public string WorkingDirectory { get; set; } = string.Empty;

    /// <summary>アイコンパス。</summary>
    [Key(6)]
    public string IconPath { get; set; } = string.Empty;

    /// <summary>NamedPipe 名。</summary>
    [Key(7)]
    public string PipeName { get; set; } = string.Empty;

    /// <summary>表示順。</summary>
    [Key(8)]
    public int SortOrder { get; set; }

    /// <summary>有効フラグ。</summary>
    [Key(9)]
    public bool Enabled { get; set; }
}

/// <summary>
/// ナビゲーションサービスの gRPC インターフェース。
/// </summary>
public interface ILauncherService : IService<ILauncherService>
{
    /// <summary>ナビゲーション設定を取得する。</summary>
    UnaryResult<GetLauncherConfigResponse> GetConfigAsync();
}
