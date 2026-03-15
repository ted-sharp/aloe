// <copyright file="ServiceCollectionExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.CsvImporterLib.JfagyCodes.Extensions;

/// <summary>
/// J-FAGY アレルゲンコードインポートハンドラーの DI 登録拡張メソッド。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// J-FAGY アレルゲンコードインポートハンドラーを登録する。
    /// </summary>
    public static IServiceCollection AddJfagyCodesImport(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IImportHandler>(sp => new JfagyCodesImportHandler(
            sp.GetRequiredService<ImportRunRepository>(),
            connectionString));

        return services;
    }
}
