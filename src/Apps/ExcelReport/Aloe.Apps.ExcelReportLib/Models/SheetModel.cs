// <copyright file="SheetModel.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.ExcelReportLib.Models;

/// <summary>
/// Excelシートから抽出した印刷レイアウト情報の中間モデル。
/// ライブラリ非依存で、セル座標はすべてポイント(pt)単位に正規化される。
/// </summary>
public class SheetModel
{
    /// <summary>用紙・余白・向きなどのページ設定。</summary>
    public PageSettings PageSettings { get; set; } = new();

    /// <summary>列幅の配列(pt)。インデックスが列番号に対応する。</summary>
    public List<double> ColumnWidths { get; set; } = [];

    /// <summary>行高の配列(pt)。インデックスが行番号に対応する。</summary>
    public List<double> RowHeights { get; set; } = [];

    /// <summary>値を持つセルの一覧。</summary>
    public List<CellData> Cells { get; set; } = [];

    /// <summary>結合セル範囲の一覧。</summary>
    public List<MergedRange> MergedRanges { get; set; } = [];

    /// <summary>図形の一覧。</summary>
    public List<ShapeData> Shapes { get; set; } = [];

    /// <summary>画像の一覧。</summary>
    public List<ImageData> Images { get; set; } = [];
}
