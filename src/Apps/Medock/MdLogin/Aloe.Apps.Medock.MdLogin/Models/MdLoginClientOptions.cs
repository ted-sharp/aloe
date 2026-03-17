// <copyright file="MdLoginClientOptions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.IO;

namespace Aloe.Apps.Medock.MdLogin.Models;

/// <summary>
/// MdLogin クライアントの設定。
/// </summary>
public class MdLoginClientOptions
{
    /// <summary>gRPC サーバーアドレス。</summary>
    public string ServerAddress { get; set; } = "http://localhost:5119";

    /// <summary>セッションファイルパス（空の場合は既定パスを使用）。</summary>
    public string SessionFilePath { get; set; } = string.Empty;

    /// <summary>解決済みのセッションファイルパスを取得する。</summary>
    public string ResolvedSessionFilePath =>
        string.IsNullOrEmpty(this.SessionFilePath)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Medock",
                "appsettings.Sessions.json")
            : this.SessionFilePath;
}
