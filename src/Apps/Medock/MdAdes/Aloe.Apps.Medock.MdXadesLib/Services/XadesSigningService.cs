// <copyright file="XadesSigningService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Text;
using Aloe.Apps.Medock.MdXadesLib.Abstractions;
using Aloe.Apps.Medock.MdXadesLib.Models;
using Aloe.Apps.Medock.MdXadesLib.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.Medock.MdXadesLib.Services;

/// <summary>
/// XAdES 署名のオーケストレータ。XML 構築・タイムスタンプ取得・検証を統合する。
/// </summary>
public class XadesSigningService : IXadesSigner
{
    private readonly ICertificateProvider _certificateProvider;
    private readonly ITimestampClient _timestampClient;
    private readonly XadesOptions _options;
    private readonly ILogger<XadesSigningService> _logger;

    /// <summary>
    /// <see cref="XadesSigningService"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public XadesSigningService(
        ICertificateProvider certificateProvider,
        ITimestampClient timestampClient,
        IOptions<XadesOptions> options,
        ILogger<XadesSigningService> logger)
    {
        _certificateProvider = certificateProvider;
        _timestampClient = timestampClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<XadesSignResult> SignAsync(XadesSignRequest request, CancellationToken cancellationToken = default)
    {
        var signatureId = Guid.NewGuid();
        var certificate = _certificateProvider.GetCertificate();

        _logger.LogInformation("XAdES 署名を開始: {FileName}, アルゴリズム={Algorithm}", request.FileName, request.HashAlgorithm);

        // XAdES-BES 署名 XML を構築
        var (doc, signatureValue) = XadesXmlBuilder.Build(request, certificate, signatureId);

        // タイムスタンプ取得・埋め込み（XAdES-T）
        if (_options.EnableTimestamp)
        {
            var timestampToken = await _timestampClient.GetTimestampAsync(signatureValue, cancellationToken);
            XadesXmlBuilder.EmbedTimestamp(doc, timestampToken);
        }

        // XML をバイト列に変換（署名検証のため、空白を追加せず BOM なし UTF-8 で出力）
        using var ms = new MemoryStream();
        using (var writer = new System.Xml.XmlTextWriter(ms, new UTF8Encoding(false)))
        {
            doc.WriteTo(writer);
        }

        var xmlBytes = ms.ToArray();

        _logger.LogInformation("XAdES 署名を完了: ID={SignatureId}", signatureId);

        return new XadesSignResult
        {
            SignatureId = signatureId,
            XmlBytes = xmlBytes,
            SignedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <inheritdoc/>
    public Task<XadesVerifyResult> VerifyAsync(XadesVerifyRequest request, CancellationToken cancellationToken = default)
    {
        var result = XadesXmlVerifier.Verify(request.XmlBytes);
        return Task.FromResult(result);
    }
}
