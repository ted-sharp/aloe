// <copyright file="ICertificateProvider.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Security.Cryptography.X509Certificates;

namespace Aloe.Apps.Medock.MdXadesLib.Abstractions;

/// <summary>
/// 署名用証明書を提供するインターフェース。
/// </summary>
public interface ICertificateProvider
{
    /// <summary>署名用の X.509 証明書を取得する。</summary>
    X509Certificate2 GetCertificate();
}
