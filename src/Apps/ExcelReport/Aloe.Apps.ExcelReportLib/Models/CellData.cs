// <copyright file="CellData.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.ExcelReportLib.Models;

/// <summary>
/// 1つのセルの値・書式情報を保持するモデル。
/// </summary>
public class CellData
{
    /// <summary>行インデックス(0-based)。</summary>
    public int Row { get; set; }

    /// <summary>列インデックス(0-based)。</summary>
    public int Column { get; set; }

    /// <summary>セルの表示値(文字列化済み)。</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>フォント情報。</summary>
    public FontData Font { get; set; } = new();

    /// <summary>配置情報。</summary>
    public AlignmentData Alignment { get; set; } = new();

    /// <summary>罫線情報。</summary>
    public CellBorderData Borders { get; set; } = new();

    /// <summary>背景色(ARGB)。null は塗りつぶしなし。</summary>
    public string? BackgroundColor { get; set; }
}
