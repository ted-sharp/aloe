// <copyright file="ServiceCollectionExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdXadesLib.Abstractions;
using Aloe.Apps.Medock.MdXadesLib.Certificates;
using Aloe.Apps.Medock.MdXadesLib.Options;
using Aloe.Apps.Medock.MdXadesLib.Services;
using Aloe.Apps.Medock.MdXadesLib.Timestamps;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.Medock.MdXadesLib.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> へ MdXades サービスを登録する拡張メソッド。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// MdXades サービスを登録する（開発用証明書 + RFC 3161 タイムスタンプ）。
    /// </summary>
    public static IServiceCollection AddMdXades(this IServiceCollection services)
    {
        services.AddSingleton<ICertificateProvider, DevCertificateProvider>();
        services.AddHttpClient<Rfc3161TimestampClient>();
        services.AddSingleton<ITimestampClient>(sp => sp.GetRequiredService<Rfc3161TimestampClient>());
        services.AddSingleton<IXadesSigner, XadesSigningService>();
        services.AddOptions<XadesOptions>();

        return services;
    }

    /// <summary>
    /// MdXades サービスを登録する（PFX ファイル証明書 + RFC 3161 タイムスタンプ）。
    /// </summary>
    public static IServiceCollection AddMdXadesWithFileCert(this IServiceCollection services)
    {
        services.AddSingleton<ICertificateProvider, FileCertificateProvider>();
        services.AddHttpClient<Rfc3161TimestampClient>();
        services.AddSingleton<ITimestampClient>(sp => sp.GetRequiredService<Rfc3161TimestampClient>());
        services.AddSingleton<IXadesSigner, XadesSigningService>();
        services.AddOptions<XadesOptions>();

        return services;
    }
}
