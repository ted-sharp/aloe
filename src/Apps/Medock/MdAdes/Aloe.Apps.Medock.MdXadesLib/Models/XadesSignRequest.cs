// <copyright file="XadesSignRequest.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.Medock.MdXadesLib.Models;

/// <summary>
/// XAdES 署名リクエスト。
/// </summary>
public class XadesSignRequest
{
    /// <summary>ハッシュアルゴリズム。</summary>
    public HashAlgorithmType HashAlgorithm { get; set; } = HashAlgorithmType.SHA256;

    /// <summary>ファイルハッシュ値（Base64エンコード）。</summary>
    public string HashValue { get; set; } = string.Empty;

    /// <summary>署名対象のファイル名。</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>署名対象の MIME タイプ。</summary>
    public string? MimeType { get; set; }
}
