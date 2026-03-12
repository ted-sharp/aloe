// <copyright file="FontData.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.ExcelReportLib.Models;

/// <summary>
/// フォントの描画情報を保持するモデル。
/// </summary>
public class FontData
{
    /// <summary>フォント名。</summary>
    public string Name { get; set; } = "Yu Gothic";

    /// <summary>フォントサイズ(pt)。</summary>
    public double SizeInPoints { get; set; } = 11.0;

    /// <summary>太字かどうか。</summary>
    public bool Bold { get; set; }

    /// <summary>斜体かどうか。</summary>
    public bool Italic { get; set; }

    /// <summary>下線かどうか。</summary>
    public bool Underline { get; set; }

    /// <summary>二重下線かどうか。</summary>
    public bool DoubleUnderline { get; set; }

    /// <summary>取り消し線かどうか。</summary>
    public bool Strikethrough { get; set; }

    /// <summary>フォント色(ARGB形式、例: "#FF000000")。</summary>
    public string Color { get; set; } = "#FF000000";
}
