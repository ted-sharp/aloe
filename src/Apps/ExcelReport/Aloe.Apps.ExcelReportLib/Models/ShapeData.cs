// <copyright file="ShapeData.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.ExcelReportLib.Models;

/// <summary>
/// Excelの図形(オートシェイプ等)の情報を保持するモデル。
/// 位置はTwo-Cell Anchorで表現し、描画時にポイント座標へ変換する。
/// </summary>
public class ShapeData
{
    /// <summary>図形の種別。</summary>
    public ShapeType ShapeType { get; set; } = ShapeType.Rectangle;

    /// <summary>図形の左上アンカー。</summary>
    public CellAnchor From { get; set; } = new();

    /// <summary>図形の右下アンカー。</summary>
    public CellAnchor To { get; set; } = new();

    /// <summary>塗りつぶし色(ARGB)。null は塗りつぶしなし。</summary>
    public string? FillColor { get; set; }

    /// <summary>線の色(ARGB)。</summary>
    public string LineColor { get; set; } = "#FF000000";

    /// <summary>線の幅(pt)。</summary>
    public double LineWidth { get; set; } = 0.75;

    /// <summary>図形内テキスト。</summary>
    public string? Text { get; set; }

    /// <summary>図形内テキストのフォント情報。</summary>
    public FontData? TextFont { get; set; }
}

/// <summary>図形の種別。</summary>
public enum ShapeType
{
    /// <summary>矩形。</summary>
    Rectangle,

    /// <summary>楕円。</summary>
    Ellipse,

    /// <summary>直線。</summary>
    Line,

    /// <summary>角丸矩形。</summary>
    RoundedRectangle,

    /// <summary>その他。</summary>
    Other,
}
