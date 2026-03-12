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
}
