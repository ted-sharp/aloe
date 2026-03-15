// <copyright file="FileCertificateProvider.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Security.Cryptography.X509Certificates;
using Aloe.Apps.Medock.MdXadesLib.Abstractions;
using Aloe.Apps.Medock.MdXadesLib.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.Medock.MdXadesLib.Certificates;

/// <summary>
/// PFX ファイルから証明書を読み込むプロバイダー。
/// </summary>
public class FileCertificateProvider : ICertificateProvider, IDisposable
{
    private readonly X509Certificate2 _certificate;
    private bool _disposed;

    /// <summary>
    /// <see cref="FileCertificateProvider"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public FileCertificateProvider(IOptions<XadesOptions> options, ILogger<FileCertificateProvider> logger)
    {
        var opt = options.Value;
        if (string.IsNullOrEmpty(opt.CertificatePath))
        {
            throw new InvalidOperationException("CertificatePath が設定されていません。");
        }

        _certificate = X509CertificateLoader.LoadPkcs12FromFile(
            opt.CertificatePath,
            opt.CertificatePassword,
            X509KeyStorageFlags.Exportable);

        logger.LogInformation("PFX 証明書を読み込みました: {Subject}", _certificate.Subject);
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
