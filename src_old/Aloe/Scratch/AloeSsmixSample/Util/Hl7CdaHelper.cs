using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

// ReSharper disable ArrangeStaticMemberQualifier

namespace AloeSsmixSample.Util;

/// <summary>
/// セクション単位のデータを表すレコードです。
/// </summary>
public record SectionData(
    string SectionKey,
    string SectionType,
    string Content,
    string ContentHash
);

/// <summary>
/// SOAP/CDA 形式の XML ファイルを検出し、セクション単位で抽出・ハッシュ化するユーティリティクラスです。
/// </summary>
public static class Hl7CdaHelper
{
    /// <summary>
    /// ファイルが SOAP Envelope + HL7-CDA (ClinicalDocument) かどうかを判定します。
    /// </summary>
    public static bool IsSoapCda(string filePath)
    {
        var settings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true,
            DtdProcessing = DtdProcessing.Prohibit,
        };

        using var reader = XmlReader.Create(filePath, settings);
        if (reader.Read() &&
            reader is
            {
                NodeType: XmlNodeType.Element,
                LocalName: "Envelope",
            } &&
            reader.NamespaceURI.Contains("soap"))
        {
            while (reader.Read())
            {
                if (reader is
                    {
                        NodeType: XmlNodeType.Element,
                        LocalName: "ClinicalDocument",
                        NamespaceURI: "urn:hl7-org:v3",
                    })
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// XML ファイルを <see cref="XDocument"/> として読み込み返します。
    /// </summary>
    public static XDocument LoadXml(string filePath)
    {
        return XDocument.Load(filePath, LoadOptions.PreserveWhitespace);
    }

    /// <summary>
    /// 指定ファイル内のすべての &lt;section&gt; 要素を抽出し、セクションキー、内容、SHA‑256 ハッシュを返します。
    /// </summary>
    public static IEnumerable<SectionData> ExtractSections(string filePath)
    {
        var document = XDocument.Load(filePath, LoadOptions.PreserveWhitespace);
        var sections = document.Descendants().Where(x => x.Name.LocalName == "section");

        foreach (var section in sections)
        {
            var sectionKey = GetXPath(section);
            var sectionType = section.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "code")
                ?.Attribute("code")
                ?.Value ?? String.Empty;
            var content = section.ToString(SaveOptions.DisableFormatting);
            var contentHash = ComputeSha256HashCanonical(section);

            yield return new SectionData(sectionKey, sectionType, content, contentHash);
        }
    }

    /// <summary>
    /// XElement の絶対 XPath（ルートからのパス）を生成します。
    /// </summary>
    private static string GetXPath(XElement element)
    {
        var ancestors = element.AncestorsAndSelf().Reverse();
        var sb = new StringBuilder();

        foreach (var e in ancestors)
        {
            var siblings = e.Parent?.Elements(e.Name) ?? Enumerable.Empty<XElement>();
            var index = siblings.Count() > 1
                ? $"[{siblings.TakeWhile(s => s != e).Count() + 1}]"
                : String.Empty;

            sb.Append('/').Append(e.Name.LocalName).Append(index);
        }

        return sb.ToString();
    }

    /// <summary>
    /// XElement を Canonical XML 化し、SHA-256 ハッシュを計算します。
    /// </summary>
    private static string ComputeSha256HashCanonical(XElement element)
    {
        var xmlDoc = new XmlDocument();
        using (var reader = element.CreateReader())
        {
            xmlDoc.Load(reader);
        }

        var transform = new XmlDsigC14NTransform();
        transform.LoadInput(xmlDoc);

        using var stream = (Stream)transform.GetOutput(typeof(Stream));
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(stream);
        var sb = new StringBuilder();

        foreach (var b in hashBytes)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }
}
