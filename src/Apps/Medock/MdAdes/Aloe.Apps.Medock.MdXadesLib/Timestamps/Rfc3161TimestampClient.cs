// <copyright file="Rfc3161TimestampClient.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using Aloe.Apps.Medock.MdXadesLib.Abstractions;
using Aloe.Apps.Medock.MdXadesLib.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.Medock.MdXadesLib.Timestamps;

/// <summary>
/// RFC 3161 タイムスタンプサーバーに HTTP で問い合わせるクライアント。
/// </summary>
public class Rfc3161TimestampClient : ITimestampClient
{
    private readonly HttpClient _httpClient;
    private readonly XadesOptions _options;
    private readonly ILogger<Rfc3161TimestampClient> _logger;

    /// <summary>
    /// <see cref="Rfc3161TimestampClient"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public Rfc3161TimestampClient(
        HttpClient httpClient,
        IOptions<XadesOptions> options,
        ILogger<Rfc3161TimestampClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<byte[]> GetTimestampAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        var hash = SHA256.HashData(data);

        var tsaRequest = Rfc3161TimestampRequest.CreateFromHash(
            hash,
            HashAlgorithmName.SHA256,
            requestSignerCertificates: true);

        var encoded = tsaRequest.Encode();

        using var content = new ByteArrayContent(encoded);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/timestamp-query");

        _logger.LogInformation("TSA リクエストを送信: {Url}", _options.TimestampServerUrl);

        var response = await _httpClient.PostAsync(_options.TimestampServerUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        var tsaResponse = tsaRequest.ProcessResponse(responseBytes, out _);

        _logger.LogInformation("タイムスタンプトークンを取得しました");

        return tsaResponse.AsSignedCms().Encode();
    }
}
