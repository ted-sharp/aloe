// <copyright file="SignatureInfo.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.Medock.MdXadesApi.Models;

/// <summary>
/// 署名情報を保持するモデル。
/// </summary>
public class SignatureInfo
{
    /// <summary>署名 ID。</summary>
    public Guid SignatureId { get; set; }

    /// <summary>署名対象のファイル名。</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>署名時刻（UTC）。</summary>
    public DateTimeOffset SignedAt { get; set; }

    /// <summary>署名 XML ファイルのパス。</summary>
    public string XmlFilePath { get; set; } = string.Empty;
}
