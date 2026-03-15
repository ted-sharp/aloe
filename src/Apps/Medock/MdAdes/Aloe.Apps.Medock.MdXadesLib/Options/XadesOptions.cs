// <copyright file="XadesOptions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.Medock.MdXadesLib.Options;

/// <summary>
/// XAdES 署名サービスの設定オプション。
/// </summary>
public class XadesOptions
{
    /// <summary>開発用自己署名証明書を使用するかどうか。</summary>
    public bool UseDevCertificate { get; set; } = true;

    /// <summary>PFX 証明書ファイルのパス。</summary>
    public string? CertificatePath { get; set; }

    /// <summary>PFX 証明書のパスワード。</summary>
    public string? CertificatePassword { get; set; }

    /// <summary>RFC 3161 タイムスタンプサーバーの URL。</summary>
    public string TimestampServerUrl { get; set; } = "http://localhost:8318/";

    /// <summary>タイムスタンプを有効にするかどうか。</summary>
    public bool EnableTimestamp { get; set; } = true;

    /// <summary>署名 XML の出力ディレクトリパス。</summary>
    public string OutputPath { get; set; } = "signatures";

    /// <summary>署名の最大保持時間（分）。</summary>
    public int MaxSignatureAgeMins { get; set; } = 1440;
}
