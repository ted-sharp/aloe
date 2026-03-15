// <copyright file="SignaturesController.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdXadesApi.Models;
using Aloe.Apps.Medock.MdXadesApi.Options;
using Aloe.Apps.Medock.MdXadesApi.Services;
using Aloe.Apps.Medock.MdXadesLib.Abstractions;
using Aloe.Apps.Medock.MdXadesLib.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.Medock.MdXadesApi.Controllers;

/// <summary>
/// XAdES 署名を管理する REST API コントローラー。
/// </summary>
[ApiController]
[Route("api/signatures")]
public class SignaturesController : ControllerBase
{
    private readonly IXadesSigner _signer;
    private readonly ICertificateProvider _certificateProvider;
    private readonly SignatureStore _store;
    private readonly MdXadesApiOptions _options;

    /// <summary>
    /// <see cref="SignaturesController"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public SignaturesController(
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

    /// <summary>
    /// ハッシュ値を受け取り XAdES 署名を生成する。
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SignAsync([FromBody] SignRequestDto dto, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<HashAlgorithmType>(dto.HashAlgorithm, ignoreCase: true, out var hashAlg))
        {
            return this.BadRequest($"未対応のハッシュアルゴリズム: {dto.HashAlgorithm}");
        }

        var request = new XadesSignRequest
        {
            HashAlgorithm = hashAlg,
            HashValue = dto.HashValue,
            FileName = dto.FileName,
            MimeType = dto.MimeType,
        };

        var result = await _signer.SignAsync(request, cancellationToken);

        // ファイル保存
        var basePath = GetOutputPath();
        Directory.CreateDirectory(basePath);
        var filePath = Path.Combine(basePath, $"{result.SignatureId}.xml");
        await System.IO.File.WriteAllBytesAsync(filePath, result.XmlBytes, cancellationToken);

        _store.Add(new SignatureInfo
        {
            SignatureId = result.SignatureId,
            FileName = dto.FileName,
            SignedAt = result.SignedAt,
            XmlFilePath = filePath,
        });

        return this.Ok(new
        {
            signatureId = result.SignatureId,
            signedAt = result.SignedAt,
            xmlBase64 = Convert.ToBase64String(result.XmlBytes),
        });
    }

    /// <summary>
    /// 署名一覧を返す。
    /// </summary>
    [HttpGet]
    public IActionResult List()
    {
        var all = _store.GetAll();
        return this.Ok(all.Select(s => new
        {
            signatureId = s.SignatureId,
            fileName = s.FileName,
            signedAt = s.SignedAt,
        }));
    }

    /// <summary>
    /// 署名 XML をダウンロードする。
    /// </summary>
    [HttpGet("{id:guid}/download")]
    public IActionResult Download(Guid id)
    {
        var info = _store.Get(id);
        if (info == null)
        {
            return this.NotFound();
        }

        if (!System.IO.File.Exists(info.XmlFilePath))
        {
            return this.NotFound("署名ファイルが見つかりません。");
        }

        var stream = System.IO.File.OpenRead(info.XmlFilePath);
        return this.File(stream, "application/xml", $"{id}.xml");
    }

    /// <summary>
    /// XAdES XML を検証する。
    /// </summary>
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyAsync([FromBody] VerifyRequestDto dto, CancellationToken cancellationToken)
    {
        var request = new XadesVerifyRequest
        {
            XmlBytes = Convert.FromBase64String(dto.XmlBase64),
        };

        var result = await _signer.VerifyAsync(request, cancellationToken);

        return this.Ok(new
        {
            isValid = result.IsValid,
            signedAt = result.SignedAt,
            signerSubject = result.SignerSubject,
            hasTimestamp = result.HasTimestamp,
            timestampedAt = result.TimestampedAt,
            errorMessage = result.ErrorMessage,
        });
    }

    /// <summary>
    /// 署名証明書情報を返す。
    /// </summary>
    [HttpGet("certificate")]
    public IActionResult GetCertificate()
    {
        var cert = _certificateProvider.GetCertificate();
        return this.Ok(new
        {
            subject = cert.Subject,
            issuer = cert.Issuer,
            notBefore = new DateTimeOffset(cert.NotBefore, TimeSpan.Zero),
            notAfter = new DateTimeOffset(cert.NotAfter, TimeSpan.Zero),
            serialNumber = cert.SerialNumber,
            thumbprint = cert.Thumbprint,
        });
    }

    private string GetOutputPath()
    {
        return Path.IsPathRooted(_options.OutputPath)
            ? _options.OutputPath
            : Path.Combine(AppContext.BaseDirectory, _options.OutputPath);
    }
}

/// <summary>署名リクエスト DTO。</summary>
public class SignRequestDto
{
    /// <summary>ハッシュアルゴリズム名。</summary>
    public string HashAlgorithm { get; set; } = "SHA256";

    /// <summary>ファイルハッシュ値（Base64）。</summary>
    public string HashValue { get; set; } = string.Empty;

    /// <summary>署名対象のファイル名。</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>MIME タイプ（省略可）。</summary>
    public string? MimeType { get; set; }
}

/// <summary>検証リクエスト DTO。</summary>
public class VerifyRequestDto
{
    /// <summary>検証対象の XAdES XML（Base64）。</summary>
    public string XmlBase64 { get; set; } = string.Empty;
}
