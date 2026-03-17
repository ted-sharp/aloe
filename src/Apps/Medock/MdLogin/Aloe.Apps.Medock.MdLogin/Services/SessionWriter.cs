// <copyright file="SessionWriter.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using Aloe.Apps.Medock.MdLauncherLib.Contracts.Services;

namespace Aloe.Apps.Medock.MdLogin.Services;

/// <summary>
/// セッション情報を JSON ファイルにアトミック書き込みするサービス。
/// </summary>
public class SessionWriter
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly string _filePath;

    /// <summary>
    /// <see cref="SessionWriter"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public SessionWriter(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>
    /// セッション情報を JSON ファイルに書き込む。
    /// </summary>
    public async Task WriteAsync(LoginResponse response)
    {
        var session = new
        {
            Session = new
            {
                response.UserCode,
                UserId = response.UserId.ToString(),
                SessionKey = response.SessionKey.ToString(),
                IssuedAt = response.IssuedAt.ToString("o"),
            },
        };

        var json = JsonSerializer.Serialize(session, s_jsonOptions);

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = _filePath + ".tmp";
        await File.WriteAllTextAsync(tempPath, json);
        File.Move(tempPath, _filePath, overwrite: true);
    }
}
