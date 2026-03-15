// <copyright file="ServiceCollectionExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.CsvImporterLib.Abstractions;
using Aloe.Apps.CsvImporterLib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.CsvImporterLib.FhirCodes.Extensions;

/// <summary>
/// FHIR観察コードインポートハンドラーの DI 登録拡張メソッド。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// FHIR観察コードインポートハンドラーを登録する。
    /// </summary>
    public static IServiceCollection AddFhirCodesImport(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IImportHandler>(sp => new FhirCodesImportHandler(
            sp.GetRequiredService<ImportRunRepository>(),
            connectionString));

        return services;
    }
}
