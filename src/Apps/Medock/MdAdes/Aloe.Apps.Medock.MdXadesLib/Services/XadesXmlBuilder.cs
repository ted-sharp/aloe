// <copyright file="XadesXmlBuilder.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using Aloe.Apps.Medock.MdXadesLib.Models;

namespace Aloe.Apps.Medock.MdXadesLib.Services;

/// <summary>
/// XAdES XML を手動で構築する。
/// SignedXml は detached signature の pre-computed ハッシュと相性が悪いため、
/// 手動で SignedInfo を構築 → C14N → RSA.SignData() で署名するアプローチを採用。
/// </summary>
public static class XadesXmlBuilder
{
    private const string DsNs = "http://www.w3.org/2000/09/xmldsig#";
    private const string XadesNs = "http://uri.etsi.org/01903/v1.3.2#";
    private const string C14NUrl = "http://www.w3.org/2001/10/xml-exc-c14n#";

    /// <summary>
    /// XAdES-BES 署名 XML を構築する。タイムスタンプは後から埋め込む。
    /// </summary>
    public static (XmlDocument Document, byte[] SignatureValue) Build(
        XadesSignRequest request,
        X509Certificate2 certificate,
        Guid signatureId)
    {
        var signedPropertiesId = $"SignedProperties-{signatureId}";
        var signatureValueId = $"SignatureValue-{signatureId}";
        var signingTime = DateTimeOffset.UtcNow;

        var digestAlgorithmUri = GetDigestAlgorithmUri(request.HashAlgorithm);
        var signatureAlgorithmUri = GetSignatureAlgorithmUri(request.HashAlgorithm);

        var hashBytes = Convert.FromBase64String(request.HashValue);

        // 1. SignedProperties XML
        var signedPropertiesXml = BuildSignedProperties(
            signedPropertiesId, signingTime, certificate, request, digestAlgorithmUri);

        // 2. SignedProperties のダイジェスト計算
        var signedPropertiesDigest = ComputeC14NDigest(signedPropertiesXml, request.HashAlgorithm);

        // 3. SignedInfo XML 構築
        var signedInfoXml = BuildSignedInfo(
            digestAlgorithmUri, signatureAlgorithmUri, hashBytes, request.FileName,
            signedPropertiesDigest, signedPropertiesId);

        // 4. SignedInfo を C14N して署名
        var signedInfoCanonical = Canonicalize(signedInfoXml);
        var rsa = certificate.GetRSAPrivateKey()
            ?? throw new InvalidOperationException("証明書に RSA 秘密鍵がありません。");

        var hashAlgName = ToHashAlgorithmName(request.HashAlgorithm);
        var signatureValue = rsa.SignData(signedInfoCanonical, hashAlgName, RSASignaturePadding.Pkcs1);

        // 5. Signature XML 全体を組み立て
        var doc = AssembleSignature(
            signedInfoXml, signatureValue, signatureValueId, certificate, signedPropertiesXml);

        return (doc, signatureValue);
    }

    /// <summary>
    /// UnsignedProperties にタイムスタンプトークンを埋め込む。
    /// </summary>
    public static void EmbedTimestamp(XmlDocument doc, byte[] timestampToken)
    {
        if (timestampToken.Length == 0)
        {
            return;
        }

        var nsMgr = new XmlNamespaceManager(doc.NameTable);
        nsMgr.AddNamespace("ds", DsNs);
        nsMgr.AddNamespace("xades", XadesNs);

        var qualifyingProps = doc.SelectSingleNode("//xades:QualifyingProperties", nsMgr)
            ?? throw new InvalidOperationException("QualifyingProperties が見つかりません。");

        var unsignedProps = doc.CreateElement("xades", "UnsignedProperties", XadesNs);
        var unsignedSigProps = doc.CreateElement("xades", "UnsignedSignatureProperties", XadesNs);
        var sigTimeStamp = doc.CreateElement("xades", "SignatureTimeStamp", XadesNs);

        var encapTsToken = doc.CreateElement("xades", "EncapsulatedTimeStamp", XadesNs);
        encapTsToken.InnerText = Convert.ToBase64String(timestampToken);

        sigTimeStamp.AppendChild(encapTsToken);
        unsignedSigProps.AppendChild(sigTimeStamp);
        unsignedProps.AppendChild(unsignedSigProps);
        qualifyingProps.AppendChild(unsignedProps);
    }

    private static string BuildSignedProperties(
        string signedPropertiesId,
        DateTimeOffset signingTime,
        X509Certificate2 certificate,
        XadesSignRequest request,
        string digestAlgorithmUri)
    {
        var certDigest = ComputeCertificateDigest(certificate, request.HashAlgorithm);

        var sb = new StringBuilder();
        sb.Append($"<xades:SignedProperties xmlns:xades=\"{XadesNs}\" xmlns:ds=\"{DsNs}\" Id=\"{signedPropertiesId}\">");
        sb.Append("<xades:SignedSignatureProperties>");
        sb.Append($"<xades:SigningTime>{signingTime:yyyy-MM-ddTHH:mm:ssZ}</xades:SigningTime>");
        sb.Append("<xades:SigningCertificate>");
        sb.Append("<xades:Cert>");
        sb.Append("<xades:CertDigest>");
        sb.Append($"<ds:DigestMethod Algorithm=\"{digestAlgorithmUri}\"/>");
        sb.Append($"<ds:DigestValue>{certDigest}</ds:DigestValue>");
        sb.Append("</xades:CertDigest>");
        sb.Append("<xades:IssuerSerial>");
        sb.Append($"<ds:X509IssuerName>{certificate.Issuer}</ds:X509IssuerName>");
        sb.Append($"<ds:X509SerialNumber>{certificate.SerialNumber}</ds:X509SerialNumber>");
        sb.Append("</xades:IssuerSerial>");
        sb.Append("</xades:Cert>");
        sb.Append("</xades:SigningCertificate>");
        sb.Append("</xades:SignedSignatureProperties>");
        sb.Append("<xades:SignedDataObjectProperties>");
        sb.Append("<xades:DataObjectFormat ObjectReference=\"#detached-ref\">");
        if (!string.IsNullOrEmpty(request.MimeType))
        {
            sb.Append($"<xades:MimeType>{request.MimeType}</xades:MimeType>");
        }

        sb.Append("</xades:DataObjectFormat>");
        sb.Append("</xades:SignedDataObjectProperties>");
        sb.Append("</xades:SignedProperties>");

        return sb.ToString();
    }

    private static string BuildSignedInfo(
        string digestAlgorithmUri,
        string signatureAlgorithmUri,
        byte[] fileHash,
        string fileName,
        string signedPropertiesDigest,
        string signedPropertiesId)
    {
        var sb = new StringBuilder();
        sb.Append($"<ds:SignedInfo xmlns:ds=\"{DsNs}\">");
        sb.Append($"<ds:CanonicalizationMethod Algorithm=\"{C14NUrl}\"/>");
        sb.Append($"<ds:SignatureMethod Algorithm=\"{signatureAlgorithmUri}\"/>");

        // Detached ファイル参照
        sb.Append($"<ds:Reference Id=\"detached-ref\" URI=\"{Uri.EscapeDataString(fileName)}\">");
        sb.Append($"<ds:DigestMethod Algorithm=\"{digestAlgorithmUri}\"/>");
        sb.Append($"<ds:DigestValue>{Convert.ToBase64String(fileHash)}</ds:DigestValue>");
        sb.Append("</ds:Reference>");

        // SignedProperties 参照
        sb.Append($"<ds:Reference URI=\"#{signedPropertiesId}\" Type=\"http://uri.etsi.org/01903#SignedProperties\">");
        sb.Append($"<ds:DigestMethod Algorithm=\"{digestAlgorithmUri}\"/>");
        sb.Append($"<ds:DigestValue>{signedPropertiesDigest}</ds:DigestValue>");
        sb.Append("</ds:Reference>");

        sb.Append("</ds:SignedInfo>");

        return sb.ToString();
    }

    private static XmlDocument AssembleSignature(
        string signedInfoXml,
        byte[] signatureValue,
        string signatureValueId,
        X509Certificate2 certificate,
        string signedPropertiesXml)
    {
        var sb = new StringBuilder();
        sb.Append($"<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.Append($"<ds:Signature xmlns:ds=\"{DsNs}\">");
        sb.Append(signedInfoXml);
        sb.Append($"<ds:SignatureValue Id=\"{signatureValueId}\">{Convert.ToBase64String(signatureValue)}</ds:SignatureValue>");

        // KeyInfo
        sb.Append("<ds:KeyInfo>");
        sb.Append("<ds:X509Data>");
        sb.Append($"<ds:X509Certificate>{Convert.ToBase64String(certificate.RawData)}</ds:X509Certificate>");
        sb.Append("</ds:X509Data>");
        sb.Append("</ds:KeyInfo>");

        // Object > QualifyingProperties
        sb.Append("<ds:Object>");
        sb.Append($"<xades:QualifyingProperties xmlns:xades=\"{XadesNs}\" Target=\"#Signature\">");
        sb.Append(signedPropertiesXml);
        sb.Append("</xades:QualifyingProperties>");
        sb.Append("</ds:Object>");

        sb.Append("</ds:Signature>");

        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.LoadXml(sb.ToString());
        return doc;
    }

    private static byte[] Canonicalize(string xml)
    {
        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.LoadXml(xml);

        var transform = new XmlDsigExcC14NTransform();
        transform.LoadInput(doc);
        using var stream = (MemoryStream)transform.GetOutput(typeof(Stream));
        return stream.ToArray();
    }

    private static string ComputeC14NDigest(string xml, HashAlgorithmType algorithmType)
    {
        var canonical = Canonicalize(xml);
        var hashAlg = ToHashAlgorithmName(algorithmType);
        var digest = HashData(canonical, hashAlg);
        return Convert.ToBase64String(digest);
    }

    private static string ComputeCertificateDigest(X509Certificate2 certificate, HashAlgorithmType algorithmType)
    {
        var hashAlg = ToHashAlgorithmName(algorithmType);
        var digest = HashData(certificate.RawData, hashAlg);
        return Convert.ToBase64String(digest);
    }

    private static byte[] HashData(byte[] data, HashAlgorithmName algorithmName)
    {
        return algorithmName.Name switch
        {
            "SHA256" => SHA256.HashData(data),
            "SHA384" => SHA384.HashData(data),
            "SHA512" => SHA512.HashData(data),
            _ => throw new ArgumentException($"未対応のハッシュアルゴリズム: {algorithmName.Name}"),
        };
    }

    private static string GetDigestAlgorithmUri(HashAlgorithmType algorithmType) => algorithmType switch
    {
        HashAlgorithmType.SHA256 => "http://www.w3.org/2001/04/xmlenc#sha256",
        HashAlgorithmType.SHA384 => "http://www.w3.org/2001/04/xmldsig-more#sha384",
        HashAlgorithmType.SHA512 => "http://www.w3.org/2001/04/xmlenc#sha512",
        _ => throw new ArgumentException($"未対応のハッシュアルゴリズム: {algorithmType}"),
    };

    private static string GetSignatureAlgorithmUri(HashAlgorithmType algorithmType) => algorithmType switch
    {
        HashAlgorithmType.SHA256 => "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
        HashAlgorithmType.SHA384 => "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384",
        HashAlgorithmType.SHA512 => "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512",
        _ => throw new ArgumentException($"未対応のハッシュアルゴリズム: {algorithmType}"),
    };

    private static HashAlgorithmName ToHashAlgorithmName(HashAlgorithmType algorithmType) => algorithmType switch
    {
        HashAlgorithmType.SHA256 => HashAlgorithmName.SHA256,
        HashAlgorithmType.SHA384 => HashAlgorithmName.SHA384,
        HashAlgorithmType.SHA512 => HashAlgorithmName.SHA512,
        _ => throw new ArgumentException($"未対応のハッシュアルゴリズム: {algorithmType}"),
    };
}
