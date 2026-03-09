// <copyright file="AlignmentData.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.ExcelReportLib.Models;

/// <summary>
/// セル内のテキスト配置情報を保持するモデル。
/// </summary>
public class AlignmentData
{
    /// <summary>水平方向の配置。</summary>
    public HorizontalAlign Horizontal { get; set; } = HorizontalAlign.General;

    /// <summary>垂直方向の配置。</summary>
    public VerticalAlign Vertical { get; set; } = VerticalAlign.Bottom;

    /// <summary>テキストを折り返すかどうか。</summary>
    public bool WrapText { get; set; }

    /// <summary>テキストの回転角度(度)。0〜360。</summary>
    public int Rotation { get; set; }
}

/// <summary>水平方向の配置。</summary>
public enum HorizontalAlign
{
    /// <summary>標準(型に応じて自動判定)。</summary>
    General,

    /// <summary>左揃え。</summary>
    Left,

    /// <summary>中央揃え。</summary>
    Center,

    /// <summary>右揃え。</summary>
    Right,
}

/// <summary>垂直方向の配置。</summary>
public enum VerticalAlign
{
    /// <summary>上揃え。</summary>
    Top,

    /// <summary>中央揃え。</summary>
    Center,

    /// <summary>下揃え。</summary>
    Bottom,
}
