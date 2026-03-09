// <copyright file="PageSettings.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.ExcelReportLib.Models;

/// <summary>
/// ページの用紙サイズ・余白・向きを表す設定。単位はミリメートル(mm)。
/// </summary>
public class PageSettings
{
    /// <summary>用紙幅(mm)。A4縦のデフォルト 210mm。</summary>
    public double PaperWidthMm { get; set; } = 210.0;

    /// <summary>用紙高さ(mm)。A4縦のデフォルト 297mm。</summary>
    public double PaperHeightMm { get; set; } = 297.0;

    /// <summary>上余白(mm)。</summary>
    public double MarginTopMm { get; set; } = 20.0;

    /// <summary>下余白(mm)。</summary>
    public double MarginBottomMm { get; set; } = 20.0;

    /// <summary>左余白(mm)。</summary>
    public double MarginLeftMm { get; set; } = 15.0;

    /// <summary>右余白(mm)。</summary>
    public double MarginRightMm { get; set; } = 15.0;

    /// <summary>ページの向き。</summary>
    public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;

    /// <summary>用紙幅をポイント(pt)で返す。</summary>
    public double PaperWidthPt => PaperWidthMm * 72.0 / 25.4;

    /// <summary>用紙高さをポイント(pt)で返す。</summary>
    public double PaperHeightPt => PaperHeightMm * 72.0 / 25.4;

    /// <summary>上余白をポイント(pt)で返す。</summary>
    public double MarginTopPt => MarginTopMm * 72.0 / 25.4;

    /// <summary>下余白をポイント(pt)で返す。</summary>
    public double MarginBottomPt => MarginBottomMm * 72.0 / 25.4;

    /// <summary>左余白をポイント(pt)で返す。</summary>
    public double MarginLeftPt => MarginLeftMm * 72.0 / 25.4;

    /// <summary>右余白をポイント(pt)で返す。</summary>
    public double MarginRightPt => MarginRightMm * 72.0 / 25.4;
}
