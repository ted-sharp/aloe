// <copyright file="PrintOptions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.ExcelReportLib.Models;

/// <summary>
/// 印刷オプション。
/// </summary>
public class PrintOptions
{
    /// <summary>送信先プリンター名。</summary>
    public string PrinterName { get; set; } = string.Empty;

    /// <summary>印刷部数。</summary>
    public int Copies { get; set; } = 1;

    /// <summary>印刷解像度(DPI)。</summary>
    public int Dpi { get; set; } = 300;
}
