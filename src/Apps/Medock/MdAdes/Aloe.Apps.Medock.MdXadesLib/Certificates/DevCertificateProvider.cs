// <copyright file="DevCertificateProvider.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Aloe.Apps.Medock.MdXadesLib.Abstractions;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.Medock.MdXadesLib.Certificates;

/// <summary>
/// 開発用の自己署名証明書を起動時に生成するプロバイダー。
/// </summary>
public class DevCertificateProvider : ICertificateProvider, IDisposable
{
    private readonly X509Certificate2 _certificate;
    private bool _disposed;

    /// <summary>
    /// <see cref="DevCertificateProvider"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public DevCertificateProvider(ILogger<DevCertificateProvider> logger)
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(
            "CN=MdXades Dev",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        req.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

        var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));

        // Windows では秘密鍵のエクスポート可能なコピーが必要
        _certificate = X509CertificateLoader.LoadPkcs12(
            cert.Export(X509ContentType.Pfx),
            null,
            X509KeyStorageFlags.Exportable);

        logger.LogInformation("開発用自己署名証明書を生成しました: {Subject}", _certificate.Subject);
    }

    /// <inheritdoc/>
    public X509Certificate2 GetCertificate()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _certificate;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _certificate.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
