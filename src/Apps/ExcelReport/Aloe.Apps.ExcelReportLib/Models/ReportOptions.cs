// <copyright file="ReportOptions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.ExcelReportLib.Models;

/// <summary>
/// レポート生成の全体オプション。
/// </summary>
public class ReportOptions
{
    /// <summary>読み取るシートのインデックス(0-based)。null の場合は先頭シート。</summary>
    public int? SheetIndex { get; set; }

    /// <summary>Excel読み取りオプション。</summary>
    public ExcelReadOptions ExcelOptions { get; set; } = new();

    /// <summary>PDF描画オプション。</summary>
    public PdfRenderOptions PdfOptions { get; set; } = new();

    /// <summary>テンプレートオプション。</summary>
    public TemplateOptions TemplateOptions { get; set; } = new();
}

/// <summary>
/// Excel読み取り時のオプション。
/// </summary>
public class ExcelReadOptions
{
    /// <summary>読み取るシートのインデックス(0-based)。null の場合は先頭シート。</summary>
    public int? SheetIndex { get; set; }
}

/// <summary>
/// PDF描画時のオプション。
/// </summary>
public class PdfRenderOptions
{
}

/// <summary>
/// テンプレート置換のオプション。
/// </summary>
public class TemplateOptions
{
    /// <summary>置換キーワードのプレフィックス。</summary>
    public string Prefix { get; set; } = "${";

    /// <summary>置換キーワードのサフィックス。</summary>
    public string Suffix { get; set; } = "}";
}
