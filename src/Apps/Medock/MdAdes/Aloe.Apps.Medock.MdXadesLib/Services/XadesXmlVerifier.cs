// <copyright file="XadesXmlVerifier.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Aloe.Apps.Medock.MdXadesLib.Models;

namespace Aloe.Apps.Medock.MdXadesLib.Services;

/// <summary>
/// XAdES XML 署名を検証する。
/// </summary>
public static class XadesXmlVerifier
{
    private const string DsNs = "http://www.w3.org/2000/09/xmldsig#";
    private const string XadesNs = "http://uri.etsi.org/01903/v1.3.2#";

    /// <summary>
    /// XAdES XML を検証する。
    /// </summary>
    public static XadesVerifyResult Verify(byte[] xmlBytes)
    {
        var result = new XadesVerifyResult();

        try
        {
            var doc = new XmlDocument { PreserveWhitespace = true };
            doc.LoadXml(System.Text.Encoding.UTF8.GetString(xmlBytes));

            var nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ds", DsNs);
            nsMgr.AddNamespace("xades", XadesNs);

            // 証明書を取得
            var certNode = doc.SelectSingleNode("//ds:X509Certificate", nsMgr);
            if (certNode == null)
            {
                result.ErrorMessage = "署名に X509Certificate が含まれていません。";
                return result;
            }

            var cert = X509CertificateLoader.LoadCertificate(Convert.FromBase64String(certNode.InnerText));
            result.SignerSubject = cert.Subject;

            // SignedInfo の正規化と署名値の検証
            var signedInfoNode = doc.SelectSingleNode("//ds:SignedInfo", nsMgr);
            var signatureValueNode = doc.SelectSingleNode("//ds:SignatureValue", nsMgr);
            if (signedInfoNode == null || signatureValueNode == null)
            {
                result.ErrorMessage = "SignedInfo または SignatureValue が見つかりません。";
                return result;
            }

            // C14N 正規化（ドキュメントコンテキスト内で正規化するため XmlNodeList を使用）
            var transform = new XmlDsigExcC14NTransform();
            transform.LoadInput(CreateNodeListDocument(signedInfoNode));
            byte[] canonicalSignedInfo;
            using (var stream = (MemoryStream)transform.GetOutput(typeof(Stream)))
            {
                canonicalSignedInfo = stream.ToArray();
            }

            var signatureBytes = Convert.FromBase64String(signatureValueNode.InnerText);

            // 署名アルゴリズムを取得
            var sigMethodNode = doc.SelectSingleNode("//ds:SignedInfo/ds:SignatureMethod", nsMgr);
            var algorithm = sigMethodNode?.Attributes?["Algorithm"]?.Value ?? string.Empty;
            var hashAlgName = GetHashAlgorithmFromUri(algorithm);

            var rsa = cert.GetRSAPublicKey();
            if (rsa == null)
            {
                result.ErrorMessage = "RSA 公開鍵を取得できません。";
                return result;
            }

            result.IsValid = rsa.VerifyData(canonicalSignedInfo, signatureBytes, hashAlgName, RSASignaturePadding.Pkcs1);

            // 署名時刻
            var signingTimeNode = doc.SelectSingleNode("//xades:SigningTime", nsMgr);
            if (signingTimeNode != null && DateTimeOffset.TryParse(signingTimeNode.InnerText, out var signingTime))
            {
                result.SignedAt = signingTime;
            }

            // タイムスタンプ
            var tsNode = doc.SelectSingleNode("//xades:EncapsulatedTimeStamp", nsMgr);
            result.HasTimestamp = tsNode != null;
            if (tsNode != null)
            {
                result.TimestampedAt = result.SignedAt;
            }

            if (!result.IsValid)
            {
                result.ErrorMessage = "署名の検証に失敗しました。";
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"検証中にエラーが発生しました: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// SignedInfo ノードを独立した XmlDocument として再構築する。
    /// 名前空間宣言がノードに直接含まれるようにする。
    /// </summary>
    private static XmlDocument CreateNodeListDocument(XmlNode signedInfoNode)
    {
        // OuterXml ではなく、名前空間を明示的に含む XML を構築
        var outerXml = signedInfoNode.OuterXml;

        // 親からの名前空間がない場合に備えて追加
        if (!outerXml.Contains("xmlns:ds"))
        {
            outerXml = outerXml.Replace("<ds:SignedInfo", $"<ds:SignedInfo xmlns:ds=\"{DsNs}\"");
        }

        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.LoadXml(outerXml);
        return doc;
    }

    private static HashAlgorithmName GetHashAlgorithmFromUri(string uri) => uri switch
    {
        "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256" => HashAlgorithmName.SHA256,
        "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384" => HashAlgorithmName.SHA384,
        "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512" => HashAlgorithmName.SHA512,
        _ => HashAlgorithmName.SHA256,
    };
}
