// <copyright file="ServiceCollectionExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportLib.Abstractions;
using Aloe.Apps.ExcelReportLib.Models;
using Aloe.Apps.ExcelReportLib.Readers;
using Aloe.Apps.ExcelReportLib.Services;
using Aloe.Apps.ExcelReportLib.Writers;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.ExcelReportLib.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> へ ExcelReport サービスを登録する拡張メソッド。
/// Excel読み取りとPDF描画のライブラリ実装をDIで切り替えて使用できる。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// デフォルト構成(NPOI + PDFsharp)で ExcelReport サービスを登録する。
    /// </summary>
    public static IServiceCollection AddExcelReport(this IServiceCollection services)
    {
        return services
            .AddExcelReportCore()
            .AddExcelReportWithNpoi()
            .AddExcelReportWithPdfSharp();
    }

    /// <summary>
    /// コアサービス(TemplateEngine, PositionCalculator, ExcelReportService)を登録する。
    /// Excel読み取り / PDF描画の実装は別途登録が必要。
    /// </summary>
    public static IServiceCollection AddExcelReportCore(this IServiceCollection services)
    {
        services.AddSingleton<PositionCalculator>();
        services.AddSingleton<ITemplateEngine, TemplateEngine>();
        services.AddSingleton<ExcelReportService>();

        services.AddOptions<TemplateOptions>();

        return services;
    }

    /// <summary>
    /// NPOI を使用した Excel 読み取り実装を登録する。
    /// </summary>
    public static IServiceCollection AddExcelReportWithNpoi(this IServiceCollection services)
    {
        services.AddSingleton<IExcelReader, NpoiExcelReader>();
        return services;
    }

    /// <summary>
    /// ClosedXML を使用した Excel 読み取り実装を登録する。
    /// 注意: ClosedXMLは図形サポートが限定的。
    /// </summary>
    public static IServiceCollection AddExcelReportWithClosedXml(this IServiceCollection services)
    {
        services.AddSingleton<IExcelReader, ClosedXmlExcelReader>();
        return services;
    }

    /// <summary>
    /// PDFsharp を使用した PDF 描画実装を登録する。
    /// </summary>
    public static IServiceCollection AddExcelReportWithPdfSharp(this IServiceCollection services)
    {
        services.AddSingleton<IPdfRenderer, PdfSharpWriter>();
        return services;
    }

    /// <summary>
    /// QuestPDF を使用した PDF 描画実装を登録する。
    /// </summary>
    public static IServiceCollection AddExcelReportWithQuestPdf(this IServiceCollection services)
    {
        services.AddSingleton<IPdfRenderer, QuestPdfWriter>();
        return services;
    }
}
