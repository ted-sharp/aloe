// <copyright file="ExcelReportApiOptions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.ExcelReportApi.Options;

/// <summary>
/// ExcelReportApi の設定オプション。
/// </summary>
public class ExcelReportApiOptions
{
    /// <summary>サーバー側テンプレートを格納するディレクトリパス（絶対または相対）。</summary>
    public string TemplatePath { get; set; } = "templates";

    /// <summary>完了済みジョブを保持する最大時間（分）。</summary>
    public int MaxJobAgeMins { get; set; } = 60;

    /// <summary>印刷済みPDFを保存するフォルダパス（絶対または相対）。</summary>
    public string PrintOutputPath { get; set; } = "printjobs";

    /// <summary>印刷済みPDFの最大保持件数。超過分は古い順に削除される。</summary>
    public int MaxPrintedPdfCount { get; set; } = 100;
}
