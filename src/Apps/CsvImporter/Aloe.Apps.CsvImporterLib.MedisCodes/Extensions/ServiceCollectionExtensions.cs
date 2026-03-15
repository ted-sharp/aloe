// <copyright file="ServiceCollectionExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.CsvImporterLib.MedisCodes.Extensions;

/// <summary>
/// MEDIS コードインポートハンドラーの DI 登録拡張メソッド。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// MEDIS 関連インポートハンドラー（medis-hot, medis-disease, medis-jlac10）を登録する。
    /// </summary>
    public static IServiceCollection AddMedisCodesImport(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IImportHandler>(sp => new MedisHotImportHandler(
            sp.GetRequiredService<ICsvBulkLoader>(),
            sp.GetRequiredService<ImportRunRepository>(),
            connectionString));

        services.AddSingleton<IImportHandler>(sp => new MedisDiseaseImportHandler(
            sp.GetRequiredService<ICsvBulkLoader>(),
            sp.GetRequiredService<ImportRunRepository>(),
            connectionString));

        services.AddSingleton<IImportHandler>(sp => new MedisJlac10ImportHandler(
            sp.GetRequiredService<ImportRunRepository>(),
            connectionString));

        return services;
    }
}
