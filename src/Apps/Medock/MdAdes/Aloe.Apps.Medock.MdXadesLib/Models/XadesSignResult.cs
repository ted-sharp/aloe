// <copyright file="XadesSignResult.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.Medock.MdXadesLib.Models;

/// <summary>
/// XAdES 署名結果。
/// </summary>
public class XadesSignResult
{
    /// <summary>署名 ID。</summary>
    public Guid SignatureId { get; set; }

    /// <summary>署名 XML のバイト列。</summary>
    public byte[] XmlBytes { get; set; } = [];

    /// <summary>署名時刻（UTC）。</summary>
    public DateTimeOffset SignedAt { get; set; }
}
