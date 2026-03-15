// <copyright file="ServiceCollectionExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.CsvImporterLib.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> へ CsvImporter コアサービスを登録する拡張メソッド。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// CsvImporter コアインフラ（ISourceFetcher・ICsvBulkLoader・ImportRunRepository）を登録する。
    /// </summary>
    /// <param name="services">サービスコレクション。</param>
    /// <param name="connectionString">PostgreSQL 接続文字列。</param>
    public static IServiceCollection AddCsvImporterCore(this IServiceCollection services, string connectionString)
    {
        services.AddHttpClient<ISourceFetcher, HttpSourceFetcher>();
        services.AddSingleton<ICsvBulkLoader, NpgsqlCsvBulkLoader>();
        services.AddSingleton(new ImportRunRepository(connectionString));
        return services;
    }
}
