// <copyright file="ServiceCollectionExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.CsvImporterLib.HealthFacility.Extensions;

/// <summary>
/// 特定健診実施機関インポートハンドラーの DI 登録拡張メソッド。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 特定健診実施機関インポートハンドラーを登録する。
    /// </summary>
    public static IServiceCollection AddHealthFacilityImport(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IImportHandler>(sp => new HealthFacilityImportHandler(
            sp.GetRequiredService<ICsvBulkLoader>(),
            sp.GetRequiredService<ImportRunRepository>(),
            connectionString));

        return services;
    }
}
