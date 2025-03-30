using System;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Aloe.Common.AloeCoreLib.Logging;
using MagicOnion.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AloeExtImporter;

// ここでは汎用的な設定を行うため、CA1859 は抑制します。
#pragma warning disable IDE0079 // 不要な抑制を削除する (IDE0079)
#pragma warning disable CA1859 // パフォーマンスの向上のために可能な場合は具象型を使用する

internal static class HostExtensions
{
    /// <summary>
    /// 構成の追加を行います。
    /// </summary>
    internal static T ConfigureAloeExtImporter<T>(this T builder)
        where T : IHostApplicationBuilder
    {
        builder
            .AddApp()
            .AddPostgreSql()
            .AddSerilog();

        return builder;
    }

    /// <summary>
    /// PostgreSQL(EFCore) と関連クラスを追加します。
    /// </summary>
    private static IHostApplicationBuilder AddApp(this IHostApplicationBuilder builder)
    {
        // ユーザーシークレットIDが .csproj に設定されている前提
        builder.Configuration.AddUserSecrets<App>(optional: true);

        // HttpClient は使い回す
        builder.Services.AddHttpClient();

        return builder;
    }

    /// <summary>
    /// PostgreSQL(EFCore) と関連クラスを追加します。
    /// </summary>
    private static IHostApplicationBuilder AddPostgreSql(this IHostApplicationBuilder builder)
    {
        // DateTime は EFCore 6.0 以降は with timezone にマッピングされるので、それを without timezone にします。
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
        ArgumentException.ThrowIfNullOrWhiteSpace(connStr, nameof(connStr));

        // 都度サービスから取得するのでファクトリは使いません
        builder.Services.AddDbContext<AppDbContext>((services, options) =>
        {
            options.UseNpgsql(connStr);

            if (false)
            {
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }
            }
            else
            {
                // ログを出力しない
                options.UseLoggerFactory(NullLoggerFactory.Instance);
                //// 警告を無効にする
                //options.ConfigureWarnings(x => x.Ignore());
            }
        });

        return builder;
    }

}
