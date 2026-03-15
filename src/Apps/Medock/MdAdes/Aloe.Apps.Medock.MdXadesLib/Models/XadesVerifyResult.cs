// <copyright file="XadesVerifyResult.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.Medock.MdXadesLib.Models;

/// <summary>
/// XAdES 署名検証結果。
/// </summary>
public class XadesVerifyResult
{
    /// <summary>署名が有効かどうか。</summary>
    public bool IsValid { get; set; }

    /// <summary>署名時刻（UTC）。</summary>
    public DateTimeOffset? SignedAt { get; set; }

    /// <summary>署名者のサブジェクト名。</summary>
    public string? SignerSubject { get; set; }

    /// <summary>タイムスタンプが含まれるかどうか。</summary>
    public bool HasTimestamp { get; set; }

    /// <summary>タイムスタンプ時刻（UTC）。</summary>
    public DateTimeOffset? TimestampedAt { get; set; }

    /// <summary>検証エラーメッセージ。</summary>
    public string? ErrorMessage { get; set; }
}
