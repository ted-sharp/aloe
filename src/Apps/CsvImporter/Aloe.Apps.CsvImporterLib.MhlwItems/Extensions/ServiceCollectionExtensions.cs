// <copyright file="ServiceCollectionExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.CsvImporterLib.MhlwItems.Extensions;

/// <summary>
/// 厚労省XML特定健診項目インポートハンドラーの DI 登録拡張メソッド。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 厚労省XML特定健診項目インポートハンドラーを登録する。
    /// </summary>
    public static IServiceCollection AddMhlwItemsImport(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IImportHandler>(sp => new MhlwItemsImportHandler(
            sp.GetRequiredService<ImportRunRepository>(),
            connectionString));

        return services;
    }
}
