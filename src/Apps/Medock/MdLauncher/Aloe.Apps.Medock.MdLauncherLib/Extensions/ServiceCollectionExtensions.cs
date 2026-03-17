// <copyright file="ServiceCollectionExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.Medock.MdLauncherLib.Extensions;

/// <summary>
/// MdLauncher サービスの DI 登録拡張メソッド。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// MdLauncher のサービスを DI コンテナに登録する。
    /// </summary>
    public static IServiceCollection AddMdLauncher(this IServiceCollection services, string configFilePath)
    {
        services.AddSingleton<ILauncherConfigService>(sp =>
            new LauncherConfigService(configFilePath, sp.GetRequiredService<ILogger<LauncherConfigService>>()));

        return services;
    }
}
