// <copyright file="XadesVerifyRequest.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.Medock.MdXadesLib.Models;

/// <summary>
/// XAdES 署名検証リクエスト。
/// </summary>
public class XadesVerifyRequest
{
    /// <summary>検証対象の XAdES XML バイト列。</summary>
    public byte[] XmlBytes { get; set; } = [];
}
