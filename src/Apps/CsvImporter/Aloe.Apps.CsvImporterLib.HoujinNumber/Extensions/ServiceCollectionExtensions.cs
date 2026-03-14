// <copyright file="ServiceCollectionExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.CsvImporterLib.HoujinNumber.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> へ法人番号インポートハンドラーを登録する拡張メソッド。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 法人番号インポートハンドラーを登録する。
    /// <see cref="Aloe.Apps.CsvImporterLib.Extensions.ServiceCollectionExtensions.AddCsvImporterCore"/> の後に呼ぶこと。
    /// </summary>
    /// <param name="services">サービスコレクション。</param>
    /// <param name="connectionString">PostgreSQL 接続文字列。</param>
    public static IServiceCollection AddHoujinNumberImport(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IImportHandler>(sp => new HoujinNumberImportHandler(
            sp.GetRequiredService<ICsvBulkLoader>(),
            sp.GetRequiredService<ImportRunRepository>(),
            connectionString));
        return services;
    }
}
