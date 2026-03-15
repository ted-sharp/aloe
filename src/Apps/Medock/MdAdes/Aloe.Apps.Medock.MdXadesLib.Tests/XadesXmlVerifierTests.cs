// <copyright file="XadesXmlVerifierTests.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Aloe.Apps.Medock.MdXadesLib.Models;
using Aloe.Apps.Medock.MdXadesLib.Services;
using FluentAssertions;

namespace Aloe.Apps.Medock.MdXadesLib.Tests;

public class XadesXmlVerifierTests
{
    private static X509Certificate2 CreateTestCertificate()
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest("CN=Verifier Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));
        return X509CertificateLoader.LoadPkcs12(cert.Export(X509ContentType.Pfx), null, X509KeyStorageFlags.Exportable);
    }

    [Fact]
    public void Verify_ValidSignature_ReturnsValid()
    {
        // Arrange
        using var cert = CreateTestCertificate();
        var hash = SHA256.HashData("verify test"u8.ToArray());
        var request = new XadesSignRequest
        {
            HashAlgorithm = HashAlgorithmType.SHA256,
            HashValue = Convert.ToBase64String(hash),
            FileName = "test.pdf",
        };
        var (doc, _) = XadesXmlBuilder.Build(request, cert, Guid.NewGuid());

        using var ms = new MemoryStream();
        using (var writer = new System.Xml.XmlTextWriter(ms, new UTF8Encoding(false)))
        {
            doc.WriteTo(writer);
        }

        // Act
        var result = XadesXmlVerifier.Verify(ms.ToArray());

        // Assert
        result.IsValid.Should().BeTrue();
        result.SignerSubject.Should().Contain("CN=Verifier Test");
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Verify_TamperedSignature_ReturnsInvalid()
    {
        // Arrange
        using var cert = CreateTestCertificate();
        var hash = SHA256.HashData("tamper test"u8.ToArray());
        var request = new XadesSignRequest
        {
            HashAlgorithm = HashAlgorithmType.SHA256,
            HashValue = Convert.ToBase64String(hash),
            FileName = "test.pdf",
        };
        var (doc, _) = XadesXmlBuilder.Build(request, cert, Guid.NewGuid());

        // Tamper with the signature value
        var xml = doc.OuterXml.Replace("test.pdf", "tampered.pdf");
        var xmlBytes = Encoding.UTF8.GetBytes(xml);

        // Act
        var result = XadesXmlVerifier.Verify(xmlBytes);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Verify_InvalidXml_ReturnsError()
    {
        // Act
        var result = XadesXmlVerifier.Verify(Encoding.UTF8.GetBytes("<invalid/>"));

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }
}
