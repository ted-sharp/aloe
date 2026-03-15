// <copyright file="ServiceCollectionExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.CsvImporterLib.JisCompatMap.Extensions;

/// <summary>
/// JIS互換マップインポートハンドラーの DI 登録拡張メソッド。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// JIS互換マップインポートハンドラーを登録する。
    /// </summary>
    public static IServiceCollection AddJisCompatMapImport(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IImportHandler>(sp => new JisCompatMapImportHandler(
            sp.GetRequiredService<ImportRunRepository>(),
            connectionString));

        return services;
    }
}
