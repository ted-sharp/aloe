// <copyright file="NpgsqlExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace Aloe.Utils.Hosting.Npgsql;

/// <summary>
/// Npgsql / EF Core DbContext の DI 登録を一元化する拡張メソッドを提供します。
/// </summary>
public static class NpgsqlExtensions
{
    /// <summary>
    /// EF Core の <typeparamref name="TContext"/> を Npgsql で DI 登録します。
    /// </summary>
    /// <typeparam name="TContext">登録する <see cref="DbContext"/> の型。</typeparam>
    /// <param name="services">サービスコレクション。</param>
    /// <param name="configuration">接続文字列を取得するための構成インスタンス。</param>
    /// <param name="connectionStringName">接続文字列名。</param>
    /// <param name="configure">
    /// <see cref="DbContextOptionsBuilder"/> に対する追加のカスタマイズ。null の場合は無視されます。
    /// </param>
    /// <param name="options">オプション設定。null の場合はデフォルト値が使用されます。</param>
    /// <returns>チェーン呼び出し可能なサービスコレクション。</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/>、<paramref name="configuration"/>、
    /// または <paramref name="connectionStringName"/> が null の場合にスローされます。
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// 指定された接続文字列名に対応する接続文字列が見つからない場合にスローされます。
    /// </exception>
    public static IServiceCollection AddNpgsqlDbContext<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName,
        Action<DbContextOptionsBuilder>? configure = null,
        NpgsqlDbContextOptions? options = null)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(connectionStringName);

        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException(
                $"接続文字列 '{connectionStringName}' が構成に見つかりません。");

        var opts = options ?? new NpgsqlDbContextOptions();

        services.AddDbContext<TContext>(
            (sp, dbContextOptions) =>
            {
                dbContextOptions.UseNpgsql(connectionString);

                if (IsDevelopment(sp))
                {
                    dbContextOptions.EnableDetailedErrors();
                    dbContextOptions.EnableSensitiveDataLogging();
                }

                if (opts.SuppressSqlLogging)
                {
                    dbContextOptions.UseLoggerFactory(NullLoggerFactory.Instance);
                }

                configure?.Invoke(dbContextOptions);
            },
            opts.Lifetime);

        if (opts.AddFactory)
        {
            services.AddDbContextFactory<TContext>(
                (sp, dbContextOptions) =>
                {
                    dbContextOptions.UseNpgsql(connectionString);

                    if (IsDevelopment(sp))
                    {
                        dbContextOptions.EnableDetailedErrors();
                        dbContextOptions.EnableSensitiveDataLogging();
                    }

                    if (opts.SuppressSqlLogging)
                    {
                        dbContextOptions.UseLoggerFactory(NullLoggerFactory.Instance);
                    }

                    configure?.Invoke(dbContextOptions);
                },
                opts.Lifetime);
        }

        return services;
    }

    /// <summary>
    /// 生の <see cref="NpgsqlDataSource"/> を DI 登録します。
    /// COPY 等の低レベル操作用。
    /// </summary>
    /// <param name="services">サービスコレクション。</param>
    /// <param name="configuration">接続文字列を取得するための構成インスタンス。</param>
    /// <param name="connectionStringName">接続文字列名。</param>
    /// <param name="configure">
    /// <see cref="NpgsqlDataSourceBuilder"/> に対する追加のカスタマイズ。null の場合は無視されます。
    /// </param>
    /// <returns>チェーン呼び出し可能なサービスコレクション。</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/>、<paramref name="configuration"/>、
    /// または <paramref name="connectionStringName"/> が null の場合にスローされます。
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// 指定された接続文字列名に対応する接続文字列が見つからない場合にスローされます。
    /// </exception>
    public static IServiceCollection AddNpgsqlDataSource(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName,
        Action<NpgsqlDataSourceBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(connectionStringName);

        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException(
                $"接続文字列 '{connectionStringName}' が構成に見つかりません。");

        services.AddSingleton(_ =>
        {
            var builder = new NpgsqlDataSourceBuilder(connectionString);
            configure?.Invoke(builder);
            return builder.Build();
        });

        return services;
    }

    /// <summary>
    /// 現在の環境が Development かどうかを判定します。
    /// <see cref="IHostEnvironment"/> が DI に登録されていない場合は環境変数にフォールバックします。
    /// </summary>
    /// <param name="sp">サービスプロバイダー。</param>
    /// <returns>Development 環境の場合は true。</returns>
    private static bool IsDevelopment(IServiceProvider sp)
    {
        var hostEnvironment = sp.GetService<IHostEnvironment>();
        if (hostEnvironment != null)
        {
            return hostEnvironment.IsDevelopment();
        }

        var envName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase);
    }
}
