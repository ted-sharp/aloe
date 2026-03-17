// <copyright file="ServiceCollectionExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Data;
using Aloe.Apps.Medock.MdLauncherLib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.Medock.MdLauncherLib.Extensions;

/// <summary>
/// MdLauncher サービスの DI 登録拡張メソッド。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// MdLauncher のサービスを DI コンテナに登録する。
    /// </summary>
    public static IServiceCollection AddMdLauncher(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MdLauncherDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ILauncherMenuService, LauncherMenuService>();

        return services;
    }
}
