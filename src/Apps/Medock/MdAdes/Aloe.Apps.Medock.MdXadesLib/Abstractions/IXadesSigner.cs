// <copyright file="IXadesSigner.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdXadesLib.Models;

namespace Aloe.Apps.Medock.MdXadesLib.Abstractions;

/// <summary>
/// XAdES 署名サービスのインターフェース。
/// </summary>
public interface IXadesSigner
{
    /// <summary>ハッシュ値に対して XAdES 署名を生成する。</summary>
    Task<XadesSignResult> SignAsync(XadesSignRequest request, CancellationToken cancellationToken = default);

    /// <summary>XAdES 署名を検証する。</summary>
    Task<XadesVerifyResult> VerifyAsync(XadesVerifyRequest request, CancellationToken cancellationToken = default);
}
