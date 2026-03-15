// <copyright file="XadesXmlBuilderTests.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Aloe.Apps.Medock.MdXadesLib.Models;
using Aloe.Apps.Medock.MdXadesLib.Services;
using FluentAssertions;

namespace Aloe.Apps.Medock.MdXadesLib.Tests;

public class XadesXmlBuilderTests
{
    private static X509Certificate2 CreateTestCertificate()
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest("CN=Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));
        return X509CertificateLoader.LoadPkcs12(cert.Export(X509ContentType.Pfx), null, X509KeyStorageFlags.Exportable);
    }

    [Fact]
    public void Build_ValidRequest_ReturnsXmlWithSignature()
    {
        // Arrange
        using var cert = CreateTestCertificate();
        var hash = SHA256.HashData("test data"u8.ToArray());
        var request = new XadesSignRequest
        {
            HashAlgorithm = HashAlgorithmType.SHA256,
            HashValue = Convert.ToBase64String(hash),
            FileName = "test.pdf",
            MimeType = "application/pdf",
        };

        // Act
        var (doc, signatureValue) = XadesXmlBuilder.Build(request, cert, Guid.NewGuid());

        // Assert
        doc.Should().NotBeNull();
        signatureValue.Should().NotBeEmpty();

        var nsMgr = new XmlNamespaceManager(doc.NameTable);
        nsMgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
        nsMgr.AddNamespace("xades", "http://uri.etsi.org/01903/v1.3.2#");

        doc.SelectSingleNode("//ds:SignatureValue", nsMgr).Should().NotBeNull();
        doc.SelectSingleNode("//ds:X509Certificate", nsMgr).Should().NotBeNull();
        doc.SelectSingleNode("//xades:SigningTime", nsMgr).Should().NotBeNull();
        doc.SelectSingleNode("//xades:SigningCertificate", nsMgr).Should().NotBeNull();
    }

    [Fact]
    public void EmbedTimestamp_WithToken_AddsUnsignedProperties()
    {
        // Arrange
        using var cert = CreateTestCertificate();
        var hash = SHA256.HashData("test"u8.ToArray());
        var request = new XadesSignRequest
        {
            HashAlgorithm = HashAlgorithmType.SHA256,
            HashValue = Convert.ToBase64String(hash),
            FileName = "test.pdf",
        };
        var (doc, _) = XadesXmlBuilder.Build(request, cert, Guid.NewGuid());
        var fakeToken = new byte[] { 1, 2, 3, 4 };

        // Act
        XadesXmlBuilder.EmbedTimestamp(doc, fakeToken);

        // Assert
        var nsMgr = new XmlNamespaceManager(doc.NameTable);
        nsMgr.AddNamespace("xades", "http://uri.etsi.org/01903/v1.3.2#");
        doc.SelectSingleNode("//xades:EncapsulatedTimeStamp", nsMgr).Should().NotBeNull();
    }

    [Fact]
    public void EmbedTimestamp_EmptyToken_DoesNotModify()
    {
        // Arrange
        using var cert = CreateTestCertificate();
        var hash = SHA256.HashData("test"u8.ToArray());
        var request = new XadesSignRequest
        {
            HashAlgorithm = HashAlgorithmType.SHA256,
            HashValue = Convert.ToBase64String(hash),
            FileName = "test.pdf",
        };
        var (doc, _) = XadesXmlBuilder.Build(request, cert, Guid.NewGuid());
        var originalXml = doc.OuterXml;

        // Act
        XadesXmlBuilder.EmbedTimestamp(doc, []);

        // Assert
        doc.OuterXml.Should().Be(originalXml);
    }
}
