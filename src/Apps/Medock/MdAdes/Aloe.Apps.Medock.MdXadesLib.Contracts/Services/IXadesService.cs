// <copyright file="IXadesService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using MagicOnion;
using MessagePack;

namespace Aloe.Apps.Medock.MdXadesLib.Contracts.Services;

/// <summary>署名リクエスト。</summary>
[MessagePackObject]
public class SignRequest
{
    /// <summary>ハッシュアルゴリズム名（SHA256, SHA384, SHA512）。</summary>
    [Key(0)]
    public string HashAlgorithm { get; set; } = "SHA256";

    /// <summary>ファイルハッシュ値（Base64）。</summary>
    [Key(1)]
    public string HashValue { get; set; } = string.Empty;

    /// <summary>署名対象のファイル名。</summary>
    [Key(2)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>MIME タイプ（省略可）。</summary>
    [Key(3)]
    public string? MimeType { get; set; }
}

/// <summary>署名レスポンス。</summary>
[MessagePackObject]
public class SignResponse
{
    /// <summary>署名 ID。</summary>
    [Key(0)]
    public Guid SignatureId { get; set; }

    /// <summary>署名 XML のバイト列。</summary>
    [Key(1)]
    public byte[] XmlBytes { get; set; } = [];

    /// <summary>署名時刻（UTC）。</summary>
    [Key(2)]
    public DateTimeOffset SignedAt { get; set; }
}

/// <summary>署名検証リクエスト。</summary>
[MessagePackObject]
public class VerifyRequest
{
    /// <summary>検証対象の XAdES XML バイト列。</summary>
    [Key(0)]
    public byte[] XmlBytes { get; set; } = [];
}

/// <summary>署名検証レスポンス。</summary>
[MessagePackObject]
public class VerifyResponse
{
    /// <summary>署名が有効かどうか。</summary>
    [Key(0)]
    public bool IsValid { get; set; }

    /// <summary>署名時刻（UTC）。</summary>
    [Key(1)]
    public DateTimeOffset? SignedAt { get; set; }

    /// <summary>署名者のサブジェクト名。</summary>
    [Key(2)]
    public string? SignerSubject { get; set; }

    /// <summary>タイムスタンプが含まれるかどうか。</summary>
    [Key(3)]
    public bool HasTimestamp { get; set; }

    /// <summary>タイムスタンプ時刻（UTC）。</summary>
    [Key(4)]
    public DateTimeOffset? TimestampedAt { get; set; }

    /// <summary>検証エラーメッセージ。</summary>
    [Key(5)]
    public string? ErrorMessage { get; set; }
}

/// <summary>署名取得リクエスト。</summary>
[MessagePackObject]
public class GetSignatureRequest
{
    /// <summary>署名 ID。</summary>
    [Key(0)]
    public Guid SignatureId { get; set; }
}

/// <summary>署名一覧レスポンス。</summary>
[MessagePackObject]
public class ListSignaturesResponse
{
    /// <summary>署名情報一覧。</summary>
    [Key(0)]
    public List<SignatureSummary> Signatures { get; set; } = [];
}

/// <summary>署名サマリー。</summary>
[MessagePackObject]
public class SignatureSummary
{
    /// <summary>署名 ID。</summary>
    [Key(0)]
    public Guid SignatureId { get; set; }

    /// <summary>署名対象ファイル名。</summary>
    [Key(1)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>署名時刻（UTC）。</summary>
    [Key(2)]
    public DateTimeOffset SignedAt { get; set; }
}

/// <summary>証明書情報レスポンス。</summary>
[MessagePackObject]
public class CertificateInfoResponse
{
    /// <summary>サブジェクト名。</summary>
    [Key(0)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>発行者名。</summary>
    [Key(1)]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>有効期限開始。</summary>
    [Key(2)]
    public DateTimeOffset NotBefore { get; set; }

    /// <summary>有効期限終了。</summary>
    [Key(3)]
    public DateTimeOffset NotAfter { get; set; }

    /// <summary>シリアル番号。</summary>
    [Key(4)]
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>拇印（Thumbprint）。</summary>
    [Key(5)]
    public string Thumbprint { get; set; } = string.Empty;
}

/// <summary>
/// XAdES 署名サービスの gRPC インターフェース。
/// </summary>
public interface IXadesService : IService<IXadesService>
{
    /// <summary>ハッシュ値に対して XAdES 署名を生成する。</summary>
    UnaryResult<SignResponse> SignAsync(SignRequest request);

    /// <summary>XAdES 署名を検証する。</summary>
    UnaryResult<VerifyResponse> VerifyAsync(VerifyRequest request);

    /// <summary>署名 XML をダウンロードする。</summary>
    UnaryResult<SignResponse> GetSignatureAsync(GetSignatureRequest request);

    /// <summary>署名一覧を取得する。</summary>
    UnaryResult<ListSignaturesResponse> ListSignaturesAsync();

    /// <summary>署名証明書情報を取得する。</summary>
    UnaryResult<CertificateInfoResponse> GetCertificateInfoAsync();
}
