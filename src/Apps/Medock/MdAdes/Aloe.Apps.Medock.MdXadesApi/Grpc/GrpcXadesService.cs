// <copyright file="GrpcXadesService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdXadesApi.Options;
using Aloe.Apps.Medock.MdXadesApi.Services;
using Aloe.Apps.Medock.MdXadesLib.Abstractions;
using Aloe.Apps.Medock.MdXadesLib.Contracts.Services;
using Aloe.Apps.Medock.MdXadesLib.Models;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.Medock.MdXadesApi.Grpc;

/// <summary>
/// XAdES 署名サービスの MagicOnion gRPC 実装。
/// </summary>
public class GrpcXadesService : ServiceBase<IXadesService>, IXadesService
{
    private readonly IXadesSigner _signer;
    private readonly ICertificateProvider _certificateProvider;
    private readonly SignatureStore _store;
    private readonly MdXadesApiOptions _options;

    /// <summary>
    /// <see cref="GrpcXadesService"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public GrpcXadesService(
        IXadesSigner signer,
        ICertificateProvider certificateProvider,
        SignatureStore store,
        IOptions<MdXadesApiOptions> options)
    {
        _signer = signer;
        _certificateProvider = certificateProvider;
        _store = store;
        _options = options.Value;
    }

    /// <inheritdoc/>
    public async UnaryResult<SignResponse> SignAsync(SignRequest request)
    {
        if (!Enum.TryParse<HashAlgorithmType>(request.HashAlgorithm, ignoreCase: true, out var hashAlg))
        {
            throw new ReturnStatusException(StatusCode.InvalidArgument, $"未対応のハッシュアルゴリズム: {request.HashAlgorithm}");
        }

        var signRequest = new XadesSignRequest
        {
            HashAlgorithm = hashAlg,
            HashValue = request.HashValue,
            FileName = request.FileName,
            MimeType = request.MimeType,
        };

        var result = await _signer.SignAsync(signRequest);

        // ファイル保存
        var basePath = GetOutputPath();
        Directory.CreateDirectory(basePath);
        var filePath = Path.Combine(basePath, $"{result.SignatureId}.xml");
        await File.WriteAllBytesAsync(filePath, result.XmlBytes);

        _store.Add(new Models.SignatureInfo
        {
            SignatureId = result.SignatureId,
            FileName = request.FileName,
            SignedAt = result.SignedAt,
            XmlFilePath = filePath,
        });

        return new SignResponse
        {
            SignatureId = result.SignatureId,
            XmlBytes = result.XmlBytes,
            SignedAt = result.SignedAt,
        };
    }

    /// <inheritdoc/>
    public async UnaryResult<VerifyResponse> VerifyAsync(VerifyRequest request)
    {
        var verifyRequest = new XadesVerifyRequest { XmlBytes = request.XmlBytes };
        var result = await _signer.VerifyAsync(verifyRequest);

        return new VerifyResponse
        {
            IsValid = result.IsValid,
            SignedAt = result.SignedAt,
            SignerSubject = result.SignerSubject,
            HasTimestamp = result.HasTimestamp,
            TimestampedAt = result.TimestampedAt,
            ErrorMessage = result.ErrorMessage,
        };
    }

    /// <inheritdoc/>
    public UnaryResult<SignResponse> GetSignatureAsync(GetSignatureRequest request)
    {
        var info = _store.Get(request.SignatureId);
        if (info == null)
        {
            throw new ReturnStatusException(StatusCode.NotFound, $"署名 '{request.SignatureId}' が見つかりません。");
        }

        if (!File.Exists(info.XmlFilePath))
        {
            throw new ReturnStatusException(StatusCode.NotFound, "署名ファイルが見つかりません。");
        }

        var xmlBytes = File.ReadAllBytes(info.XmlFilePath);

        return UnaryResult.FromResult(new SignResponse
        {
            SignatureId = info.SignatureId,
            XmlBytes = xmlBytes,
            SignedAt = info.SignedAt,
        });
    }

    /// <inheritdoc/>
    public UnaryResult<ListSignaturesResponse> ListSignaturesAsync()
    {
        var all = _store.GetAll();
        var summaries = all.Select(s => new SignatureSummary
        {
            SignatureId = s.SignatureId,
            FileName = s.FileName,
            SignedAt = s.SignedAt,
        }).ToList();

        return UnaryResult.FromResult(new ListSignaturesResponse { Signatures = summaries });
    }

    /// <inheritdoc/>
    public UnaryResult<CertificateInfoResponse> GetCertificateInfoAsync()
    {
        var cert = _certificateProvider.GetCertificate();
        return UnaryResult.FromResult(new CertificateInfoResponse
        {
            Subject = cert.Subject,
            Issuer = cert.Issuer,
            NotBefore = new DateTimeOffset(cert.NotBefore, TimeSpan.Zero),
            NotAfter = new DateTimeOffset(cert.NotAfter, TimeSpan.Zero),
            SerialNumber = cert.SerialNumber,
            Thumbprint = cert.Thumbprint,
        });
    }

    private string GetOutputPath()
    {
        return Path.IsPathRooted(_options.OutputPath)
            ? _options.OutputPath
            : Path.Combine(AppContext.BaseDirectory, _options.OutputPath);
    }
}
