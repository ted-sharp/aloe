// <copyright file="LauncherConfig.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Aloe.Apps.Medock.MdLauncherLib.Models;

/// <summary>
/// ナビゲーション設定ファイルのルートモデル。
/// </summary>
public class LauncherConfig
{
    /// <summary>設定バージョン。</summary>
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    /// <summary>カテゴリ一覧。</summary>
    [JsonPropertyName("categories")]
    public List<AppCategory> Categories { get; set; } = [];
}

/// <summary>
/// アプリケーションカテゴリ。
/// </summary>
public class AppCategory
{
    /// <summary>カテゴリ ID。</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>カテゴリ名。</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>表示順。</summary>
    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }

    /// <summary>アプリ一覧。</summary>
    [JsonPropertyName("apps")]
    public List<AppRegistration> Apps { get; set; } = [];
}

/// <summary>
/// アプリケーション登録情報。
/// </summary>
public class AppRegistration
{
    /// <summary>アプリ ID。</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>アプリ名。</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>説明。</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>実行ファイルパス。</summary>
    [JsonPropertyName("exePath")]
    public string ExePath { get; set; } = string.Empty;

    /// <summary>起動引数。</summary>
    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = string.Empty;

    /// <summary>作業ディレクトリ。</summary>
    [JsonPropertyName("workingDirectory")]
    public string WorkingDirectory { get; set; } = string.Empty;

    /// <summary>アイコンパス。</summary>
    [JsonPropertyName("iconPath")]
    public string IconPath { get; set; } = string.Empty;

    /// <summary>NamedPipe 名。</summary>
    [JsonPropertyName("pipeName")]
    public string PipeName { get; set; } = string.Empty;

    /// <summary>表示順。</summary>
    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }

    /// <summary>有効フラグ。</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
}
