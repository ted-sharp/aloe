// <copyright file="BorderData.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.ExcelReportLib.Models;

/// <summary>
/// セルの四辺の罫線情報をまとめたモデル。
/// </summary>
public class CellBorderData
{
    /// <summary>上辺の罫線。</summary>
    public BorderEdge Top { get; set; } = new();

    /// <summary>下辺の罫線。</summary>
    public BorderEdge Bottom { get; set; } = new();

    /// <summary>左辺の罫線。</summary>
    public BorderEdge Left { get; set; } = new();

    /// <summary>右辺の罫線。</summary>
    public BorderEdge Right { get; set; } = new();
}

/// <summary>
/// 罫線1辺のスタイルと色を表すモデル。
/// </summary>
public class BorderEdge
{
    /// <summary>罫線のスタイル。</summary>
    public BorderLineStyle Style { get; set; } = BorderLineStyle.None;

    /// <summary>罫線の色(ARGB形式)。</summary>
    public string Color { get; set; } = "#FF000000";
}

/// <summary>罫線のスタイル。</summary>
public enum BorderLineStyle
{
    /// <summary>なし。</summary>
    None,

    /// <summary>細線。</summary>
    Thin,

    /// <summary>中線。</summary>
    Medium,

    /// <summary>太線。</summary>
    Thick,

    /// <summary>破線。</summary>
    Dashed,

    /// <summary>点線。</summary>
    Dotted,

    /// <summary>二重線。</summary>
    Double,

    /// <summary>細い破線。</summary>
    Hair,
}
