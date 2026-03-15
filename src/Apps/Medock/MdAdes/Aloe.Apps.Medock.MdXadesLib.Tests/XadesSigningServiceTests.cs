// <copyright file="XadesSigningServiceTests.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Aloe.Apps.Medock.MdXadesLib.Abstractions;
using Aloe.Apps.Medock.MdXadesLib.Models;
using Aloe.Apps.Medock.MdXadesLib.Options;
using Aloe.Apps.Medock.MdXadesLib.Services;
using Aloe.Apps.Medock.MdXadesLib.Timestamps;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Aloe.Apps.Medock.MdXadesLib.Tests;

public class XadesSigningServiceTests
{
    private static X509Certificate2 CreateTestCertificate()
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest("CN=Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));
        return X509CertificateLoader.LoadPkcs12(cert.Export(X509ContentType.Pfx), null, X509KeyStorageFlags.Exportable);
    }

    private static XadesSigningService CreateService(X509Certificate2 cert, bool enableTimestamp = false)
    {
        var certProvider = new Mock<ICertificateProvider>();
        certProvider.Setup(x => x.GetCertificate()).Returns(cert);

        var options = Microsoft.Extensions.Options.Options.Create(new XadesOptions
        {
            EnableTimestamp = enableTimestamp,
        });

        return new XadesSigningService(
            certProvider.Object,
            new NullTimestampClient(),
            options,
            NullLogger<XadesSigningService>.Instance);
    }

    [Fact]
    public async Task SignAsync_ValidRequest_ReturnsXmlBytes()
    {
        // Arrange
        using var cert = CreateTestCertificate();
        var service = CreateService(cert);
        var hash = SHA256.HashData("test data"u8.ToArray());
        var request = new XadesSignRequest
        {
            HashAlgorithm = HashAlgorithmType.SHA256,
            HashValue = Convert.ToBase64String(hash),
            FileName = "test.pdf",
            MimeType = "application/pdf",
        };

        // Act
        var result = await service.SignAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.SignatureId.Should().NotBeEmpty();
        result.XmlBytes.Should().NotBeEmpty();
        result.SignedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SignAndVerify_RoundTrip_Succeeds()
    {
        // Arrange
        using var cert = CreateTestCertificate();
        var service = CreateService(cert);
        var hash = SHA256.HashData("round trip test"u8.ToArray());
        var signRequest = new XadesSignRequest
        {
            HashAlgorithm = HashAlgorithmType.SHA256,
            HashValue = Convert.ToBase64String(hash),
            FileName = "roundtrip.pdf",
        };

        // Act
        var signResult = await service.SignAsync(signRequest);
        var verifyResult = await service.VerifyAsync(new XadesVerifyRequest { XmlBytes = signResult.XmlBytes });

        // Assert
        verifyResult.IsValid.Should().BeTrue();
        verifyResult.SignerSubject.Should().Contain("CN=Test");
        verifyResult.SignedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(HashAlgorithmType.SHA256)]
    [InlineData(HashAlgorithmType.SHA384)]
    [InlineData(HashAlgorithmType.SHA512)]
    public async Task SignAsync_AllAlgorithms_Succeed(HashAlgorithmType algorithm)
    {
        // Arrange
        using var cert = CreateTestCertificate();
        var service = CreateService(cert);
        var hash = algorithm switch
        {
            HashAlgorithmType.SHA256 => SHA256.HashData("test"u8.ToArray()),
            HashAlgorithmType.SHA384 => SHA384.HashData("test"u8.ToArray()),
            HashAlgorithmType.SHA512 => SHA512.HashData("test"u8.ToArray()),
            _ => throw new ArgumentException(),
        };

        var request = new XadesSignRequest
        {
            HashAlgorithm = algorithm,
            HashValue = Convert.ToBase64String(hash),
            FileName = "test.pdf",
        };

        // Act
        var result = await service.SignAsync(request);

        // Assert
        result.XmlBytes.Should().NotBeEmpty();
    }
}
